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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChromaFx.Filters.ColorMatrix;
using ChromaFx.Filters.Interfaces;
using ChromaFx.Numerics;
using ChromaFx.Colors;

namespace ChromaFx.Filters.Binary;

/// <summary>
/// Adaptive threshold an image
/// </summary>
/// <seealso cref="IFilter"/>
public class AdaptiveThreshold : IFilter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AdaptiveThreshold" /> class.
    /// </summary>
    /// <param name="apertureRadius">The aperture radius.</param>
    /// <param name="color1">The color1.</param>
    /// <param name="color2">The color2.</param>
    /// <param name="threshold">The threshold.</param>
    public AdaptiveThreshold(int apertureRadius, Color color1, Color color2, float threshold)
    {
        Threshold = threshold;
        Color2 = color2;
        Color1 = color1;
        ApertureRadius = apertureRadius;
    }

    /// <summary>
    /// Gets or sets the aperture radius.
    /// </summary>
    /// <value>The aperture radius.</value>
    public int ApertureRadius { get; set; }

    /// <summary>
    /// Gets or sets the color1.
    /// </summary>
    /// <value>
    /// The color1.
    /// </value>
    public Color Color1 { get; set; }

    /// <summary>
    /// Gets or sets the color2.
    /// </summary>
    /// <value>
    /// The color2.
    /// </value>
    public Color Color2 { get; set; }

    /// <summary>
    /// Gets or sets the threshold.
    /// </summary>
    /// <value>
    /// The threshold.
    /// </value>
    public float Threshold { get; set; }

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
                    var rValues = new List<byte>();

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
                        rValues.Average(value => value / 255f) >= Threshold ? Color1 : Color2;
                }
            }
        );

        return image.ReCreate(image.Width, image.Height, tempValues);
    }
}
