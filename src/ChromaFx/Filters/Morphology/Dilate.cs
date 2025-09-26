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

namespace ChromaFx.Filters.Morphology;

/// <summary>
/// Dilates an image
/// </summary>
/// <seealso cref="IFilter"/>
/// <remarks>
/// Initializes a new instance of the <see cref="Dilate"/> class.
/// </remarks>
/// <param name="apertureRadius">The aperture radius.</param>
public class Dilate(int apertureRadius) : IFilter
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
                    byte rValue = 0;
                    byte gValue = 0;
                    byte bValue = 0;

                    for (var y2 = apertureMin; y2 < apertureMax; ++y2)
                    {
                        var tempY = y + y2;
                        var tempX = x + apertureMin;

                        if (tempY >= 0 && tempY < image.Height)
                        {
                            var length = ApertureRadius * 2;

                            if (tempX < 0)
                            {
                                length += tempX;
                                tempX = 0;
                            }

                            var start = tempY * image.Width + tempX;

                            for (var x2 = 0; x2 < length; ++x2)
                            {
                                if (tempX >= image.Width)
                                    break;

                                var tempR = tempValues[start].Red;
                                var tempG = tempValues[start].Green;
                                var tempB = tempValues[start].Blue;

                                ++start;

                                rValue = rValue > tempR ? rValue : tempR;
                                gValue = gValue > tempG ? gValue : tempG;
                                bValue = bValue > tempB ? bValue : tempB;

                                ++tempX;
                            }
                        }
                    }

                    image.Pixels[y * image.Width + x] = new Color(rValue, gValue, bValue);
                }
            }
        );

        return image;
    }
}
