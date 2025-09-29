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

using ChromaFx.Core.Colors;
using ChromaFx.Processing.Filters.Interfaces;
using ChromaFx.Processing.Numerics;
using ChromaFx.Core;
using ChromaFx.Processing.Filters.ColorMatrix;

namespace ChromaFx.Processing.Filters.Binary;

/// <summary>
/// Threshold filter
/// </summary>
/// <seealso cref="IFilter" />
/// <remarks>
/// Initializes a new instance of the <see cref="Threshold"/> class.
/// </remarks>
/// <param name="color1">The first color.</param>
/// <param name="color2">The second color.</param>
/// <param name="threshold">The threshold.</param>
public class Threshold(Color color1, Color color2, float threshold) : IFilter
{

    /// <summary>
    /// Gets or sets the color1.
    /// </summary>
    /// <value>The color1.</value>
    public Color Color1 { get; set; } = color1;

    /// <summary>
    /// Gets or sets the color2.
    /// </summary>
    /// <value>The color2.</value>
    public Color Color2 { get; set; } = color2;

    /// <summary>
    /// Gets or sets the threshold value.
    /// </summary>
    /// <value>The threshold value.</value>
    public float ThresholdValue { get; set; } = threshold;

    /// <summary>
    /// Applies the filter to the specified image.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <param name="targetLocation">The target location.</param>
    /// <returns>The image</returns>
    public Image Apply(Image image, Rectangle targetLocation = default)
    {
        targetLocation = targetLocation.Normalize(image);
        new Greyscale709().Apply(image, targetLocation);

        Parallel.For(
            targetLocation.Bottom,
            targetLocation.Top,
            y =>
            {
                for (var x = targetLocation.Left; x < targetLocation.Right; ++x)
                {
                    var targetIndex = y * image.Width + x;
                    image.Pixels[targetIndex] =
                        image.Pixels[targetIndex].Red / 255f >= ThresholdValue ? Color1 : Color2;
                }
            }
        );

        return image;
    }
}
