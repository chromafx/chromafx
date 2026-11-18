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
using ChromaFx.Core.Colors;
using ChromaFx.Core.Simd;

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
        var returnValue = new byte[width * height * 4];
        var rowBytes = Stride(width, 24);

        Parallel.For(
            0,
            height,
            y =>
            {
                var sourceY = SourceRow(header, y);

                var sourceRowOffset = sourceY * rowBytes;
                var destinationRowOffset = y * width * 4;
                var dest = MemoryMarshal.Cast<byte, uint>(
                    returnValue.AsSpan(destinationRowOffset, width * 4)
                );
                ByteSimd.PackBgr8ToRgba(data.AsSpan(sourceRowOffset, width * 3), dest);
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
                var src = MemoryMarshal.Cast<byte, uint>(data.AsSpan(sourceRowOffset, width * 4));
                ByteSimd.UnpackRgbaToBgr8(
                    src,
                    returnValue.AsSpan(destinationRowOffset, width * 3)
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

                var sourceRow = MemoryMarshal.Cast<Color, uint>(image.GetRowSpan(sourceY));
                ByteSimd.UnpackRgbaToBgr8(
                    sourceRow,
                    returnValue.AsSpan(y * rowBytes, width * 3)
                );
            }
        );

        return returnValue;
    }
}
