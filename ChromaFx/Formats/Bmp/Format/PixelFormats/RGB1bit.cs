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

using ChromaFx.Formats.Bmp.Format.PixelFormats.BaseClasses;
using ChromaFx.IO;

namespace ChromaFx.Formats.Bmp.Format.PixelFormats;

/// <summary>
/// RGB 1bit pixel format
/// </summary>
/// <seealso cref="Interfaces.IPixelFormat"/>
public class Rgb1Bit : PixelFormatBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Rgb1Bit"/> class.
    /// </summary>
    public Rgb1Bit()
    {
        byte value = 1;
        ByteMasks = new byte[8];
        for (var x = 0; x < 8; ++x)
        {
            ByteMasks[x] = value;
            value = (byte)(value << 1);
        }
    }

    /// <summary>
    /// The bytes per pixel
    /// </summary>
    /// <value>The BPP.</value>
    public override double Bpp => 0.125;

    /// <summary>
    /// Gets or sets the byte masks.
    /// </summary>
    /// <value>The byte masks.</value>
    private byte[] ByteMasks { get; }

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
        var alignment = (int)((4d - width / 8.0d % 4d) % 4d * 8);
        var returnValue = new byte[width * height * 4];
        using (var bitStream = new BitReader(data))
        {
            for (var y = 0; y < height; ++y)
            {
                var destinationY = height - y - 1;
                var destinationOffset = destinationY * width * 4;
                for (var x = 0; x < width; ++x)
                {
                    var currentBit = bitStream.ReadBit();
                    if (currentBit == null)
                        return returnValue;
                    if (currentBit.Value)
                    {
                        returnValue[destinationOffset] = 255;
                        returnValue[destinationOffset + 1] = 255;
                        returnValue[destinationOffset + 2] = 255;
                        returnValue[destinationOffset + 3] = 255;
                    }
                    else
                    {
                        returnValue[destinationOffset] = 0;
                        returnValue[destinationOffset + 1] = 0;
                        returnValue[destinationOffset + 2] = 0;
                        returnValue[destinationOffset + 3] = 255;
                    }
                    destinationOffset += 4;
                }
                bitStream.Skip(alignment);
            }
        }
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