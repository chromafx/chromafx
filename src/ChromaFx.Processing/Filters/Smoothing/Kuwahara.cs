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
        var width = image.Width;
        var result = FilterBufferPool.RentTightCopy(image);
        try
        {
            Parallel.For(
                targetLocation.Bottom,
                targetLocation.Top,
                () => new long[10 * (width + 1)],
                (y, _, prefix) =>
                {
                    ApplyRow(image, result, targetLocation, y, radius, prefix);
                    return prefix;
                },
                _ => { }
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

    // Inclusive classic quadrants: rows [y-r, y] or [y, y+r], columns [x-r, x] or [x, x+r].
    // Column prefix sums of R, G, B, luminance and luminance² for each vertical half make every
    // quadrant sum O(1) per pixel.
    private static void ApplyRow(
        Image image,
        Color[] result,
        Rectangle targetLocation,
        int y,
        int radius,
        long[] prefix
    )
    {
        var width = image.Width;
        var stride = width + 1;
        Array.Clear(prefix);
        var upper = prefix.AsSpan(0, 5 * stride);
        var lower = prefix.AsSpan(5 * stride, 5 * stride);
        var upperStart = Math.Max(0, y - radius);
        var lowerEnd = Math.Min(image.Height, y + radius + 1);
        AccumulateRows(image, upper, stride, upperStart, y + 1);
        AccumulateRows(image, lower, stride, y, lowerEnd);
        var upperRows = (uint)(y + 1 - upperStart);
        var lowerRows = (uint)(lowerEnd - y);

        Span<uint> sumR = stackalloc uint[4];
        Span<uint> sumG = stackalloc uint[4];
        Span<uint> sumB = stackalloc uint[4];
        Span<uint> count = stackalloc uint[4];
        Span<ulong> sumLum = stackalloc ulong[4];
        Span<ulong> sumLumSq = stackalloc ulong[4];
        for (var x = targetLocation.Left; x < targetLocation.Right; ++x)
        {
            var left = Math.Max(0, x - radius);
            var right = Math.Min(width, x + radius + 1);
            Quadrant(upper, stride, left, x + 1, upperRows, 0, sumR, sumG, sumB, count, sumLum, sumLumSq);
            Quadrant(upper, stride, x, right, upperRows, 1, sumR, sumG, sumB, count, sumLum, sumLumSq);
            Quadrant(lower, stride, left, x + 1, lowerRows, 2, sumR, sumG, sumB, count, sumLum, sumLumSq);
            Quadrant(lower, stride, x, right, lowerRows, 3, sumR, sumG, sumB, count, sumLum, sumLumSq);

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
            result[y * width + x] = new Color(
                (byte)(sumR[best] / nBest),
                (byte)(sumG[best] / nBest),
                (byte)(sumB[best] / nBest),
                alpha
            );
        }
    }

    private static void AccumulateRows(Image image, Span<long> sums, int stride, int rowStart, int rowEnd)
    {
        var width = stride - 1;
        for (var y = rowStart; y < rowEnd; ++y)
        {
            var row = image.GetRowSpan(y);
            for (var x = 0; x < width; ++x)
            {
                var sample = row[x];
                // Rec. 709-ish integer luminance in 0..255.
                var lum = (uint)((77 * sample.Red + 150 * sample.Green + 29 * sample.Blue) >> 8);
                sums[x + 1] += sample.Red;
                sums[stride + x + 1] += sample.Green;
                sums[2 * stride + x + 1] += sample.Blue;
                sums[3 * stride + x + 1] += lum;
                sums[4 * stride + x + 1] += lum * lum;
            }
        }

        for (var k = 0; k < 5; ++k)
        {
            var channel = sums.Slice(k * stride, stride);
            for (var x = 1; x < stride; ++x)
                channel[x] += channel[x - 1];
        }
    }

    private static void Quadrant(
        Span<long> sums,
        int stride,
        int start,
        int end,
        uint rows,
        int i,
        Span<uint> sumR,
        Span<uint> sumG,
        Span<uint> sumB,
        Span<uint> count,
        Span<ulong> sumLum,
        Span<ulong> sumLumSq
    )
    {
        sumR[i] = (uint)(sums[end] - sums[start]);
        sumG[i] = (uint)(sums[stride + end] - sums[stride + start]);
        sumB[i] = (uint)(sums[2 * stride + end] - sums[2 * stride + start]);
        sumLum[i] = (ulong)(sums[3 * stride + end] - sums[3 * stride + start]);
        sumLumSq[i] = (ulong)(sums[4 * stride + end] - sums[4 * stride + start]);
        count[i] = (uint)(end - start) * rows;
    }
}
