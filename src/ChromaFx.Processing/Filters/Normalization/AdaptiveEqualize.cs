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

using System.Buffers;
using ChromaFx.Core;
using ChromaFx.Core.Colors;
using ChromaFx.Processing.Filters.Interfaces;
using ChromaFx.Processing.Numerics;
using ChromaFx.Processing.Numerics.Interfaces;

namespace ChromaFx.Processing.Filters.Normalization;

/// <summary>
/// Adaptive equalization of an image
/// </summary>
/// <seealso cref="IFilter"/>
/// <remarks>
/// Initializes a new instance of the <see cref="AdaptiveEqualize"/> class.
/// </remarks>
/// <param name="radius">The radius.</param>
/// <param name="histogram">The histogram.</param>
public class AdaptiveEqualize(int radius, Func<IHistogram> histogram = null) : IFilter
{
    /// <summary>
    /// Gets or sets the radius.
    /// </summary>
    /// <value>The radius.</value>
    public int Radius { get; set; } = radius;

    /// <summary>
    /// Gets or sets the histogram.
    /// </summary>
    /// <value>The histogram.</value>
    private Func<IHistogram> Histogram { get; } = histogram ?? (() => new RgbHistogram());

    private readonly bool _defaultHistogram = histogram == null;

    /// <summary>
    /// Applies the filter to the specified image.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <param name="targetLocation">The target location.</param>
    /// <returns>The image</returns>
    public Image Apply(Image image, Rectangle targetLocation = default)
    {
        if (Radius <= 0)
            return image;

        targetLocation = targetLocation.Normalize(image);
        var tempValues = FilterBufferPool.RentTightCopy(image);
        try
        {
            if (_defaultHistogram)
            {
                var radius = Radius;
                Parallel.For(
                    targetLocation.Bottom,
                    targetLocation.Top,
                    y => EqualizeRow(image, tempValues, targetLocation, y, radius)
                );
                return image;
            }

            var apertureMin = -Radius;
            var apertureMax = Radius;
            var apertureSide = Math.Max(Radius * 2, 1);
            var maxColors = apertureSide * apertureSide;

            Parallel.For(
                targetLocation.Bottom,
                targetLocation.Top,
                () =>
                    (
                        Histogram: Histogram(),
                        Buffer: ArrayPool<Color>.Shared.Rent(maxColors)
                    ),
                (y, _, local) =>
                {
                    for (var x = targetLocation.Left; x < targetLocation.Right; ++x)
                    {
                        var count = 0;
                        for (var y2 = apertureMin; y2 < apertureMax; ++y2)
                        {
                            var tempY = y + y2;
                            var tempX = x + apertureMin;
                            if (tempY < 0 || tempY >= image.Height)
                                continue;
                            var length = Radius * 2;
                            if (tempX < 0)
                            {
                                length += tempX;
                                tempX = 0;
                            }
                            var start = tempY * image.Width + tempX;
                            for (var x2 = 0; x2 < length && tempX < image.Width; ++x2)
                            {
                                local.Buffer[count++] = tempValues[start];
                                ++start;
                                ++tempX;
                            }
                        }

                        var tempHistogram = local
                            .Histogram.Load(local.Buffer.AsSpan(0, count))
                            .Equalize();
                        var row = image.GetRowSpan(y);
                        var resultColor = tempHistogram.EqualizeColor(row[x]);

                        row[x].Red = resultColor.Red;
                        row[x].Green = resultColor.Green;
                        row[x].Blue = resultColor.Blue;
                    }

                    return local;
                },
                local => ArrayPool<Color>.Shared.Return(local.Buffer)
            );
        }
        finally
        {
            FilterBufferPool.Return(tempValues);
        }

        return image;
    }

    // Same result as RgbHistogram.Load(window).Equalize().EqualizeColor(center), using sliding
    // per-channel counts and evaluating only the center pixel's bins.
    private static void EqualizeRow(
        Image image,
        Color[] source,
        Rectangle targetLocation,
        int y,
        int radius
    )
    {
        Span<int> histogram = stackalloc int[768];
        histogram.Clear();
        var width = image.Width;
        var rowStart = Math.Max(0, y - radius);
        var rowEnd = Math.Min(image.Height, y + radius);
        var left = targetLocation.Left;
        var end = Math.Min(width, left + radius);
        for (var column = Math.Max(0, left - radius); column < end; ++column)
            UpdateColumn(source, width, histogram, column, rowStart, rowEnd, 1);

        var row = image.GetRowSpan(y);
        for (var x = left; x < targetLocation.Right; ++x)
        {
            if (x > left)
            {
                var removed = x - 1 - radius;
                if (removed >= 0)
                    UpdateColumn(source, width, histogram, removed, rowStart, rowEnd, -1);
                var added = x + radius - 1;
                if (added < width)
                    UpdateColumn(source, width, histogram, added, rowStart, rowEnd, 1);
            }

            float total =
                (Math.Min(width, x + radius) - Math.Max(0, x - radius)) * (rowEnd - rowStart);
            var color = source[y * width + x];
            row[x].Red = EqualizeValue(histogram[..256], color.Red, total);
            row[x].Green = EqualizeValue(histogram.Slice(256, 256), color.Green, total);
            row[x].Blue = EqualizeValue(histogram.Slice(512, 256), color.Blue, total);
        }
    }

    private static void UpdateColumn(
        Color[] source,
        int width,
        Span<int> histogram,
        int column,
        int rowStart,
        int rowEnd,
        int delta
    )
    {
        for (var y = rowStart; y < rowEnd; ++y)
        {
            var pixel = source[y * width + column];
            histogram[pixel.Red] += delta;
            histogram[256 + pixel.Green] += delta;
            histogram[512 + pixel.Blue] += delta;
        }
    }

    private static byte EqualizeValue(ReadOnlySpan<int> histogram, int value, float total)
    {
        var min = 0;
        while (histogram[min] == 0)
            ++min;
        float cumulative = histogram[0];
        for (var i = 1; i <= value; ++i)
            cumulative += histogram[i];
        if (min == 0)
        {
            var first = histogram[0] * 256f / total;
            return value == 0 ? (byte)first : (byte)((cumulative - first) / (total - first) * 255);
        }

        if (value == min)
        {
            float count = histogram[min];
            return (byte)((cumulative - count) / (total - count) * 255);
        }

        return (byte)(cumulative / total * 255);
    }
}
