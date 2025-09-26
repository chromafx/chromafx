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

using System.Numerics;
using ChromaFx.Filters.Interfaces;
using ChromaFx.Numerics;

namespace ChromaFx.Filters.Normalization;

/// <summary>
/// Adjusts gamma of an image
/// </summary>
/// <seealso cref="IFilter"/>
public class Gamma : IFilter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Gamma"/> class.
    /// </summary>
    /// <param name="value">The value.</param>
    public Gamma(float value)
    {
        Value = value;
        Ramp = new int[256];
        Parallel.For(
            0,
            256,
            x =>
            {
                Ramp[x] = (int)(255.0 * Math.Pow(x / 255.0, 1.0 / Value) + 0.5);
                Ramp[x] =
                    Ramp[x] < 0 ? 0
                    : Ramp[x] > 255 ? 255
                    : Ramp[x];
            }
        );
    }

    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    /// <value>The value.</value>
    public float Value { get; set; }

    private int[] Ramp { get; }

    /// <summary>
    /// Applies the filter to the specified image.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <param name="targetLocation">The target location.</param>
    /// <returns>The image</returns>
    public Image Apply(Image image, Rectangle targetLocation = default)
    {
        targetLocation = targetLocation.Normalize(image);

        var pixels = image.Pixels;
        int width = image.Width;
        int left = targetLocation.Left;
        int right = targetLocation.Right;
        int bottom = targetLocation.Bottom;
        int top = targetLocation.Top;
        int[] ramp = Ramp;

        int simdLength = Vector<byte>.Count;

        Parallel.For(
            bottom,
            top,
            y =>
            {
                int rowStart = y * width + left;
                int rowEnd = y * width + right;
                int x = rowStart;
                byte[] rArr = new byte[simdLength];
                byte[] gArr = new byte[simdLength];
                byte[] bArr = new byte[simdLength];
                for (; x <= rowEnd - simdLength; x += simdLength)
                {
                    for (int i = 0; i < simdLength; i++)
                    {
                        rArr[i] = pixels[x + i].Red;
                        gArr[i] = pixels[x + i].Green;
                        bArr[i] = pixels[x + i].Blue;
                    }
                    for (int i = 0; i < simdLength; i++)
                    {
                        rArr[i] = (byte)ramp[rArr[i]];
                        gArr[i] = (byte)ramp[gArr[i]];
                        bArr[i] = (byte)ramp[bArr[i]];
                    }
                    for (int i = 0; i < simdLength; i++)
                    {
                        pixels[x + i].Red = rArr[i];
                        pixels[x + i].Green = gArr[i];
                        pixels[x + i].Blue = bArr[i];
                    }
                }
                for (; x < rowEnd; x++)
                {
                    pixels[x].Red = (byte)ramp[pixels[x].Red];
                    pixels[x].Green = (byte)ramp[pixels[x].Green];
                    pixels[x].Blue = (byte)ramp[pixels[x].Blue];
                }
            }
        );

        return image;
    }
}
