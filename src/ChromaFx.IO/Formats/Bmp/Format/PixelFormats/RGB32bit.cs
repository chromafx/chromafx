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

namespace ChromaFx.IO.Formats.Bmp.Format.PixelFormats;

/// <summary>
/// RGB 32bit pixel format
/// </summary>
/// <seealso cref="Interfaces.IPixelFormat"/>
internal class Rgb32Bit : PixelFormatBase
{
    /// <summary>
    /// The bytes per pixel
    /// </summary>
    /// <value>The BPP.</value>
    public override double Bpp => 4;

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
                var sourceRowOffset = sourceY * (width * (int)Bpp + alignment);
                var destinationRowOffset = y * width * 4;
                var dest = MemoryMarshal.Cast<byte, uint>(
                    returnValue.AsSpan(destinationRowOffset, width * 4)
                );
                var opaque = header.AlphaMask == 0;
                for (var x = 0; x < width; ++x)
                {
                    var sourceOffset = sourceRowOffset + x * 4;
                    var r = data[sourceOffset + 3];
                    var g = data[sourceOffset + 2];
                    var b = data[sourceOffset + 1];
                    var a = opaque ? (byte)255 : data[sourceOffset];
                    dest[x] = r | ((uint)g << 8) | ((uint)b << 16) | ((uint)a << 24);
                }
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
                var destinationRowOffset = y * (width * (int)Bpp + alignment);
                var src = MemoryMarshal.Cast<byte, uint>(
                    data.AsSpan(sourceRowOffset, width * 4)
                );
                for (var x = 0; x < width; ++x)
                {
                    // RGBA → BGRX (alpha discarded, same as prior encode).
                    var pixel = src[x];
                    var r = (byte)pixel;
                    var g = (byte)(pixel >> 8);
                    var b = (byte)(pixel >> 16);
                    var destinationOffset = destinationRowOffset + x * 4;
                    returnValue[destinationOffset] = b;
                    returnValue[destinationOffset + 1] = g;
                    returnValue[destinationOffset + 2] = r;
                }
            }
        );
        return returnValue;
    }
}
