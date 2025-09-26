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
using ChromaFx.Formats.Png.Format.Enums;
using ChromaFx.Colors;

namespace ChromaFx.Formats.Png.Format.ColorFormats;

/// <summary>
/// Palette reader
/// </summary>
/// <seealso cref="IColorReader"/>
/// <remarks>
/// Initializes a new instance of the <see cref="PaletteReader"/> class.
/// </remarks>
/// <param name="palette">The palette.</param>
/// <param name="alphaPalette">The alpha palette.</param>
public class PaletteReader(Palette palette, Palette alphaPalette) : IColorReader
{

    /// <summary>
    /// Gets or sets the alpha palette.
    /// </summary>
    /// <value>The alpha palette.</value>
    public Palette AlphaPalette { get; set; } = alphaPalette ?? new Palette([], PaletteType.Alpha);

    /// <summary>
    /// Gets or sets the palette.
    /// </summary>
    /// <value>The palette.</value>
    public Palette Palette { get; set; } = palette ?? new Palette([], PaletteType.Color);

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

        if (AlphaPalette.Data.Length > 0)
        {
            for (var x = 0; x < header.Width; ++x)
            {
                var offset = row * header.Width + x;
                var pixelOffset = scanline[x] * 3;
                pixels[offset].Red = Palette.Data[pixelOffset];
                pixels[offset].Green = Palette.Data[pixelOffset + 1];
                pixels[offset].Blue = Palette.Data[pixelOffset + 2];
                pixels[offset].Alpha = (byte)(AlphaPalette.Data.Length > scanline[x] ? AlphaPalette.Data[scanline[x]] : 255);
            }
        }
        else
        {
            for (var x = 0; x < header.Width; ++x)
            {
                var offset = row * header.Width + x;
                var pixelOffset = scanline[x] * 3;

                pixels[offset].Red = Palette.Data[pixelOffset];
                pixels[offset].Green = Palette.Data[pixelOffset + 1];
                pixels[offset].Blue = Palette.Data[pixelOffset + 2];
                pixels[offset].Alpha = 255;
            }
        }
    }
}