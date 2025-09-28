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

using ChromaFx.Core;
using ChromaFx.Core.Colors;
using ChromaFx.Core.Colors.ColorSpaces;
using ChromaFx.Core.ExtensionMethods;
using ChromaFx.IO.Formats.Gif.Quantizers.BaseClasses;

namespace ChromaFx.IO.Formats.Gif.Quantizers.Octree;

/// <summary>
/// Octree quantizer class
/// </summary>
/// <seealso cref="QuantizerBase"/>
public class OctreeQuantizer : QuantizerBase
{
    /// <summary>
    /// Maximum allowed color depth
    /// </summary>
    private int _colors;

    /// <summary>
    /// Stores the tree
    /// </summary>
    private Octree _octree;

    /// <summary>
    /// Gets the palette.
    /// </summary>
    /// <returns>The list of colors in the palette</returns>
    protected override Bgra[] GetPalette()
    {
        var palette = _octree.Palletize(Math.Max(_colors, 1));
        palette.Add(new Color(0, 0, 0, 0));
        TransparentIndex = _colors;
        return palette.ToArray();
    }

    /// <summary>
    /// Initializes this instance.
    /// </summary>
    /// <param name="image">Image</param>
    /// <param name="maxColors">Maximum colors</param>
    protected override void Initialize(Image image, int maxColors)
    {
        _colors = maxColors.Clamp(1, 255);

        _octree ??= new Octree(GetBitsNeededForColorDepth(maxColors));
        for (var y = 0; y < image.Height; y++)
        {
            for (var x = 0; x < image.Width; x++)
            {
                var tempOffset = y * image.Width + x;
                _octree.AddColor(image.Pixels[tempOffset]);
            }
        }
    }

    /// <summary>
    /// Processes the specified image.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <returns>The resulting byte array.</returns>
    protected override byte[] Process(Image image)
    {
        var quantizedPixels = new byte[image.Width * image.Height];
        Parallel.For(
            0,
            image.Height,
            y =>
            {
                for (var x = 0; x < image.Width; x++)
                {
                    var tempOffset = y * image.Width + x;
                    quantizedPixels[tempOffset] = QuantizePixel(image.Pixels[tempOffset]);
                }
            }
        );

        return quantizedPixels;
    }

    /// <summary>
    /// Quantizes the pixel.
    /// </summary>
    /// <param name="pixel">The pixel.</param>
    /// <returns>The resulting byte</returns>
    protected byte QuantizePixel(Bgra pixel)
    {
        var paletteIndex = (byte)_colors;
        if (pixel.Alpha > TransparencyThreshold)
        {
            paletteIndex = (byte)_octree.GetPaletteIndex(pixel);
        }

        return paletteIndex;
    }

    /// <summary>
    /// Gets the bits needed for color depth.
    /// </summary>
    /// <param name="colorCount">The color count.</param>
    /// <returns>The bits needed</returns>
    private static int GetBitsNeededForColorDepth(int colorCount)
    {
        return (int)Math.Ceiling(Math.Log(colorCount, 2));
    }
}
