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

using System.Threading.Tasks;
using ChromaFx.Filters.Interfaces;
using ChromaFx.Numerics;

namespace ChromaFx.Filters.Effects;

/// <summary>
/// Adds randomization to each pixel in an image
/// </summary>
/// <seealso cref="IFilter"/>
public class Jitter : IFilter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Jitter"/> class.
    /// </summary>
    /// <param name="amount">The amount of potential randomization.</param>
    public Jitter(int amount)
    {
        Amount = amount;
    }

    /// <summary>
    /// Gets or sets the amount.
    /// </summary>
    /// <value>The amount.</value>
    public int Amount { get; set; }

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
                    var newX = Random.ThreadSafeNext(-Amount, Amount);
                    var newY = Random.ThreadSafeNext(-Amount, Amount);
                    newX += x;
                    newY += y;
                    newX =
                        newX < targetLocation.Left
                            ? targetLocation.Left
                            : newX >= targetLocation.Right
                                ? targetLocation.Right - 1
                                : newX;
                    newY =
                        newY < targetLocation.Bottom
                            ? targetLocation.Bottom
                            : newY >= targetLocation.Top
                                ? targetLocation.Top - 1
                                : newY;

                    var sourceIndex = y * image.Width + x;
                    var destinationIndex = newY * image.Width + newX;

                    image.Pixels[destinationIndex] = image.Pixels[sourceIndex];
                }
            }
        );

        return image;
    }
}
