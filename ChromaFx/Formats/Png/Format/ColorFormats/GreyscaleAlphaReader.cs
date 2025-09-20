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

using ChromaFx.ExtensionMethods;
using ChromaFx.Formats.Png.Format.ColorFormats.Interfaces;
using ChromaFx.Colors;

namespace ChromaFx.Formats.Png.Format.ColorFormats;

/// <summary>
/// Greyscale with alpha reader
/// </summary>
/// <seealso cref="IColorReader"/>
public class GreyscaleAlphaReader : IColorReader
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
        Parallel.For(0, header.Width, x =>
        {
            var offset = row * header.Width + x;
            pixels[offset].Red = scanline[x * 2];
            pixels[offset].Green = scanline[x * 2];
            pixels[offset].Blue = scanline[x * 2];
            pixels[offset].Alpha = scanline[x * 2 + 1];
        });
    }
}