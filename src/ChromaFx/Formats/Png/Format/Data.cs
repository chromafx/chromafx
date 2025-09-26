/*
Copyright 2025 Ho Tzin Mein

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using ChromaFx.Colors;
using ChromaFx.Formats.Png.Format.ColorFormats;
using ChromaFx.Formats.Png.Format.ColorFormats.Interfaces;
using ChromaFx.Formats.Png.Format.Enums;
using ChromaFx.Formats.Png.Format.Filters;
using ChromaFx.Formats.Png.Format.Filters.Interfaces;
using ChromaFx.Formats.Png.Format.Helpers;
using System.IO.Compression;
using System.Numerics;

namespace ChromaFx.Formats.Png.Format;

/// <summary>
/// PNG image data
/// </summary>
public class Data
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
            [ColorType.Greyscale] = new(
                1,
                [1, 2, 4, 8],
                (_, _) => new GreyscaleNoAlphaReader()
            ),
            [ColorType.TrueColor] = new(3, [8], (_, _) => new TrueColorNoAlphaReader()),
            [ColorType.Palette] = new(
                1,
                [1, 2, 4, 8],
                (x, y) => new PaletteReader(x, y)
            ),
            [ColorType.GreyscaleWithAlpha] = new(
                2,
                [8],
                (_, _) => new GreyscaleAlphaReader()
            ),
            [ColorType.TrueColorWithAlpha] = new(
                4,
                [8],
                (_, _) => new TrueColorAlphaReader()
            )
        };
        Filters = new Dictionary<FilterType, IScanFilter>
        {
            [FilterType.Average] = new AverageFilter(),
            [FilterType.None] = new NoFilter(),
            [FilterType.Paeth] = new PaethFilter(),
            [FilterType.Sub] = new SubFilter(),
            [FilterType.Up] = new UpFilter()
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
    /// Gets or sets the filters.
    /// </summary>
    /// <value>The filters.</value>
    private Dictionary<FilterType, IScanFilter> Filters { get; set; }

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
        var pixels = new Color[header.Width * header.Height];
        var colorTypeInfo = ColorTypes[header.ColorType];

        if (colorTypeInfo == null)
            return new Image(header.Width, header.Height, pixels);

        var colorReader = colorTypeInfo.CreateColorReader(palette, alphaPalette);
        using var tempStream = new MemoryStream(ImageData);
        ReadScanlines(tempStream, pixels, colorReader, colorTypeInfo, header);

        return new Image(header.Width, header.Height, pixels);
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
        var scanLineLength =
            header.Width * header.BitDepth * colorTypeInformation.ScanlineFactor;

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
        return header.BitDepth >= 8
            ? colorTypeInformation.ScanlineFactor * header.BitDepth / 8
            : 1;
    }

    /// <summary>
    /// Paethes the predicator.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="above">The above.</param>
    /// <param name="upperLeft">The upper left.</param>
    /// <returns>The predicted paeth...</returns>
    private static byte PaethPredicator(byte left, byte above, byte upperLeft)
    {
        var p = left + above - upperLeft;
        var pa = Math.Abs(p - left);
        var pb = Math.Abs(p - above);
        var pc = Math.Abs(p - upperLeft);
        if (pa <= pb && pa <= pc)
        {
            return left;
        }
        return pb <= pc ? above : upperLeft;
    }

    internal static byte[] ToScanlines(Image image)
    {
        var rowLength = image.Width * 4 + 1;

        using var ms = new MemoryStream();
        using (var compressor = new ZLibStream(ms, CompressionLevel.Optimal, leaveOpen: true))
        {
            var prevRow = new byte[image.Width * 4];
            var curRow = new byte[image.Width * 4];
            var filteredRow = new byte[rowLength];

            for (int y = 0; y < image.Height; y++)
            {
                // Fill current row (raw RGBA)
                for (int x = 0; x < image.Width; x++)
                {
                    var pixel = image.Pixels[y * image.Width + x];
                    int offset = x * 4;
                    curRow[offset] = pixel.Red;
                    curRow[offset + 1] = pixel.Green;
                    curRow[offset + 2] = pixel.Blue;
                    curRow[offset + 3] = pixel.Alpha;
                }

                // Try all filters (0–4), keep the one with lowest "cost"
                int bestFilter = 0;
                int bestScore = int.MaxValue;

                for (int filter = 0; filter <= 4; filter++)
                {
                    ApplyFilter(filter, curRow, prevRow, filteredRow);
                    int score = Score(filteredRow);
                    if (score < bestScore)
                    {
                        bestScore = score;
                        bestFilter = filter;
                    }
                }

                // Write best row
                ApplyFilter(bestFilter, curRow, prevRow, filteredRow);
                compressor.Write(filteredRow, 0, filteredRow.Length);

                // Swap rows
                var tmp = prevRow;
                prevRow = curRow;
                curRow = tmp;
            }
        }

        return ms.ToArray();
    }

    private static void ApplyFilter(int filter, byte[] cur, byte[] prev, byte[] output)
    {
        output[0] = (byte)filter;
        switch (filter)
        {
            case 0: // None
                Buffer.BlockCopy(cur, 0, output, 1, cur.Length);
                break;
            case 1: // Sub
                ApplySub(cur, output.AsSpan(1));
                break;
            case 2: // Up
                ApplyUp(cur, prev, output.AsSpan(1));
                break;
            case 3: // Average
                ApplyAverage(cur, prev, output.AsSpan(1));
                break;
            case 4: // Paeth
                ApplyPaeth(cur, prev, output.AsSpan(1));
                break;
        }
    }

    private static void ApplySub(byte[] cur, Span<byte> dst)
    {
        int bpp = 4; // RGBA
        for (int i = 0; i < cur.Length; i++)
        {
            byte left = i >= bpp ? cur[i - bpp] : (byte)0;
            dst[i] = (byte)(cur[i] - left);
        }
    }

    private static void ApplyUp(byte[] cur, byte[] prev, Span<byte> dst)
    {
        // Vectorized Up filter: cur - prev
        int i = 0;
        int simdLength = Vector<byte>.Count;

        for (; i <= cur.Length - simdLength; i += simdLength)
        {
            var vCur = new Vector<byte>(cur, i);
            var vPrev = new Vector<byte>(prev, i);
            var vRes = vCur - vPrev;
            vRes.CopyTo(dst.Slice(i));
        }

        for (; i < cur.Length; i++)
            dst[i] = (byte)(cur[i] - prev[i]);
    }

    private static void ApplyAverage(byte[] cur, byte[] prev, Span<byte> dst)
    {
        int bpp = 4;
        for (int i = 0; i < cur.Length; i++)
        {
            byte left = i >= bpp ? cur[i - bpp] : (byte)0;
            byte up = prev[i];
            dst[i] = (byte)(cur[i] - ((left + up) >> 1));
        }
    }

    private static void ApplyPaeth(byte[] cur, byte[] prev, Span<byte> dst)
    {
        int bpp = 4;
        for (int i = 0; i < cur.Length; i++)
        {
            byte a = i >= bpp ? cur[i - bpp] : (byte)0; // left
            byte b = prev[i];                           // up
            byte c = i >= bpp ? prev[i - bpp] : (byte)0; // up-left

            int p = a + b - c;
            int pa = Math.Abs(p - a);
            int pb = Math.Abs(p - b);
            int pc = Math.Abs(p - c);

            byte predictor = (pa <= pb && pa <= pc) ? a : (pb <= pc ? b : c);
            dst[i] = (byte)(cur[i] - predictor);
        }
    }

    private static int Score(byte[] row)
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
    /// <param name="pixels">The pixels.</param>
    /// <param name="colorReader">The color reader.</param>
    /// <param name="colorTypeInformation">The color type information.</param>
    /// <param name="header">The header.</param>
    private static void ReadScanlines(
        MemoryStream dataStream,
        Color[] pixels,
        IColorReader colorReader,
        ColorTypeInformation colorTypeInformation,
        Header header
    )
    {
        dataStream.Seek(0, SeekOrigin.Begin);

        var scanLineLength = CalculateScanlineLength(colorTypeInformation, header);
        var scanLineStep = CalculateScanLineStep(colorTypeInformation, header);

        var lastScanLine = new byte[scanLineLength];
        var currentScanLine = new byte[scanLineLength];

        int filter = 0,
            column = -1,
            row = 0;

        using var decompressedStream = new ZLibStream(dataStream, CompressionMode.Decompress);
        using var stream = new MemoryStream();
        decompressedStream.CopyTo(stream);
        stream.Flush();
        var decompressedArray = stream.ToArray();
        foreach (var by in decompressedArray)
        {
            if (column == -1)
            {
                filter = by;
                ++column;
            }
            else
            {
                currentScanLine[column] = by;
                byte a;
                byte c;
                if (column >= scanLineStep)
                {
                    a = currentScanLine[column - scanLineStep];
                    c = lastScanLine[column - scanLineStep];
                }
                else
                {
                    a = 0;
                    c = 0;
                }

                var b = lastScanLine[column];
                currentScanLine[column] = filter switch
                {
                    1 => (byte)(currentScanLine[column] + a),
                    2 => (byte)(currentScanLine[column] + b),
                    3 => (byte)(currentScanLine[column] + (byte)((a + b) / 2)),
                    4 => (byte)(currentScanLine[column] + PaethPredicator(a, b, c)),
                    _ => currentScanLine[column]
                };

                ++column;

                if (column == scanLineLength)
                {
                    colorReader.ReadScanline(currentScanLine, pixels, header, row);
                    ++row;
                    column = -1;
                    (lastScanLine, currentScanLine) = (currentScanLine, lastScanLine);
                }
            }
        }
    }
}