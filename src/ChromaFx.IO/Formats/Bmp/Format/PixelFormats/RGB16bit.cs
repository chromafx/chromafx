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

using ChromaFx.Core.ExtensionMethods;
using ChromaFx.IO.Formats.Bmp.Format.PixelFormats.BaseClasses;
using ChromaFx.IO.Formats.Bmp.Format.PixelFormats.Interfaces;

namespace ChromaFx.IO.Formats.Bmp.Format.PixelFormats;

/// <summary>
/// RGB 16bit pixel format
/// </summary>
/// <seealso cref="IPixelFormat"/>
public class Rgb16Bit : PixelFormatBase
{
    /// <summary>
    /// The bytes per pixel
    /// </summary>
    /// <value>The BPP.</value>
    public override double Bpp => 2;

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
        Parallel.For(0, height, y =>
        {
            var rowOffset = y * (width * (int)Bpp + alignment);
            var currentY = height - y - 1;
            if (currentY < 0)
                currentY = 0;
            if (currentY >= height)
                currentY = height - 1;
            for (var x = 0; x < width; ++x)
            {
                var offset = rowOffset + x * (int)Bpp;
                var tempValue = BitConverter.ToInt16(data, offset);
                var r = (int)(((tempValue & header.RedMask) >> header.RedOffset) * header.RedMultiplier);
                var g = (int)(((tempValue & header.GreenMask) >> header.GreenOffset) * header.GreenMultiplier);
                var b = (int)(((tempValue & header.BlueMask) >> header.BlueOffset) * header.BlueMultiplier);
                var a = (int)(header.AlphaMask == 0 ? 255 : ((tempValue & header.AlphaMask) >> header.AlphaOffset) * header.AlphaMultiplier);

                var arrayOffset = (currentY * width + x) * 4;
                returnValue[arrayOffset] = r.ToByte();
                returnValue[arrayOffset + 1] = g.ToByte();
                returnValue[arrayOffset + 2] = b.ToByte();
                returnValue[arrayOffset + 3] = a.ToByte();
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
        var width = header.Width;
        var height = header.Height;
        var alignment = (4 - width * (int)Bpp % 4) % 4;
        var returnValue = new byte[(width * (int)Bpp + alignment) * height];
        Parallel.For(0, height, y =>
        {
            var sourceY = height - y - 1;
            if (sourceY < 0)
                sourceY = 0;
            if (sourceY >= height)
                sourceY = height - 1;
            var sourceRowOffset = sourceY * width * 4;
            var destinationRowOffset = y * (width * (int)Bpp + alignment);
            for (var x = 0; x < width; ++x)
            {
                var sourceX = x * 4;
                var sourceOffset = sourceX + sourceRowOffset;
                var destinationX = x * (int)Bpp;
                var destinationOffset = destinationX + destinationRowOffset;
                var r = data[sourceOffset + 2] >> 3;
                var g = data[sourceOffset + 1] >> 2;
                var b = data[sourceOffset] >> 3;
                var tempValue = (short)((r << 11) | (g << 5) | b);
                var values = BitConverter.GetBytes(tempValue);

                returnValue[destinationOffset] = values[0];
                returnValue[destinationOffset + 1] = values[1];
            }
        });
        return returnValue;
    }
}