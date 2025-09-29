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

using ChromaFx.Core;
using ChromaFx.Core.Colors;
using ChromaFx.Processing.Filters.Interfaces;
using ChromaFx.Processing.Numerics;

namespace ChromaFx.Processing.Filters.Smoothing;

/// <summary>
/// SNN Blur on an image
/// </summary>
/// <seealso cref="IFilter"/>
/// <remarks>
/// Initializes a new instance of the <see cref="SnnBlur"/> class.
/// </remarks>
/// <param name="apertureRadius">The aperture radius.</param>
public class SnnBlur(int apertureRadius) : IFilter
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
        targetLocation = targetLocation.Normalize(image);
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
                    uint rValue = 0;
                    uint gValue = 0;
                    uint bValue = 0;
                    var numPixels = 0;

                    for (var x2 = apertureMin; x2 < apertureMax; ++x2)
                    {
                        var tempX1 = x + x2;
                        var tempX2 = x - x2;

                        if (
                            tempX1 < targetLocation.Left
                            || tempX1 >= targetLocation.Right
                            || tempX2 < targetLocation.Left
                            || tempX2 >= targetLocation.Right
                        )
                            continue;

                        for (var y2 = apertureMin; y2 < apertureMax; ++y2)
                        {
                            var tempY1 = y + y2;
                            var tempY2 = y - y2;

                            if (
                                tempY1 < targetLocation.Bottom
                                || tempY1 >= targetLocation.Top
                                || tempY2 < targetLocation.Bottom
                                || tempY2 >= targetLocation.Top
                            )
                                continue;

                            var tempValue1 = image.Pixels[y * image.Width + x];
                            var tempValue2 = image.Pixels[tempY1 * image.Width + tempX1];
                            var tempValue3 = image.Pixels[tempY2 * image.Width + tempX2];

                            if (
                                Distance.Euclidean(tempValue1, tempValue2)
                                < Distance.Euclidean(tempValue1, tempValue3)
                            )
                            {
                                rValue += tempValue2.Red;
                                gValue += tempValue2.Green;
                                bValue += tempValue2.Blue;
                            }
                            else
                            {
                                rValue += tempValue3.Red;
                                gValue += tempValue3.Green;
                                bValue += tempValue3.Blue;
                            }

                            ++numPixels;
                        }
                    }

                    tempValues[y * image.Width + x].Red = (byte)(rValue / numPixels);
                    tempValues[y * image.Width + x].Green = (byte)(gValue / numPixels);
                    tempValues[y * image.Width + x].Blue = (byte)(bValue / numPixels);
                    tempValues[y * image.Width + x].Alpha = image.Pixels[y * image.Width + x].Alpha;
                }
            }
        );

        return image.ReCreate(image.Width, image.Height, tempValues);
    }
}
