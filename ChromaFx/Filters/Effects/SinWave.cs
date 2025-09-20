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
using System.Threading.Tasks;
using ChromaFx.Filters.Convolution.Enums;
using ChromaFx.Filters.Interfaces;
using ChromaFx.Numerics;
using ChromaFx.Colors;

namespace ChromaFx.Filters.Effects;

/// <summary>
/// Does a sin wave on an image
/// </summary>
/// <seealso cref="IFilter"/>
public class SinWave : IFilter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SinWave"/> class.
    /// </summary>
    /// <param name="amplitude">The amplitude.</param>
    /// <param name="frequency">The frequency.</param>
    /// <param name="direction">The direction.</param>
    public SinWave(float amplitude, float frequency, Direction direction)
    {
        Direction = direction;
        Frequency = frequency;
        Amplitude = amplitude;
    }

    /// <summary>
    /// Gets or sets the amplitude.
    /// </summary>
    /// <value>The amplitude.</value>
    public float Amplitude { get; set; }

    /// <summary>
    /// Gets or sets the direction.
    /// </summary>
    /// <value>The direction.</value>
    public Direction Direction { get; set; }

    /// <summary>
    /// Gets or sets the frequency.
    /// </summary>
    /// <value>The frequency.</value>
    public float Frequency { get; set; }

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
                    double value1 = 0;
                    double value2 = 0;

                    switch (Direction)
                    {
                        case Direction.RightToLeft
                        or Direction.LeftToRight:
                            value1 = Math.Sin(x * Frequency * Math.PI / 180.0d) * Amplitude;
                            break;
                        case Direction.BottomToTop
                        or Direction.TopToBottom:
                            value2 = Math.Sin(y * Frequency * Math.PI / 180.0d) * Amplitude;
                            break;
                    }

                    value1 = y - (int)value1;
                    value2 = x - (int)value2;

                    while (value1 < 0)
                        value1 += image.Height;
                    while (value2 < 0)
                        value2 += image.Width;
                    while (value1 >= image.Height)
                        value1 -= image.Height;
                    while (value2 >= image.Width)
                        value2 -= image.Width;

                    result[y * image.Width + x] = image.Pixels[
                        (int)value1 * image.Width + (int)value2
                    ];
                }
            }
        );

        return image.ReCreate(image.Width, image.Height, result);
    }
}
