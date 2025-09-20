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
using System.Threading.Tasks;
using ChromaFx.Filters.Interfaces;
using ChromaFx.Numerics;
using ChromaFx.Numerics.Interfaces;
using ChromaFx.Colors;

namespace ChromaFx.Filters.Normalization;

/// <summary>
/// Adaptive equalization of an image
/// </summary>
/// <seealso cref="IFilter"/>
public class AdaptiveEqualize : IFilter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AdaptiveEqualize"/> class.
    /// </summary>
    /// <param name="radius">The radius.</param>
    /// <param name="histogram">The histogram.</param>
    public AdaptiveEqualize(int radius, Func<IHistogram> histogram = null)
    {
        Radius = radius;
        Histogram = histogram ?? (() => new RgbHistogram());
    }

    /// <summary>
    /// Gets or sets the radius.
    /// </summary>
    /// <value>The radius.</value>
    public int Radius { get; set; }

    /// <summary>
    /// Gets or sets the histogram.
    /// </summary>
    /// <value>The histogram.</value>
    private Func<IHistogram> Histogram { get; }

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
        var tempValues = new Color[image.Pixels.Length];
        Array.Copy(image.Pixels, tempValues, tempValues.Length);
        var apertureMin = -Radius;
        var apertureMax = Radius;

        Parallel.For(
            targetLocation.Bottom,
            targetLocation.Top,
            y =>
            {
                for (var x = targetLocation.Left; x < targetLocation.Right; ++x)
                {
                    var colorList = new List<Color>();
                    for (var y2 = apertureMin; y2 < apertureMax; ++y2)
                    {
                        var tempY = y + y2;
                        var tempX = x + apertureMin;
                        if (tempY < 0 || tempY >= image.Height)
                            continue;
                        var length = Radius * 2;
                        if (tempX < 0)
                        {
                            length += tempX;
                            tempX = 0;
                        }
                        var start = tempY * image.Width + tempX;
                        for (var x2 = 0; x2 < length && tempX < image.Width; ++x2)
                        {
                            colorList.Add(tempValues[start]);
                            ++start;
                            ++tempX;
                        }
                    }

                    var tempHistogram = Histogram().Load(colorList.ToArray()).Equalize();
                    var resultColor = tempHistogram.EqualizeColor(
                        image.Pixels[y * image.Width + x]
                    );

                    image.Pixels[y * image.Width + x].Red = resultColor.Red;
                    image.Pixels[y * image.Width + x].Green = resultColor.Green;
                    image.Pixels[y * image.Width + x].Blue = resultColor.Blue;
                }
            }
        );

        return image;
    }
}
