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

using ChromaFx.Core;
using ChromaFx.Core.Colors;
using ChromaFx.Core.Quantization;

namespace ChromaFx.IO.Formats.Gif.Quantizers;

/// <summary>
/// GIF octree quantizer (uses shared Core <see cref="Octree"/>).
/// </summary>
internal sealed class OctreeQuantizer
{
    private int _colors;
    private Octree _octree;

    /// <summary>
    /// Gets or sets the transparency threshold.
    /// </summary>
    public byte TransparencyThreshold { get; set; }

    /// <summary>
    /// Gets the transparent palette index after quantize.
    /// </summary>
    public int TransparentIndex { get; private set; } = -1;

    /// <summary>
    /// Quantizes <paramref name="image"/> to at most <paramref name="maxColors"/> colors.
    /// </summary>
    public QuantizedImage Quantize(Image image, int maxColors)
    {
        Initialize(image, maxColors);
        var palette = GetPalette();
        var pixels = Process(image);
        return new QuantizedImage(image.Width, image.Height, palette, pixels, TransparentIndex);
    }

    private Bgra[] GetPalette()
    {
        var colors = _octree.Palettize(Math.Max(_colors, 1));
        var palette = new Bgra[colors.Count + 1];
        for (var i = 0; i < colors.Count; i++)
            palette[i] = colors[i];
        palette[colors.Count] = new Color(0, 0, 0, 0);
        TransparentIndex = _colors;
        return palette;
    }

    private void Initialize(Image image, int maxColors)
    {
        _colors = maxColors.Clamp(1, 255);

        _octree ??= new Octree(Octree.BitsNeeded(maxColors));
        for (var y = 0; y < image.Height; y++)
        {
            var row = image.GetRowSpan(y);
            for (var x = 0; x < image.Width; x++)
                _octree.AddColor(row[x]);
        }
    }

    private byte[] Process(Image image)
    {
        var quantizedPixels = new byte[image.Width * image.Height];
        Parallel.For(
            0,
            image.Height,
            y =>
            {
                var row = image.GetRowSpan(y);
                for (var x = 0; x < image.Width; x++)
                    quantizedPixels[y * image.Width + x] = QuantizePixel(row[x]);
            }
        );

        return quantizedPixels;
    }

    private byte QuantizePixel(Color pixel)
    {
        var paletteIndex = (byte)_colors;
        if (pixel.Alpha > TransparencyThreshold)
            paletteIndex = (byte)_octree.GetPaletteIndex(pixel);

        return paletteIndex;
    }

}
