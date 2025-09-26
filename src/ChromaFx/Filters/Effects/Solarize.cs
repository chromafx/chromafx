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

using ChromaFx.Filters.Interfaces;
using ChromaFx.Numerics;
using ChromaFx.Colors;

namespace ChromaFx.Filters.Effects;

/// <summary>
/// Solarizes an image
/// </summary>
/// <seealso cref="IFilter"/>
/// <remarks>
/// Initializes a new instance of the <see cref="Solarize"/> class.
/// </remarks>
/// <param name="threshold">The threshold (between 0 and 3).</param>
public class Solarize(float threshold) : IFilter
{

    /// <summary>
    /// Gets or sets the threshold.
    /// </summary>
    /// <value>The threshold.</value>
    public float Threshold { get; set; } = threshold * 255;

    /// <summary>
    /// Applies the filter to the specified image.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <param name="targetLocation">The target location.</param>
    /// <returns>The image</returns>
    public Image Apply(Image image, Rectangle targetLocation = default)
    {
        targetLocation = targetLocation.Normalize(image);
        Parallel.For(
            targetLocation.Bottom,
            targetLocation.Top,
            y =>
            {
                for (var x = targetLocation.Left; x < targetLocation.Right; ++x)
                {
                    var pixelIndex = y * image.Width + x;
                    if (Distance.Euclidean(image.Pixels[pixelIndex], Color.Black) < Threshold)
                    {
                        image.Pixels[pixelIndex] = new Color(
                            (byte)(255 - image.Pixels[pixelIndex].Red),
                            (byte)(255 - image.Pixels[pixelIndex].Green),
                            (byte)(255 - image.Pixels[pixelIndex].Blue),
                            image.Pixels[pixelIndex].Alpha
                        );
                    }
                }
            }
        );

        return image;
    }
}
