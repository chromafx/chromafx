/*
 * Copyright 2017–2020 JaCraig
 * Modifications Copyright 2023–2025 Ho Tzin Mein
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

using ChromaFx.IO.Formats.Bmp.Format.PixelFormats.BaseClasses;
using ChromaFx.IO.Formats.Bmp.Format.PixelFormats.Interfaces;

namespace ChromaFx.IO.Formats.Bmp.Format.PixelFormats;

/// <summary>
/// RGB 8bit pixel format
/// </summary>
/// <seealso cref="IPixelFormat"/>
public class Rgb8Bit : PixelFormatBase
{
    /// <summary>
    /// The bytes per pixel
    /// </summary>
    /// <value>The BPP.</value>
    public override double Bpp => 1;

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
        var alignment = (4 - width % 4) % 4;
        var returnValue = new byte[width * height * 4];
        Parallel.For(0, height, y =>
        {
            var sourceY = y * (width + alignment);
            var destinationY = height - y - 1;
            var sourceOffset = sourceY;
            var destinationOffset = destinationY * width * 4;
            for (var x = 0; x < width; ++x)
            {
                var colorIndex = data[sourceOffset] * 4;
                returnValue[destinationOffset] = palette.Data[colorIndex + 2];
                returnValue[destinationOffset + 1] = palette.Data[colorIndex + 1];
                returnValue[destinationOffset + 2] = palette.Data[colorIndex];
                returnValue[destinationOffset + 3] = palette.Data[colorIndex + 3];
                destinationOffset += 4;
                ++sourceOffset;
            }
        });
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
        return data;
    }
}