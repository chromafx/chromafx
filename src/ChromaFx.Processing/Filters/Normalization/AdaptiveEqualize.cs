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
}
