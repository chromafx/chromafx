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

using ChromaFx.Core.Colors;
using ChromaFx.Core.ExtensionMethods;
using ChromaFx.IO.Formats.Png.Format.ColorFormats.Interfaces;

namespace ChromaFx.IO.Formats.Png.Format.ColorFormats;

/// <summary>
/// True color with alpha reader
/// </summary>
/// <seealso cref="IColorReader"/>
public class TrueColorAlphaReader : IColorReader
{
    /// <summary>
    /// Reads the scanline.
    /// </summary>
    /// <param name="scanline">The scanline.</param>
    /// <param name="pixels">The pixels.</param>
    /// <param name="header">The header.</param>
    /// <param name="row">The row.</param>
    public void ReadScanline(byte[] scanline, Color[] pixels, Header header, int row)
    {
        scanline = scanline.ExpandArray(header.BitDepth);
        var bytesPerPixel = header.BitDepth * 4 / 8;
        var bytesPerChannel = header.BitDepth / 8;

        int pixelIndex = row * header.Width;
        int scanlineIndex = 0;

        for (var x = 0; x < scanline.Length; x += bytesPerPixel)
        {
            pixels[pixelIndex].Red = scanline[scanlineIndex];
            scanlineIndex += bytesPerChannel;
            pixels[pixelIndex].Green = scanline[scanlineIndex];
            scanlineIndex += bytesPerChannel;
            pixels[pixelIndex].Blue = scanline[scanlineIndex];
            scanlineIndex += bytesPerChannel;
            pixels[pixelIndex].Alpha = scanline[scanlineIndex];
            pixelIndex++;
            scanlineIndex += bytesPerChannel;
        }
    }
}
