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

using ChromaFx.Colors;
using ChromaFx.Filters.ColorMatrix;
using ChromaFx.Filters.Interfaces;
using ChromaFx.Numerics;

namespace ChromaFx.Filters.Binary;

/// <summary>
/// Adaptive threshold an image
/// </summary>
/// <seealso cref="IFilter"/>
/// <remarks>
/// Initializes a new instance of the <see cref="AdaptiveThreshold"/> class.
/// </remarks>
/// <param name="apertureRadius">The aperture radius.</param>
/// <param name="color1">The color1.</param>
/// <param name="color2">The color2.</param>
/// <param name="threshold">The threshold.</param>
public class AdaptiveThreshold(int apertureRadius, Color color1, Color color2, float threshold)
    : IFilter
{
    /// <summary>
    /// Gets or sets the aperture radius.
    /// </summary>
    public int ApertureRadius { get; set; } = apertureRadius;

    /// <summary>
    /// Gets or sets the color1.
    /// </summary>
    public Color Color1 { get; set; } = color1;

    /// <summary>
    /// Gets or sets the color2.
    /// </summary>
    public Color Color2 { get; set; } = color2;

    /// <summary>
    /// Gets or sets the threshold.
    /// </summary>
    public float Threshold { get; set; } = threshold;

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

        var tempValues = new Color[image.Width * image.Height];
        Array.Copy(image.Pixels, tempValues, tempValues.Length);

        var apertureMin = -ApertureRadius;
        var apertureMax = ApertureRadius;

        Parallel.For(
            targetLocation.Bottom,
            targetLocation.Top,
            y =>
            {
                for (var x = targetLocation.Left; x < targetLocation.Right; ++x)
                {
                    var targetIndex = y * image.Width + x;
                    var rValues = new List<byte>(
                        (apertureMax - apertureMin) * (apertureMax - apertureMin)
                    );

                    for (var x2 = apertureMin; x2 < apertureMax; ++x2)
                    {
                        var tempX = x + x2;
                        if (tempX < targetLocation.Left || tempX >= targetLocation.Right)
                            continue;

                        for (var y2 = apertureMin; y2 < apertureMax; ++y2)
                        {
                            var tempY = y + y2;
                            if (tempY >= targetLocation.Bottom && tempY < targetLocation.Top)
                            {
                                var pixelIndex = tempY * image.Width + tempX;
                                rValues.Add(image.Pixels[pixelIndex].Red);
                            }
                        }
                    }

                    tempValues[targetIndex] =
                        rValues.Count > 0 && rValues.Average(value => value / 255f) >= Threshold
                            ? Color1
                            : Color2;
                }
            }
        );

        return image.ReCreate(image.Width, image.Height, tempValues);
    }
}
