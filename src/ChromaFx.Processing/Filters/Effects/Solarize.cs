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
using ChromaFx.Processing.Filters.Interfaces;
using ChromaFx.Processing.Numerics;

namespace ChromaFx.Processing.Filters.Effects;

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
        int left = targetLocation.Left;
        int right = targetLocation.Right;
        // Black is (0,0,0,255); compare squared Euclidean to avoid Sqrt per pixel.
        double thresholdSq = (double)Threshold * Threshold;
        const int blackAlpha = 255;

        FilterParallelism.ForRows(
            targetLocation,
            y =>
            {
                var row = image.GetRowSpan(y);
                for (var x = left; x < right; ++x)
                {
                    ref var pixel = ref row[x];
                    int da = pixel.Alpha - blackAlpha;
                    double distSq =
                        (double)pixel.Red * pixel.Red
                        + (double)pixel.Green * pixel.Green
                        + (double)pixel.Blue * pixel.Blue
                        + (double)da * da;
                    if (distSq < thresholdSq)
                    {
                        pixel.Red = (byte)(255 - pixel.Red);
                        pixel.Green = (byte)(255 - pixel.Green);
                        pixel.Blue = (byte)(255 - pixel.Blue);
                    }
                }
            }
        );

        return image;
    }
}
