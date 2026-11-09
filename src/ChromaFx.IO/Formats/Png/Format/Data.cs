/*
 * Copyright 2017–2020 JaCraig
 * Modifications Copyright 2023–2026 Ho Tzin Mein
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Buffers;
using System.IO.Compression;
using ChromaFx.Core;
using ChromaFx.Core.Simd;
using ChromaFx.IO.Formats.Png.Format.ColorFormats;
using ChromaFx.IO.Formats.Png.Format.Enums;
using ChromaFx.IO.Formats.Png.Format.Helpers;

namespace ChromaFx.IO.Formats.Png.Format;

/// <summary>
/// PNG image data
/// </summary>
internal class Data
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Data"/> class.
    /// </summary>
    /// <param name="image">The image.</param>
    public Data(Image image)
        : this(ToScanlines(image)) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Data"/> class.
    /// </summary>
    /// <param name="imageData">The image data.</param>
    public Data(byte[] imageData)
    {
        ImageData = imageData;
        ColorTypes = new Dictionary<ColorType, ColorTypeInformation>
        {
            [ColorType.Greyscale] = new(1, [1, 2, 4, 8], (_, _) => new GreyscaleNoAlphaReader()),
            [ColorType.TrueColor] = new(3, [8], (_, _) => new TrueColorNoAlphaReader()),
            [ColorType.Palette] = new(1, [1, 2, 4, 8], (x, y) => new PaletteReader(x, y)),
            [ColorType.GreyscaleWithAlpha] = new(2, [8], (_, _) => new GreyscaleAlphaReader()),
            [ColorType.TrueColorWithAlpha] = new(4, [8], (_, _) => new TrueColorAlphaReader()),
        };
    }

    /// <summary>
    /// Gets or sets the image data.
    /// </summary>
    /// <value>The image data.</value>
    public byte[] ImageData { get; set; }

    /// <summary>
    /// Gets or sets the color types.
    /// </summary>
    /// <value>The color types.</value>
    private Dictionary<ColorType, ColorTypeInformation> ColorTypes { get; }

    /// <summary>
    /// Performs an implicit conversion from <see cref="Data"/> to <see cref="Chunk"/>.
    /// </summary>
    /// <param name="data">The data.</param>
    /// <returns>The result of the conversion.</returns>
    public static implicit operator Chunk(Data data)
    {
        return new Chunk(data.ImageData.Length, ChunkTypes.Data, data.ImageData);
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="Chunk"/> to <see cref="Data"/>.
    /// </summary>
    /// <param name="chunk">The chunk.</param>
    /// <returns>The result of the conversion.</returns>
    public static implicit operator Data(Chunk chunk)
    {
        return new Data(chunk.Data);
    }

    /// <summary>
    /// Implements the operator +.
    /// </summary>
    /// <param name="object1">The object1.</param>
    /// <param name="object2">The object2.</param>
    /// <returns>The result of the operator.</returns>
    public static Data operator +(Data object1, Data object2)
    {
        if (object1 == null && object2 == null)
            return new Data([]);
        if (object1 == null)
            return new Data(object2.ImageData);
        if (object2 == null)
            return new Data(object1.ImageData);
        // Use MemoryStream to efficiently concatenate byte arrays
        using var ms = new MemoryStream(object1.ImageData.Length + object2.ImageData.Length);
        ms.Write(object1.ImageData, 0, object1.ImageData.Length);
        ms.Write(object2.ImageData, 0, object2.ImageData.Length);
        return new Data(ms.ToArray());
    }

    /// <summary>
    /// Parses the specified header.
    /// </summary>
    /// <param name="header">The header.</param>
    /// <param name="palette">The palette.</param>
    /// <param name="alphaPalette">The alpha palette.</param>
    /// <returns>The resulting image</returns>
    public Image Parse(Header header, Palette palette, Palette alphaPalette)
    {
        var image = new Image(header.Width, header.Height);
        var colorTypeInfo = ColorTypes[header.ColorType];

        if (colorTypeInfo == null)
            return image;

        var colorReader = colorTypeInfo.CreateColorReader(palette, alphaPalette);
        using var tempStream = new MemoryStream(ImageData);
        ReadScanlines(tempStream, image, colorReader, colorTypeInfo, header);

        return image;
    }

    /// <summary>
    /// Calculates the length of the scanline.
    /// </summary>
    /// <param name="colorTypeInformation">The color type information.</param>
    /// <param name="header">The header.</param>
    /// <returns>The scanline length</returns>
    private static int CalculateScanlineLength(
        ColorTypeInformation colorTypeInformation,
        Header header
    )
    {
        var scanLineLength = header.Width * header.BitDepth * colorTypeInformation.ScanlineFactor;

        var amount = scanLineLength % 8;
        if (amount != 0)
        {
            scanLineLength += 8 - amount;
        }

        return scanLineLength / 8;
    }

    /// <summary>
    /// Calculates the scanline step.
    /// </summary>
    /// <param name="colorTypeInformation">The color type information.</param>
    /// <param name="header">The header.</param>
    /// <returns>The scanline step</returns>
    private static int CalculateScanLineStep(
        ColorTypeInformation colorTypeInformation,
        Header header
    )
    {
        return header.BitDepth >= 8 ? colorTypeInformation.ScanlineFactor * header.BitDepth / 8 : 1;
    }

    internal static byte[] ToScanlines(Image image)
    {
        var rowLength = image.Width * 4 + 1;
        var rowBytes = image.Width * 4;

        using var ms = new MemoryStream();
        var prevRow = ArrayPool<byte>.Shared.Rent(rowBytes);
        var curRow = ArrayPool<byte>.Shared.Rent(rowBytes);
        var filteredRow = ArrayPool<byte>.Shared.Rent(rowLength);
        var bestRow = ArrayPool<byte>.Shared.Rent(rowLength);
        try
        {
            prevRow.AsSpan(0, rowBytes).Clear();
            using var compressor = new ZLibStream(ms, CompressionLevel.Optimal, leaveOpen: true);
            for (int y = 0; y < image.Height; y++)
            {
                // Row-wise: storage may include SIMD padding beyond Width.
                System.Runtime.InteropServices.MemoryMarshal.AsBytes(image.GetRowSpan(y))
                    .CopyTo(curRow.AsSpan(0, rowBytes));

                // Score None / Sub / Up only (Average/Paeth rarely win enough to offset cost).
                int bestScore = int.MaxValue;
                for (int filter = 0; filter <= 2; filter++)
                {
                    ApplyFilter(
                        filter,
                        curRow.AsSpan(0, rowBytes),
                        prevRow.AsSpan(0, rowBytes),
                        filteredRow.AsSpan(0, rowLength)
                    );
                    int score = Score(filteredRow.AsSpan(0, rowLength));
                    if (score < bestScore)
                    {
                        bestScore = score;
                        filteredRow.AsSpan(0, rowLength).CopyTo(bestRow);
                    }
                }

                compressor.Write(bestRow, 0, rowLength);
                (curRow, prevRow) = (prevRow, curRow);
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(prevRow);
            ArrayPool<byte>.Shared.Return(curRow);
            ArrayPool<byte>.Shared.Return(filteredRow);
            ArrayPool<byte>.Shared.Return(bestRow);
        }

        return ms.ToArray();
    }

    private static void ApplyFilter(
        int filter,
        ReadOnlySpan<byte> cur,
        ReadOnlySpan<byte> prev,
        Span<byte> output
    )
    {
        output[0] = (byte)filter;
        var dst = output[1..];
        switch (filter)
        {
            case 0: // None
                cur.CopyTo(dst);
                break;
            case 1: // Sub
                ByteSimd.SubPredictSubtract(cur, dst, bytesPerPixel: 4);
                break;
            case 2: // Up
                ByteSimd.SubtractWrapping(cur, prev, dst);
                break;
            case 3: // Average
                ByteSimd.AveragePredictSubtract(cur, prev, dst, bytesPerPixel: 4);
                break;
            case 4: // Paeth
                ByteSimd.PaethPredictSubtract(cur, prev, dst, bytesPerPixel: 4);
                break;
        }
    }

    private static int Score(ReadOnlySpan<byte> row)
    {
        // Heuristic: sum of absolute values of filtered bytes
        int sum = 0;
        for (int i = 1; i < row.Length; i++)
            sum += (row[i] < 128 ? row[i] : 256 - row[i]); // abs as unsigned
        return sum;
    }

    /// <summary>
    /// Reads the scanlines.
    /// </summary>
    /// <param name="dataStream">The data stream.</param>
    /// <param name="image">The destination image.</param>
    /// <param name="colorReader">The color reader.</param>
    /// <param name="colorTypeInformation">The color type information.</param>
    /// <param name="header">The header.</param>
    private static void ReadScanlines(
        MemoryStream dataStream,
        Image image,
        IColorReader colorReader,
        ColorTypeInformation colorTypeInformation,
        Header header
    )
    {
        dataStream.Seek(0, SeekOrigin.Begin);

        var scanLineLength = CalculateScanlineLength(colorTypeInformation, header);
        var scanLineStep = CalculateScanLineStep(colorTypeInformation, header);

        var lastScanLine = ArrayPool<byte>.Shared.Rent(scanLineLength);
        var currentScanLine = ArrayPool<byte>.Shared.Rent(scanLineLength);
        var rowBuffer = ArrayPool<byte>.Shared.Rent(1 + scanLineLength);
        try
        {
            lastScanLine.AsSpan(0, scanLineLength).Clear();
            using var zlib = new ZLibStream(dataStream, CompressionMode.Decompress);

            for (int row = 0; row < header.Height; row++)
            {
                zlib.ReadExactly(rowBuffer.AsSpan(0, 1 + scanLineLength));
                int filter = rowBuffer[0];
                rowBuffer.AsSpan(1, scanLineLength).CopyTo(currentScanLine);

                switch (filter)
                {
                    case 1:
                        ByteSimd.SubPredictAddInPlace(
                            currentScanLine.AsSpan(0, scanLineLength),
                            scanLineStep
                        );
                        break;
                    case 2:
                        ByteSimd.AddWrapping(
                            currentScanLine.AsSpan(0, scanLineLength),
                            lastScanLine.AsSpan(0, scanLineLength),
                            currentScanLine.AsSpan(0, scanLineLength)
                        );
                        break;
                    case 3:
                        ByteSimd.AveragePredictAddInPlace(
                            currentScanLine.AsSpan(0, scanLineLength),
                            lastScanLine.AsSpan(0, scanLineLength),
                            scanLineStep
                        );
                        break;
                    case 4:
                        ByteSimd.PaethPredictAddInPlace(
                            currentScanLine.AsSpan(0, scanLineLength),
                            lastScanLine.AsSpan(0, scanLineLength),
                            scanLineStep
                        );
                        break;
                }

                colorReader.ReadScanline(
                    currentScanLine.AsSpan(0, scanLineLength),
                    image.GetRowSpan(row),
                    header
                );
                (lastScanLine, currentScanLine) = (currentScanLine, lastScanLine);
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(lastScanLine);
            ArrayPool<byte>.Shared.Return(currentScanLine);
            ArrayPool<byte>.Shared.Return(rowBuffer);
        }
    }
}
