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

using ChromaFx.IO.Formats.Png.Format.ColorFormats.Interfaces;

namespace ChromaFx.IO.Formats.Png.Format.ColorFormats;

/// <summary>
/// Color type information
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ColorTypeInformation"/> class.
/// </remarks>
/// <param name="scanlineFactor">The scanline factor.</param>
/// <param name="supportedBitDepths">The supported bit depths.</param>
/// <param name="scanlineReaderFactory">The scanline reader factory.</param>
public class ColorTypeInformation(int scanlineFactor, int[] supportedBitDepths, Func<Palette, Palette, IColorReader> scanlineReaderFactory)
{

    /// <summary>
    /// Gets or sets the scanline factor.
    /// </summary>
    /// <value>
    /// The scanline factor.
    /// </value>
    public int ScanlineFactor { get; set; } = scanlineFactor;

    /// <summary>
    /// Gets or sets the scanline reader factory.
    /// </summary>
    /// <value>
    /// The scanline reader factory.
    /// </value>
    public Func<Palette, Palette, IColorReader> ScanlineReaderFactory { get; set; } = scanlineReaderFactory;

    /// <summary>
    /// Gets or sets the supported bit depths.
    /// </summary>
    /// <value>
    /// The supported bit depths.
    /// </value>
    public int[] SupportedBitDepths { get; set; } = supportedBitDepths;

    /// <summary>
    /// Creates the color reader.
    /// </summary>
    /// <param name="palette">The palette.</param>
    /// <param name="alphaPalette">The alpha palette.</param>
    /// <returns>The color reader</returns>
    public IColorReader CreateColorReader(Palette palette, Palette alphaPalette)
    {
        return ScanlineReaderFactory(palette, alphaPalette);
    }
}