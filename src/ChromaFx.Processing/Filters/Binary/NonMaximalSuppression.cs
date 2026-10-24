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
        var result = new Image(image.Width, image.Height);
        image.PixelSpan.CopyTo(result.PixelSpan);
        new Drawing.Rectangle(Color2, true, targetLocation).Apply(result);
        Parallel.For(
            targetLocation.Bottom,
            targetLocation.Top,
            y =>
            {
                var row = image.GetRowSpan(y);
                for (var x = targetLocation.Left; x < targetLocation.Right; ++x)
                {
                    if (row[x].Red >= Threshold1)
                        FillPixels(result, image, x, y, targetLocation);
                }
            }
        );
        return image.ReCreate(image.Width, image.Height, result.PixelSpan);
    }

    private void FillPixels(Image result, Image image, int x, int y, Rectangle targetLocation)
    {
        var tempPixels = new Stack<Tuple<int, int>>();
        tempPixels.Push(new Tuple<int, int>(x, y));
        while (tempPixels.Count > 0)
        {
            var currentPixel = tempPixels.Pop();
            var left = currentPixel.Item1 - 1;
            if (left >= targetLocation.Left)
            {
                if (
                    image.GetRowSpan(currentPixel.Item2)[left].Red > Threshold2
                    && result.GetRowSpan(currentPixel.Item2)[left] != Color1
                )
                {
                    result.GetRowSpan(currentPixel.Item2)[left] = Color1;
                    tempPixels.Push(new Tuple<int, int>(left, currentPixel.Item2));
                }
            }
            var right = currentPixel.Item1 + 1;
            if (right < targetLocation.Right)
            {
                if (
                    image.GetRowSpan(currentPixel.Item2)[right].Red > Threshold2
                    && result.GetRowSpan(currentPixel.Item2)[right] != Color1
                )
                {
                    result.GetRowSpan(currentPixel.Item2)[right] = Color1;
                    tempPixels.Push(new Tuple<int, int>(right, currentPixel.Item2));
                }
            }
            var bottom = currentPixel.Item2 - 1;
            if (bottom >= targetLocation.Bottom)
            {
                if (
                    image.GetRowSpan(bottom)[currentPixel.Item1].Red > Threshold2
                    && result.GetRowSpan(bottom)[currentPixel.Item1] != Color1
                )
                {
                    result.GetRowSpan(bottom)[currentPixel.Item1] = Color1;
                    tempPixels.Push(new Tuple<int, int>(currentPixel.Item1, bottom));
                }
            }
            var top = currentPixel.Item2 + 1;
            if (top < targetLocation.Top)
            {
                if (
                    image.GetRowSpan(top)[currentPixel.Item1].Red > Threshold2
                    && result.GetRowSpan(top)[currentPixel.Item1] != Color1
                )
                {
                    result.GetRowSpan(top)[currentPixel.Item1] = Color1;
                    tempPixels.Push(new Tuple<int, int>(currentPixel.Item1, top));
                }
            }
            if (left >= targetLocation.Left && bottom >= targetLocation.Bottom)
            {
                if (
                    image.GetRowSpan(bottom)[left].Red > Threshold2
                    && result.GetRowSpan(bottom)[left] != Color1
                )
                {
                    result.GetRowSpan(bottom)[left] = Color1;
                    tempPixels.Push(new Tuple<int, int>(left, bottom));
                }
            }
            if (left >= targetLocation.Left && top < targetLocation.Top)
            {
                if (
                    image.GetRowSpan(top)[left].Red > Threshold2
                    && result.GetRowSpan(top)[left] != Color1
                )
                {
                    result.GetRowSpan(top)[left] = Color1;
                    tempPixels.Push(new Tuple<int, int>(left, top));
                }
            }
            if (right < targetLocation.Right && bottom >= targetLocation.Bottom)
            {
                if (
                    image.GetRowSpan(bottom)[right].Red > Threshold2
                    && result.GetRowSpan(bottom)[right] != Color1
                )
                {
                    result.GetRowSpan(bottom)[right] = Color1;
                    tempPixels.Push(new Tuple<int, int>(right, bottom));
                }
            }
            if (right < targetLocation.Left && top < targetLocation.Top)
            {
                if (
                    image.GetRowSpan(top)[right].Red > Threshold2
                    && result.GetRowSpan(top)[right] != Color1
                )
                {
                    result.GetRowSpan(top)[right] = Color1;
                    tempPixels.Push(new Tuple<int, int>(right, top));
                }
            }
        }
    }
}
