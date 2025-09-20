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

using ChromaFx.Formats.Png.Format.ColorFormats;
using ChromaFx.Formats.Png.Format.ColorFormats.Interfaces;
using ChromaFx.Formats.Png.Format.Enums;
using ChromaFx.Formats.Png.Format.Filters;
using ChromaFx.Formats.Png.Format.Filters.Interfaces;
using ChromaFx.Formats.Png.Format.Helpers;
using ChromaFx.Colors;
using System.IO.Compression;

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
                new[] { 1, 2, 4, 8 },
                (_, _) => new GreyscaleNoAlphaReader()
            ),
            [ColorType.TrueColor] = new(3, new[] { 8 }, (_, _) => new TrueColorNoAlphaReader()),
            [ColorType.Palette] = new(
                1,
                new[] { 1, 2, 4, 8 },
                (x, y) => new PaletteReader(x, y)
            ),
            [ColorType.GreyscaleWithAlpha] = new(
                2,
                new[] { 8 },
                (_, _) => new GreyscaleAlphaReader()
            ),
            [ColorType.TrueColorWithAlpha] = new(
                4,
                new[] { 8 },
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
            return new Data(Array.Empty<byte>());
        if (object1 == null)
            return new Data(object2.ImageData);
        if (object2 == null)
            return new Data(object1.ImageData);
        var returnData = new Data(
            new byte[object1.ImageData.Length + object2.ImageData.Length]
        );
        Array.Copy(object1.ImageData, 0, returnData.ImageData, 0, object1.ImageData.Length);
        Array.Copy(
            object2.ImageData,
            0,
            returnData.ImageData,
            object1.ImageData.Length,
            object2.ImageData.Length
        );
        return returnData;
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

    private static byte[] ToScanlines(Image image)
    {
        var data = new byte[image.Width * image.Height * 4 + image.Height];
        var rowLength = image.Width * 4 + 1;

        Parallel.For(
            0,
            image.Width,
            x =>
            {
                var dataOffset = x * 4 + 1;
                var pixelOffset = x;
                data[dataOffset] = image.Pixels[pixelOffset].Red;
                data[dataOffset + 1] = image.Pixels[pixelOffset].Green;
                data[dataOffset + 2] = image.Pixels[pixelOffset].Blue;
                data[dataOffset + 3] = image.Pixels[pixelOffset].Alpha;
                data[0] = 0;
                for (var y = 1; y < image.Height; ++y)
                {
                    dataOffset = y * rowLength + x * 4 + 1;
                    pixelOffset = image.Width * y + x;
                    var abovePixelOffset = image.Width * (y - 1) + x;
                    data[dataOffset] = (byte)(
                        image.Pixels[pixelOffset].Red - image.Pixels[abovePixelOffset].Red
                    );
                    data[dataOffset + 1] = (byte)(
                        image.Pixels[pixelOffset].Green - image.Pixels[abovePixelOffset].Green
                    );
                    data[dataOffset + 2] = (byte)(
                        image.Pixels[pixelOffset].Blue - image.Pixels[abovePixelOffset].Blue
                    );
                    data[dataOffset + 3] = (byte)(
                        image.Pixels[pixelOffset].Alpha - image.Pixels[abovePixelOffset].Alpha
                    );
                    data[y * rowLength] = 2;
                }
            }
        );

        using var tempMemoryStream = new MemoryStream();
        using (var tempDeflateStream = new ZLibStream(tempMemoryStream, CompressionMode.Compress))
        {
            tempDeflateStream.Write(data, 0, data.Length);
        }
        return tempMemoryStream.ToArray();
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
        Stream dataStream,
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
        //using (var compressedStream = new ZLibStream(dataStream, CompressionMode.Decompress))
        //{
        //    using (MemoryStream DecompressedStream = new MemoryStream())
        //    {
        //        compressedStream.CopyTo(DecompressedStream);
        //        DecompressedStream.Flush();
        //        byte[] DecompressedArray = DecompressedStream.ToArray();
        //        for (int y = 0, Column = 0; y < header.Height; ++y, Column += (scanLineLength + 1))
        //        {
        //            Array.Copy(DecompressedArray, Column + 1, currentScanLine, 0, scanLineLength);
        //            if (DecompressedArray[Column] < 0)
        //                break;
        //            byte[] Result = Filters[(FilterType)DecompressedArray[Column]].Decode(currentScanLine, lastScanLine, scanLineStep);
        //            colorReader.ReadScanline(Result, pixels, header, y);
        //            Array.Copy(currentScanLine, lastScanLine, scanLineStep);
        //        }
        //    }
        //}
    }
}