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
        if (ApertureRadius <= 0)
            return image;

        targetLocation = targetLocation.Normalize(image);
        var tempValues = FilterBufferPool.RentTightCopy(image);
        try
        {
            var radius = ApertureRadius;
            FilterParallelism.ForRows(
                targetLocation,
                y => ApplyRow(image, tempValues, targetLocation, y, radius)
            );

            for (var y = 0; y < image.Height; ++y)
                tempValues.AsSpan(y * image.Width, image.Width).CopyTo(image.GetRowSpan(y));
        }
        finally
        {
            FilterBufferPool.Return(tempValues);
        }

        return image;
    }

    // Sliding per-channel histograms over the [x-r, x+r) x [y-r, y+r) window; each median is tracked incrementally.
    private static void ApplyRow(
        Image image,
        Color[] tempValues,
        Rectangle targetLocation,
        int y,
        int radius
    )
    {
        Span<int> histogram = stackalloc int[768];
        Span<int> median = stackalloc int[3];
        Span<int> below = stackalloc int[3];
        histogram.Clear();
        median.Clear();
        below.Clear();
        var rowStart = Math.Max(y - radius, targetLocation.Bottom);
        var rowEnd = Math.Min(y + radius, targetLocation.Top);
        var left = targetLocation.Left;
        var right = targetLocation.Right;
        var end = Math.Min(left + radius, right);
        for (var column = left; column < end; ++column)
            UpdateColumn(image, histogram, median, below, column, rowStart, rowEnd, 1);

        for (var x = left; x < right; ++x)
        {
            if (x > left)
            {
                var removed = x - 1 - radius;
                if (removed >= left)
                    UpdateColumn(image, histogram, median, below, removed, rowStart, rowEnd, -1);
                var added = x + radius - 1;
                if (added < right)
                    UpdateColumn(image, histogram, median, below, added, rowStart, rowEnd, 1);
            }

            var columns = Math.Min(x + radius, right) - Math.Max(x - radius, left);
            var k = columns * (rowEnd - rowStart) / 2;
            ref var target = ref tempValues[y * image.Width + x];
            target.Red = Select(histogram[..256], ref median[0], ref below[0], k);
            target.Green = Select(histogram.Slice(256, 256), ref median[1], ref below[1], k);
            target.Blue = Select(histogram.Slice(512, 256), ref median[2], ref below[2], k);
            target.Alpha = image.GetRowSpan(y)[x].Alpha;
        }
    }

    private static void UpdateColumn(
        Image image,
        Span<int> histogram,
        Span<int> median,
        Span<int> below,
        int x,
        int rowStart,
        int rowEnd,
        int delta
    )
    {
        for (var y = rowStart; y < rowEnd; ++y)
        {
            var pixel = image.GetRowSpan(y)[x];
            histogram[pixel.Red] += delta;
            histogram[256 + pixel.Green] += delta;
            histogram[512 + pixel.Blue] += delta;
            if (pixel.Red < median[0])
                below[0] += delta;
            if (pixel.Green < median[1])
                below[1] += delta;
            if (pixel.Blue < median[2])
                below[2] += delta;
        }
    }

    // Moves the median to the k-th smallest value (0-based); below counts values under the median.
    private static byte Select(Span<int> histogram, ref int median, ref int below, int k)
    {
        var value = median;
        var count = below;
        while (count > k)
        {
            --value;
            count -= histogram[value];
        }

        while (count + histogram[value] <= k)
        {
            count += histogram[value];
            ++value;
        }

        median = value;
        below = count;
        return (byte)value;
    }
}
