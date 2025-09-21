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

namespace ChromaFx.Filters.Smoothing;

/// <summary>
/// Medians an image
/// </summary>
/// <seealso cref="IFilter"/>
/// <remarks>
/// Initializes a new instance of the <see cref="Median"/> class.
/// </remarks>
/// <param name="apertureRadius">The aperture radius.</param>
public class Median(int apertureRadius) : IFilter
{

    /// <summary>
    /// Gets or sets the aperture radius.
    /// </summary>
    /// <value>The aperture radius.</value>
    public int ApertureRadius { get; set; } = apertureRadius;

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
        var apertureMin = -ApertureRadius;
        var apertureMax = ApertureRadius;
        Parallel.For(
            targetLocation.Bottom,
            targetLocation.Top,
            y =>
            {
                for (var x = targetLocation.Left; x < targetLocation.Right; ++x)
                {
                    var rValues = new List<byte>();
                    var gValues = new List<byte>();
                    var bValues = new List<byte>();

                    for (var x2 = apertureMin; x2 < apertureMax; ++x2)
                    {
                        var tempX = x + x2;
                        if (tempX < targetLocation.Left || tempX >= targetLocation.Right)
                            continue;

                        for (var y2 = apertureMin; y2 < apertureMax; ++y2)
                        {
                            var tempY = y + y2;
                            if (tempY < targetLocation.Bottom || tempY >= targetLocation.Top)
                                continue;

                            var index = tempY * image.Width + tempX;
                            rValues.Add(image.Pixels[index].Red);
                            gValues.Add(image.Pixels[index].Green);
                            bValues.Add(image.Pixels[index].Blue);
                        }
                    }

                    rValues.Sort();
                    gValues.Sort();
                    bValues.Sort();

                    tempValues[y * image.Width + x].Red = rValues[rValues.Count / 2];
                    tempValues[y * image.Width + x].Green = gValues[gValues.Count / 2];
                    tempValues[y * image.Width + x].Blue = bValues[bValues.Count / 2];
                    tempValues[y * image.Width + x].Alpha = image.Pixels[y * image.Width + x].Alpha;
                }
            }
        );

        return image.ReCreate(image.Width, image.Height, tempValues);
    }
}
