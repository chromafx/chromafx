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

using System.Runtime.InteropServices;
using ChromaFx.Core;

namespace ChromaFx.IO.Formats.Bmp.Format.PixelFormats;

/// <summary>
/// RGB 24bit pixel format
/// </summary>
/// <seealso cref="IPixelFormat"/>
internal class Rgb24Bit : PixelFormatBase
{
    /// <summary>
    /// The bytes per pixel
    /// </summary>
    /// <value>The BPP.</value>
    public override double Bpp => 3;

    /// <summary>
    /// Decodes the specified data.
    /// </summary>
    /// <param name="header">The header.</param>
    /// <param name="data">The data.</param>
    /// <param name="palette">The palette.</param>
    /// <returns>The decoded data</returns>
    public override byte[] Decode(Header header, byte[] data, Palette palette)
    {
        var width = header.Width;
        var height = header.Height;
        var alignment = (4 - width * (int)Bpp % 4) % 4;
        var returnValue = new byte[width * height * 4];
        var rowBytes = width * (int)Bpp + alignment;

        Parallel.For(
            0,
            height,
            y =>
            {
                var sourceY = height - y - 1;
                if (sourceY < 0)
                    sourceY = 0;
                if (sourceY >= height)
                    sourceY = height - 1;

                var sourceRowOffset = sourceY * rowBytes;
                var destinationRowOffset = y * width * 4;
                var dest = MemoryMarshal.Cast<byte, uint>(
                    returnValue.AsSpan(destinationRowOffset, width * 4)
                );

                var x = 0;
                for (; x <= width - 4; x += 4)
                {
                    var o = sourceRowOffset + x * 3;
                    dest[x] = PackBgr(data, o);
                    dest[x + 1] = PackBgr(data, o + 3);
                    dest[x + 2] = PackBgr(data, o + 6);
                    dest[x + 3] = PackBgr(data, o + 9);
                }

                for (; x < width; ++x)
                    dest[x] = PackBgr(data, sourceRowOffset + x * 3);
            }
        );

        return returnValue;
    }

    /// <summary>
    /// Encodes the specified data.
    /// </summary>
    /// <param name="header">The header.</param>
    /// <param name="data">The data.</param>
    /// <param name="palette">The palette.</param>
    /// <returns>The encoded data</returns>
    public override byte[] Encode(Header header, byte[] data, Palette palette)
    {
        var width = header.Width;
        var height = header.Height;
        var alignment = (4 - width * (int)Bpp % 4) % 4;
        var returnValue = new byte[(width * (int)Bpp + alignment) * height];
        var rowBytes = width * (int)Bpp + alignment;

        Parallel.For(
            0,
            height,
            y =>
            {
                var sourceY = height - y - 1;
                if (sourceY < 0)
                    sourceY = 0;
                if (sourceY >= height)
                    sourceY = height - 1;

                var sourceRowOffset = sourceY * width * 4;
                var destinationRowOffset = y * rowBytes;

                var x = 0;
                for (; x <= width - 4; x += 4)
                {
                    var s = sourceRowOffset + x * 4;
                    var d = destinationRowOffset + x * 3;
                    UnpackRgba(data, s, returnValue, d);
                    UnpackRgba(data, s + 4, returnValue, d + 3);
                    UnpackRgba(data, s + 8, returnValue, d + 6);
                    UnpackRgba(data, s + 12, returnValue, d + 9);
                }

                for (; x < width; ++x)
                    UnpackRgba(
                        data,
                        sourceRowOffset + x * 4,
                        returnValue,
                        destinationRowOffset + x * 3
                    );
            }
        );

        return returnValue;
    }

    /// <summary>
    /// Encodes an image's logical rows directly to BMP BGR24 (skips an intermediate RGBA buffer).
    /// </summary>
    public byte[] Encode(Header header, Image image)
    {
        var width = header.Width;
        var height = header.Height;
        var alignment = (4 - width * (int)Bpp % 4) % 4;
        var returnValue = new byte[(width * (int)Bpp + alignment) * height];
        var rowBytes = width * (int)Bpp + alignment;

        Parallel.For(
            0,
            height,
            y =>
            {
                var sourceY = height - y - 1;
                if (sourceY < 0)
                    sourceY = 0;
                if (sourceY >= height)
                    sourceY = height - 1;

                var sourceRow = image.GetRowSpan(sourceY);
                var destinationRowOffset = y * rowBytes;

                for (var x = 0; x < width; ++x)
                {
                    var d = destinationRowOffset + x * 3;
                    var pixel = sourceRow[x];
                    returnValue[d] = pixel.Blue;
                    returnValue[d + 1] = pixel.Green;
                    returnValue[d + 2] = pixel.Red;
                }
            }
        );

        return returnValue;
    }

    /// <summary>BGR → packed RGBA little-endian (<see cref="Color"/> layout) with opaque alpha.</summary>
    private static uint PackBgr(byte[] data, int offset) =>
        data[offset + 2]
        | ((uint)data[offset + 1] << 8)
        | ((uint)data[offset] << 16)
        | 0xFF00_0000u;

    private static void UnpackRgba(byte[] rgba, int src, byte[] bgr, int dst)
    {
        bgr[dst] = rgba[src + 2];
        bgr[dst + 1] = rgba[src + 1];
        bgr[dst + 2] = rgba[src];
    }
}
