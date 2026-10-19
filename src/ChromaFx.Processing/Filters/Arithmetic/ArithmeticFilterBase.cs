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

using ChromaFx.Core;
using ChromaFx.Core.Colors;
using ChromaFx.Processing.Filters.Interfaces;
using ChromaFx.Processing.Numerics;

namespace ChromaFx.Processing.Filters.Arithmetic;

/// <summary>
/// Shared overlap/row logic for two-image arithmetic filters.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ArithmeticFilterBase"/> class.
/// </remarks>
/// <param name="secondImage">The second image.</param>
public abstract class ArithmeticFilterBase(Image secondImage) : IFilter
{
    /// <summary>
    /// Gets or sets the second image.
    /// </summary>
    /// <value>The second image.</value>
    public Image SecondImage { get; set; } = secondImage;

    /// <summary>
    /// Applies the operation to the overlapping region of the two images.
    /// </summary>
    /// <param name="image">The destination image.</param>
    /// <param name="targetLocation">The target location.</param>
    /// <returns>The image</returns>
    public Image Apply(Image image, Rectangle targetLocation = default)
    {
        targetLocation = targetLocation.Normalize(image);

        var destPixels = image.Pixels;
        var srcPixels = SecondImage.Pixels;
        int destWidth = image.Width;
        int srcWidth = SecondImage.Width;
        int srcHeight = SecondImage.Height;
        int left = targetLocation.Left;
        int right = targetLocation.Right;
        int bottom = targetLocation.Bottom;
        // Match legacy guard: skip rows where y >= SecondImage.Height (absolute), not relative to bottom.
        int top = Math.Min(targetLocation.Top, srcHeight);
        int overlapWidth = Math.Min(right - left, srcWidth);

        if (overlapWidth <= 0 || top <= bottom)
            return image;

        FilterParallelism.ForRows(
            new Rectangle(left, bottom, overlapWidth, top - bottom),
            y =>
            {
                int destStart = y * destWidth + left;
                int srcStart = (y - bottom) * srcWidth;
                ProcessRow(
                    destPixels.AsSpan(destStart, overlapWidth),
                    srcPixels.AsSpan(srcStart, overlapWidth));
            });

        return image;
    }

    /// <summary>
    /// Processes one overlapping row of destination and source pixels.
    /// </summary>
    /// <param name="destination">Destination row slice.</param>
    /// <param name="source">Source row slice of equal length.</param>
    protected abstract void ProcessRow(Span<Color> destination, ReadOnlySpan<Color> source);
}
