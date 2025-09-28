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

using ChromaFx.IO.Formats.Bmp.Format.PixelFormats.Interfaces;

namespace ChromaFx.IO.Formats.Bmp.Format.PixelFormats.BaseClasses;

/// <summary>
/// Pixel format base class
/// </summary>
/// <seealso cref="IPixelFormat"/>
public abstract class PixelFormatBase : IPixelFormat
{
    /// <summary>
    /// The bytes per pixel for this format.
    /// </summary>
    /// <value>The BPP.</value>
    public abstract double Bpp { get; }

    /// <summary>
    /// Decodes the specified data.
    /// </summary>
    /// <param name="header">The header.</param>
    /// <param name="data">The data.</param>
    /// <param name="palette">The palette.</param>
    /// <returns>The decoded data</returns>
    public abstract byte[] Decode(Header header, byte[] data, Palette palette);

    /// <summary>
    /// Encodes the specified data.
    /// </summary>
    /// <param name="header">The header.</param>
    /// <param name="data">The data.</param>
    /// <param name="palette">The palette.</param>
    /// <returns>The encoded data</returns>
    public abstract byte[] Encode(Header header, byte[] data, Palette palette);

    /// <summary>
    /// Reads the byte array from the stream
    /// </summary>
    /// <param name="header">The header.</param>
    /// <param name="stream">The stream.</param>
    /// <returns>The byte array of the data</returns>
    public byte[] Read(Header header, Stream stream)
    {
        switch (header.Compression)
        {
            case Compression.Rgb or Compression.Bitfields:
            {
                var width = header.Width;
                var height = header.Height;
                var alignment = (int)(4 - width * Bpp % 4) % 4;
                var size = (int)(width * Bpp + alignment) * height;
                if (size < header.ImageSize)
                    size = header.ImageSize;
                var data = new byte[size];
                stream.Read(data, 0, size);
                return data;
            }
            case Compression.Rle8:
            {
                var width = header.Width;
                var alignment = (int)(4 - width * Bpp % 4) % 4;
                var tempData = new byte[2048];
                using (var memStream = new MemoryStream())
                {
                    int length;
                    while ((length = stream.Read(tempData, 0, 2048)) > 0)
                    {
                        memStream.Write(tempData, 0, length);
                    }
                    tempData = memStream.ToArray();
                }
                using (var memStream = new MemoryStream())
                {
                    for (var x = 0; x < tempData.Length;)
                    {
                        if (tempData[x] == 0)
                        {
                            ++x;
                            switch (tempData[x])
                            {
                                case 0:
                                    for (var y = 0; y < alignment; ++y)
                                    {
                                        memStream.WriteByte(0);
                                    }
                                    ++x;
                                    break;

                                case 1:
                                    return memStream.ToArray();

                                case 2:
                                    break;

                                default:
                                    int runLength = tempData[x];
                                    ++x;
                                    var absoluteAlignment = (2 - runLength % 2) % 2;
                                    for (var y = 0; y < runLength; ++y, ++x)
                                    {
                                        memStream.WriteByte(tempData[x]);
                                    }
                                    x += absoluteAlignment;
                                    break;
                            }
                        }
                        else
                        {
                            int runLength = tempData[x];
                            ++x;
                            var value = tempData[x];
                            ++x;
                            for (var y = 0; y < runLength; ++y)
                            {
                                memStream.WriteByte(value);
                            }
                        }
                    }
                }

                break;
            }
        }

        return [];
    }
}