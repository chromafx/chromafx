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
using ChromaFx.Processing.Filters.ColorMatrix;
using ChromaFx.Processing.Filters.Interfaces;
using ChromaFx.Processing.Numerics;

namespace ChromaFx.Processing.Filters.Binary;

/// <summary>
/// Adaptive threshold an image
/// </summary>
/// <seealso cref="IFilter"/>
/// <remarks>
/// Initializes a new instance of the <see cref="AdaptiveThreshold"/> class.
/// </remarks>
/// <param name="apertureRadius">The aperture radius.</param>
/// <param name="color1">The color1.</param>
/// <param name="color2">The color2.</param>
/// <param name="threshold">The threshold.</param>
public class AdaptiveThreshold(int apertureRadius, Color color1, Color color2, float threshold)
    : IFilter
{
    /// <summary>
    /// Gets or sets the aperture radius.
    /// </summary>
    public int ApertureRadius { get; set; } = apertureRadius;

    /// <summary>
    /// Gets or sets the color1.
    /// </summary>
    public Color Color1 { get; set; } = color1;

    /// <summary>
    /// Gets or sets the color2.
    /// </summary>
    public Color Color2 { get; set; } = color2;

    /// <summary>
    /// Gets or sets the threshold.
    /// </summary>
    public float Threshold { get; set; } = threshold;

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
        new Greyscale709().Apply(image, targetLocation);

        var tempValues = FilterBufferPool.RentTightCopy(image);
        try
        {
            var radius = ApertureRadius;
            Parallel.For(
                targetLocation.Bottom,
                targetLocation.Top,
                () => new long[image.Width],
                (y, _, columns) =>
                {
                    ThresholdRow(image, tempValues, targetLocation, y, radius, columns);
                    return columns;
                },
                _ => { }
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

    // Window sum over [x-r, x+r) x [y-r, y+r) clipped to the target, from column sums slid along the row.
    private void ThresholdRow(
        Image image,
        Color[] tempValues,
        Rectangle targetLocation,
        int y,
        int radius,
        long[] columns
    )
    {
        var left = targetLocation.Left;
        var right = targetLocation.Right;
        var rowStart = Math.Max(y - radius, targetLocation.Bottom);
        var rowEnd = Math.Min(y + radius, targetLocation.Top);
        var rows = rowEnd - rowStart;
        Array.Clear(columns);
        for (var row = rowStart; row < rowEnd; ++row)
        {
            var span = image.GetRowSpan(row);
            for (var x = left; x < right; ++x)
                columns[x] += span[x].Red;
        }

        long sum = 0;
        var end = Math.Min(left + radius, right);
        for (var x = left; x < end; ++x)
            sum += columns[x];

        for (var x = left; x < right; ++x)
        {
            if (x > left)
            {
                var removed = x - 1 - radius;
                if (removed >= left)
                    sum -= columns[removed];
                var added = x + radius - 1;
                if (added < right)
                    sum += columns[added];
            }

            var count = (Math.Min(x + radius, right) - Math.Max(x - radius, left)) * rows;
            // Same as Average(value => value / 255f) >= Threshold.
            tempValues[y * image.Width + x] =
                count > 0 && sum / (count * 255f) >= Threshold ? Color1 : Color2;
        }
    }
}
