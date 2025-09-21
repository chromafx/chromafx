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
/// Does smoothing using Kuwahara filter on an image
/// </summary>
/// <seealso cref="IFilter"/>
public class Kuwahara : IFilter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Kuwahara"/> class.
    /// </summary>
    /// <param name="apertureRadius">The aperture radius.</param>
    public Kuwahara(int apertureRadius)
    {
        ApertureRadius = apertureRadius;
        _apertureMinX = [-ApertureRadius, 0, -ApertureRadius, 0];
        _apertureMaxX = [0, ApertureRadius, 0, ApertureRadius];
        _apertureMinY = [-ApertureRadius, -ApertureRadius, 0, 0];
        _apertureMaxY = [0, 0, ApertureRadius, ApertureRadius];
    }

    /// <summary>
    /// Gets or sets the aperture radius.
    /// </summary>
    /// <value>The aperture radius.</value>
    public int ApertureRadius { get; set; }

    private readonly int[] _apertureMaxX;
    private readonly int[] _apertureMaxY;
    private readonly int[] _apertureMinX;
    private readonly int[] _apertureMinY;

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
        var result = new Color[image.Pixels.Length];
        Array.Copy(image.Pixels, result, result.Length);
        Parallel.For(
            targetLocation.Bottom,
            targetLocation.Top,
            y =>
            {
                for (var x = targetLocation.Left; x < targetLocation.Right; ++x)
                {
                    uint[] rValues = [0, 0, 0, 0];
                    uint[] gValues = [0, 0, 0, 0];
                    uint[] bValues = [0, 0, 0, 0];
                    uint[] numPixels = [0, 0, 0, 0];
                    uint[] maxRValue = [0, 0, 0, 0];
                    uint[] maxGValue = [0, 0, 0, 0];
                    uint[] maxBValue = [0, 0, 0, 0];
                    uint[] minRValue = [255, 255, 255, 255];
                    uint[] minGValue = [255, 255, 255, 255];
                    uint[] minBValue = [255, 255, 255, 255];

                    for (var i = 0; i < 4; ++i)
                    {
                        for (var x2 = _apertureMinX[i]; x2 < _apertureMaxX[i]; ++x2)
                        {
                            var tempX = x + x2;
                            if (tempX < 0 || tempX >= image.Width)
                                continue;
                            for (var y2 = _apertureMinY[i]; y2 < _apertureMaxY[i]; ++y2)
                            {
                                var tempY = y + y2;
                                if (tempY < 0 || tempY >= image.Height)
                                    continue;
                                var index = tempY * image.Width + tempX;
                                rValues[i] += image.Pixels[index].Red;
                                gValues[i] += image.Pixels[index].Green;
                                bValues[i] += image.Pixels[index].Blue;

                                if (image.Pixels[index].Red > maxRValue[i])
                                    maxRValue[i] = image.Pixels[index].Red;
                                else if (image.Pixels[index].Red < minRValue[i])
                                    minRValue[i] = image.Pixels[index].Red;

                                if (image.Pixels[index].Green > maxGValue[i])
                                    maxGValue[i] = image.Pixels[index].Green;
                                else if (image.Pixels[index].Green < minGValue[i])
                                    minGValue[i] = image.Pixels[index].Green;

                                if (image.Pixels[index].Blue > maxBValue[i])
                                    maxBValue[i] = image.Pixels[index].Blue;
                                else if (image.Pixels[index].Blue < minBValue[i])
                                    minBValue[i] = image.Pixels[index].Blue;

                                ++numPixels[i];
                            }
                        }
                    }

                    var j = 0;
                    var minDifference = uint.MaxValue;
                    for (var i = 0; i < 4; ++i)
                    {
                        var currentDifference =
                            maxRValue[i]
                            - minRValue[i]
                            + (maxGValue[i] - minGValue[i])
                            + (maxBValue[i] - minBValue[i]);
                        if (currentDifference >= minDifference || numPixels[i] <= 0)
                            continue;
                        j = i;
                        minDifference = currentDifference;
                    }

                    rValues[j] /= numPixels[j];
                    gValues[j] /= numPixels[j];
                    bValues[j] /= numPixels[j];

                    result[y * image.Width + x] = new Color(
                        (byte)rValues[j],
                        (byte)gValues[j],
                        (byte)bValues[j]
                    );
                }
            }
        );

        return image.ReCreate(image.Width, image.Height, result);
    }
}
