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
using ChromaFx.Core.Colors;
using ChromaFx.Processing.Filters.Interfaces;
using ChromaFx.Processing.Numerics;

namespace ChromaFx.Processing.Filters.Smoothing;

/// <summary>
/// Classic Kuwahara smoother: four overlapping <c>(radius+1)×(radius+1)</c> quadrants
/// that each include the center pixel; the quadrant with the lowest luminance variance
/// supplies the output mean.
/// </summary>
/// <seealso cref="IFilter"/>
/// <remarks>
/// Initializes a new instance of the <see cref="Kuwahara"/> class.
/// </remarks>
/// <param name="apertureRadius">The aperture radius.</param>
public class Kuwahara(int apertureRadius) : IFilter
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
        if (ApertureRadius <= 0)
            return image;

        targetLocation = targetLocation.Normalize(image);
        var radius = ApertureRadius;
        // Inclusive classic quadrants, stored as half-open [min, max).
        int[] minX = [-radius, 0, -radius, 0];
        int[] maxX = [1, radius + 1, 1, radius + 1];
        int[] minY = [-radius, -radius, 0, 0];
        int[] maxY = [1, 1, radius + 1, radius + 1];

        var result = FilterBufferPool.RentTightCopy(image);
        try
        {
            Parallel.For(
                targetLocation.Bottom,
                targetLocation.Top,
                y =>
                {
                    Span<uint> sumR = stackalloc uint[4];
                    Span<uint> sumG = stackalloc uint[4];
                    Span<uint> sumB = stackalloc uint[4];
                    Span<uint> count = stackalloc uint[4];
                    Span<ulong> sumLum = stackalloc ulong[4];
                    Span<ulong> sumLumSq = stackalloc ulong[4];

                    for (var x = targetLocation.Left; x < targetLocation.Right; ++x)
                    {
                        sumR.Clear();
                        sumG.Clear();
                        sumB.Clear();
                        count.Clear();
                        sumLum.Clear();
                        sumLumSq.Clear();

                        for (var i = 0; i < 4; ++i)
                        {
                            for (var x2 = minX[i]; x2 < maxX[i]; ++x2)
                            {
                                var tempX = x + x2;
                                if ((uint)tempX >= (uint)image.Width)
                                    continue;
                                for (var y2 = minY[i]; y2 < maxY[i]; ++y2)
                                {
                                    var tempY = y + y2;
                                    if ((uint)tempY >= (uint)image.Height)
                                        continue;

                                    var sample = image.GetRowSpan(tempY)[tempX];
                                    sumR[i] += sample.Red;
                                    sumG[i] += sample.Green;
                                    sumB[i] += sample.Blue;

                                    // Rec. 709-ish integer luminance in 0..255.
                                    var lum = (uint)((77 * sample.Red + 150 * sample.Green + 29 * sample.Blue) >> 8);
                                    sumLum[i] += lum;
                                    sumLumSq[i] += lum * lum;
                                    ++count[i];
                                }
                            }
                        }

                        var best = -1;
                        var bestVariance = double.MaxValue;
                        for (var i = 0; i < 4; ++i)
                        {
                            if (count[i] == 0)
                                continue;

                            var n = (double)count[i];
                            var mean = sumLum[i] / n;
                            var variance = sumLumSq[i] / n - mean * mean;
                            if (variance >= bestVariance)
                                continue;

                            bestVariance = variance;
                            best = i;
                        }

                        if (best < 0)
                            continue;

                        var nBest = count[best];
                        var alpha = image.GetRowSpan(y)[x].Alpha;
                        result[y * image.Width + x] = new Color(
                            (byte)(sumR[best] / nBest),
                            (byte)(sumG[best] / nBest),
                            (byte)(sumB[best] / nBest),
                            alpha
                        );
                    }
                }
            );

            for (var y = 0; y < image.Height; ++y)
                result.AsSpan(y * image.Width, image.Width).CopyTo(image.GetRowSpan(y));
        }
        finally
        {
            FilterBufferPool.Return(result);
        }

        return image;
    }
}
