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
using ChromaFx.Processing.Filters.Interfaces;
using ChromaFx.Processing.Numerics;
using ChromaFx.Processing.Numerics.Interfaces;
using ChromaFx.Processing.Simd;

namespace ChromaFx.Processing.Filters.Normalization;

/// <summary>
/// Equalizes of an image
/// </summary>
/// <seealso cref="IFilter"/>
/// <remarks>
/// Initializes a new instance of the <see cref="Equalize"/> class.
/// </remarks>
/// <param name="histogram">The histogram.</param>
public class Equalize(IHistogram histogram = null) : IFilter
{
    /// <summary>
    /// Gets or sets the histogram.
    /// </summary>
    /// <value>The histogram.</value>
    private IHistogram Histogram { get; } = histogram ?? new RgbHistogram();

    /// <summary>
    /// Applies the filter to the specified image.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <param name="targetLocation">The target location.</param>
    /// <returns>The image</returns>
    public Image Apply(Image image, Rectangle targetLocation = default)
    {
        targetLocation = targetLocation.Normalize(image);
        Histogram.LoadImage(image).Equalize();

        // RGB histograms map channels independently → LUT. HSV (and others) may couple channels.
        if (Histogram is RgbHistogram rgb)
        {
            var lutR = new byte[256];
            var lutG = new byte[256];
            var lutB = new byte[256];
            for (var i = 0; i < 256; ++i)
            {
                lutR[i] = (byte)rgb.R[i];
                lutG[i] = (byte)rgb.G[i];
                lutB[i] = (byte)rgb.B[i];
            }

            Parallel.For(
                targetLocation.Bottom,
                targetLocation.Top,
                y =>
                {
                    var row = image.GetRowSpan(y).Slice(targetLocation.Left, targetLocation.Width);
                    PixelSimd.ApplyChannelLuts(row, lutR, lutG, lutB);
                }
            );

            return image;
        }

        Parallel.For(
            targetLocation.Bottom,
            targetLocation.Top,
            y =>
            {
                var row = image.GetRowSpan(y);
                for (var x = targetLocation.Left; x < targetLocation.Right; ++x)
                {
                    var resultColor = Histogram.EqualizeColor(row[x]);
                    row[x].Red = resultColor.Red;
                    row[x].Green = resultColor.Green;
                    row[x].Blue = resultColor.Blue;
                }
            }
        );

        return image;
    }
}
