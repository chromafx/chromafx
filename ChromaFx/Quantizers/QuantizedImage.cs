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

using System;
using System.Threading.Tasks;
using ChromaFx.Colors.ColorSpaces;
using ChromaFx.Colors;

namespace ChromaFx.Quantizers;

/// <summary>
/// A quantized image
/// </summary>
public class QuantizedImage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QuantizedImage"/> class.
    /// </summary>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    /// <param name="palette">The palette.</param>
    /// <param name="pixels">The pixels.</param>
    /// <param name="transparentIndex">Index of the transparent.</param>
    public QuantizedImage(
        int width,
        int height,
        Bgra[] palette,
        byte[] pixels,
        int transparentIndex = -1
    )
    {
        if (width <= 0)
            width = 1;
        if (height <= 0)
            height = 1;
        Width = width;
        Height = height;
        Palette = palette;
        Pixels = pixels;
        TransparentIndex = transparentIndex;
    }

    /// <summary>
    /// Gets the height.
    /// </summary>
    /// <value>The height.</value>
    public int Height { get; }

    /// <summary>
    /// Gets the palette.
    /// </summary>
    /// <value>The palette.</value>
    public Bgra[] Palette { get; }

    /// <summary>
    /// Gets the pixels.
    /// </summary>
    /// <value>The pixels.</value>
    public byte[] Pixels { get; }

    /// <summary>
    /// Gets the index of the transparent color.
    /// </summary>
    /// <value>The index of the transparent color.</value>
    public int TransparentIndex { get; private set; }

    /// <summary>
    /// Gets the width.
    /// </summary>
    /// <value>The width.</value>
    public int Width { get; }

    /// <summary>
    /// Performs an implicit conversion from <see cref="QuantizedImage"/> to <see cref="Image"/>.
    /// </summary>
    /// <param name="quantizedImage">The quantized image.</param>
    /// <returns>The result of the conversion.</returns>
    public static implicit operator Image(QuantizedImage quantizedImage)
    {
        var palletCount = quantizedImage.Palette.Length - 1;
        var pixels = new Color[quantizedImage.Pixels.Length];

        Parallel.For(
            0,
            quantizedImage.Pixels.Length,
            x =>
            {
                pixels[x] = quantizedImage.Palette[Math.Min(palletCount, quantizedImage.Pixels[x])];
            }
        );

        return new Image(quantizedImage.Width, quantizedImage.Height, pixels);
    }
}
