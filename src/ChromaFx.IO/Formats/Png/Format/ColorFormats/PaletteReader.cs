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
using ChromaFx.Core.Colors;
using ChromaFx.Core.ExtensionMethods;
using ChromaFx.IO.Formats.Png.Format.Enums;

namespace ChromaFx.IO.Formats.Png.Format.ColorFormats;

/// <summary>
/// Palette reader
/// </summary>
/// <seealso cref="IColorReader"/>
/// <remarks>
/// Initializes a new instance of the <see cref="PaletteReader"/> class.
/// </remarks>
/// <param name="palette">The palette.</param>
/// <param name="alphaPalette">The alpha palette.</param>
internal class PaletteReader(Palette palette, Palette alphaPalette) : IColorReader
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

    /// <inheritdoc />
    public void ReadScanline(byte[] scanline, Span<Color> row, Header header)
    {
        scanline = scanline.ExpandArray(header.BitDepth);
        var dest = MemoryMarshal.Cast<Color, uint>(row);
        var hasAlpha = AlphaPalette.Data.Length > 0;

        for (var x = 0; x < header.Width; ++x)
        {
            var index = scanline[x];
            var pixelOffset = index * 3;
            var r = Palette.Data[pixelOffset];
            var g = Palette.Data[pixelOffset + 1];
            var b = Palette.Data[pixelOffset + 2];
            var a = hasAlpha && AlphaPalette.Data.Length > index
                ? AlphaPalette.Data[index]
                : (byte)255;
            dest[x] = r | ((uint)g << 8) | ((uint)b << 16) | ((uint)a << 24);
        }
    }
}
