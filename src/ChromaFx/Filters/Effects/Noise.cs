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
using ChromaFx.Filters.Interfaces;
using ChromaFx.Numerics;
using Random = ChromaFx.Numerics.Random;

namespace ChromaFx.Filters.Effects;

/// <summary>
/// Adds randomization to an image
/// </summary>
/// <seealso cref="IFilter"/>
public class Noise : IFilter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Noise"/> class.
    /// </summary>
    /// <param name="amount">The amount of potential randomization (0 to 1).</param>
    public Noise(byte amount)
    {
        Amount = amount;
    }

    /// <summary>
    /// Gets or sets the amount.
    /// </summary>
    /// <value>The amount.</value>
    public byte Amount { get; set; }

    /// <summary>
    /// Applies the filter to the specified image.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <param name="targetLocation">The target location.</param>
    /// <returns>The image</returns>
    public Image Apply(Image image, Rectangle targetLocation = default)
    {
        targetLocation =
            targetLocation == default
                ? new Rectangle(0, 0, image.Width, image.Height)
                : targetLocation.Clamp(image);

        Parallel.For(
            targetLocation.Bottom,
            targetLocation.Top,
            y =>
            {
                for (var x = targetLocation.Left; x < targetLocation.Right; ++x)
                {
                    var outputIndex = y * image.Width + x;

                    var r = image.Pixels[outputIndex].Red + Random.ThreadSafeNext(-Amount, Amount);
                    var g =
                        image.Pixels[outputIndex].Green + Random.ThreadSafeNext(-Amount, Amount);
                    var b = image.Pixels[outputIndex].Blue + Random.ThreadSafeNext(-Amount, Amount);

                    r = r.ToByte();
                    g = g.ToByte();
                    b = b.ToByte();

                    image.Pixels[outputIndex].Red = (byte)r;
                    image.Pixels[outputIndex].Green = (byte)g;
                    image.Pixels[outputIndex].Blue = (byte)b;
                }
            }
        );

        return image;
    }
}
