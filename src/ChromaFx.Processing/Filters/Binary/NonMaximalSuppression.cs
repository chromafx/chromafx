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
/// Non-maximal suppression filter
/// </summary>
/// <seealso cref="IFilter"/>
/// <remarks>
/// Initializes a new instance of the <see cref="NonMaximalSuppression"/> class.
/// </remarks>
/// <param name="color1">The first color.</param>
/// <param name="color2">The second color.</param>
/// <param name="threshold1">The threshold1.</param>
/// <param name="threshold2">The threshold2.</param>
public class NonMaximalSuppression(Color color1, Color color2, float threshold1, float threshold2)
    : IFilter
{
    /// <summary>
    /// Gets or sets the color1.
    /// </summary>
    /// <value>The color1.</value>
    public Color Color1 { get; set; } = color1;

    /// <summary>
    /// Gets or sets the color2.
    /// </summary>
    /// <value>The color2.</value>
    public Color Color2 { get; set; } = color2;

    /// <summary>
    /// Gets or sets the threshold1.
    /// </summary>
    /// <value>The threshold1.</value>
    public float Threshold1 { get; set; } = threshold1 * 255;

    /// <summary>
    /// Gets or sets the threshold2.
    /// </summary>
    /// <value>The threshold2.</value>
    public float Threshold2 { get; set; } = threshold2 * 255;

    /// <summary>
    /// Applies the filter to the specified image.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <param name="targetLocation">The target location.</param>
    /// <returns>The image</returns>
    public Image Apply(Image image, Rectangle targetLocation = default)
    {
        targetLocation = targetLocation.Normalize(image);
        new Greyscale709().Apply(image, targetLocation);
        var result = FilterBufferPool.RentTightCopy(image);
        try
        {
            for (var y = targetLocation.Bottom; y < targetLocation.Top; ++y)
                result
                    .AsSpan(
                        y * image.Width + targetLocation.Left,
                        targetLocation.Right - targetLocation.Left
                    )
                    .Fill(Color2);

            // Mark strong edges first (parallel-safe: each pixel written once).
            Parallel.For(
                targetLocation.Bottom,
                targetLocation.Top,
                y =>
                {
                    var row = image.GetRowSpan(y);
                    var resultRow = y * image.Width;
                    for (var x = targetLocation.Left; x < targetLocation.Right; ++x)
                    {
                        if (row[x].Red >= Threshold1)
                            result[resultRow + x] = Color1;
                    }
                }
            );

            // Hysteresis flood from strong edges — serial to avoid races on result.
            for (var y = targetLocation.Bottom; y < targetLocation.Top; ++y)
            {
                var resultRow = y * image.Width;
                for (var x = targetLocation.Left; x < targetLocation.Right; ++x)
                {
                    if (result[resultRow + x] == Color1)
                        FillPixels(result, image, x, y, targetLocation);
                }
            }

            for (var y = 0; y < image.Height; ++y)
                result.AsSpan(y * image.Width, image.Width).CopyTo(image.GetRowSpan(y));
        }
        finally
        {
            FilterBufferPool.Return(result);
        }

        return image;
    }

    private void FillPixels(Color[] result, Image image, int x, int y, Rectangle targetLocation)
    {
        var width = image.Width;
        var tempPixels = new Stack<(int X, int Y)>();
        tempPixels.Push((x, y));
        while (tempPixels.Count > 0)
        {
            var currentPixel = tempPixels.Pop();
            TryVisit(result, image, width, targetLocation, tempPixels, currentPixel.X - 1, currentPixel.Y);
            TryVisit(result, image, width, targetLocation, tempPixels, currentPixel.X + 1, currentPixel.Y);
            TryVisit(result, image, width, targetLocation, tempPixels, currentPixel.X, currentPixel.Y - 1);
            TryVisit(result, image, width, targetLocation, tempPixels, currentPixel.X, currentPixel.Y + 1);
            TryVisit(result, image, width, targetLocation, tempPixels, currentPixel.X - 1, currentPixel.Y - 1);
            TryVisit(result, image, width, targetLocation, tempPixels, currentPixel.X - 1, currentPixel.Y + 1);
            TryVisit(result, image, width, targetLocation, tempPixels, currentPixel.X + 1, currentPixel.Y - 1);
            TryVisit(result, image, width, targetLocation, tempPixels, currentPixel.X + 1, currentPixel.Y + 1);
        }
    }

    private void TryVisit(
        Color[] result,
        Image image,
        int width,
        Rectangle targetLocation,
        Stack<(int X, int Y)> stack,
        int x,
        int y
    )
    {
        if (
            x < targetLocation.Left
            || x >= targetLocation.Right
            || y < targetLocation.Bottom
            || y >= targetLocation.Top
        )
            return;

        var index = y * width + x;
        if (result[index] == Color1)
            return;
        if (image.GetRowSpan(y)[x].Red <= Threshold2)
            return;

        result[index] = Color1;
        stack.Push((x, y));
    }
}
