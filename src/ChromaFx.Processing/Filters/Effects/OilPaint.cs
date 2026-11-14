/*
 * Copyright 2026 Ho Tzin Mein
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

using System.Buffers;
using ChromaFx.Core;
using ChromaFx.Core.Colors;
using ChromaFx.Processing.Numerics;

namespace ChromaFx.Processing.Filters.Effects;

/// <summary>
/// Classic oil-paint effect: neighborhood intensity histogram, most-frequent bin average.
/// </summary>
/// <param name="radius">Neighborhood radius (≥ 1). Default 3.</param>
/// <param name="levels">Intensity bins (2–256). Default 20.</param>
public sealed class OilPaint(int radius = 3, int levels = 20) : IFilter
{
    /// <summary>Gets the neighborhood radius.</summary>
    public int Radius { get; } = radius;

    /// <summary>Gets the intensity bin count.</summary>
    public int Levels { get; } = levels;

    /// <inheritdoc />
    public Image Apply(Image image, Rectangle targetLocation = default)
    {
        var rad = Math.Max(1, Radius);
        var bins = Math.Clamp(Levels, 2, 256);
        targetLocation = targetLocation.Normalize(image);
        var source = FilterBufferPool.RentTightCopy(image);
        var width = image.Width;
        var height = image.Height;

        try
        {
            FilterParallelism.ForRows(
                targetLocation,
                y =>
                {
                    var counts = ArrayPool<int>.Shared.Rent(bins);
                    var sumR = ArrayPool<int>.Shared.Rent(bins);
                    var sumG = ArrayPool<int>.Shared.Rent(bins);
                    var sumB = ArrayPool<int>.Shared.Rent(bins);
                    try
                    {
                        var row = image.GetRowSpan(y);

                        for (var x = targetLocation.Left; x < targetLocation.Right; x++)
                        {
                            Array.Clear(counts, 0, bins);
                            Array.Clear(sumR, 0, bins);
                            Array.Clear(sumG, 0, bins);
                            Array.Clear(sumB, 0, bins);

                            var y0 = Math.Max(0, y - rad);
                            var y1 = Math.Min(height - 1, y + rad);
                            var x0 = Math.Max(0, x - rad);
                            var x1 = Math.Min(width - 1, x + rad);

                            for (var yy = y0; yy <= y1; yy++)
                            {
                                var srcRow = yy * width;
                                for (var xx = x0; xx <= x1; xx++)
                                {
                                    var p = source[srcRow + xx];
                                    var intensity = (p.Red + p.Green + p.Blue) / 3;
                                    var bin = intensity * (bins - 1) / 255;
                                    counts[bin]++;
                                    sumR[bin] += p.Red;
                                    sumG[bin] += p.Green;
                                    sumB[bin] += p.Blue;
                                }
                            }

                            var best = 0;
                            for (var i = 1; i < bins; i++)
                            {
                                if (counts[i] > counts[best])
                                    best = i;
                            }

                            var n = Math.Max(1, counts[best]);
                            row[x] = new Color(
                                (byte)(sumR[best] / n),
                                (byte)(sumG[best] / n),
                                (byte)(sumB[best] / n),
                                source[y * width + x].Alpha
                            );
                        }
                    }
                    finally
                    {
                        ArrayPool<int>.Shared.Return(counts);
                        ArrayPool<int>.Shared.Return(sumR);
                        ArrayPool<int>.Shared.Return(sumG);
                        ArrayPool<int>.Shared.Return(sumB);
                    }
                }
            );
        }
        finally
        {
            FilterBufferPool.Return(source);
        }

        return image;
    }
}
