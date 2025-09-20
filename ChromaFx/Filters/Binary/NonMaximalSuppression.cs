/*
Copyright 2025 Ho Tzin Mein

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using ChromaFx.Filters.ColorMatrix;
using ChromaFx.Filters.Interfaces;
using ChromaFx.Numerics;
using ChromaFx.Colors;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChromaFx.Filters.Binary;

/// <summary>
/// Non-maximal suppression filter
/// </summary>
/// <seealso cref="IFilter"/>
public class NonMaximalSuppression : IFilter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NonMaximalSuppression"/> class.
    /// </summary>
    /// <param name="color1">The first color.</param>
    /// <param name="color2">The second color.</param>
    /// <param name="threshold1">The threshold1.</param>
    /// <param name="threshold2">The threshold2.</param>
    public NonMaximalSuppression(Color color1, Color color2, float threshold1, float threshold2)
    {
        Threshold2 = threshold2 * 255;
        Threshold1 = threshold1 * 255;
        Color1 = color1;
        Color2 = color2;
    }

    /// <summary>
    /// Gets or sets the color1.
    /// </summary>
    /// <value>The color1.</value>
    public Color Color1 { get; set; }

    /// <summary>
    /// Gets or sets the color2.
    /// </summary>
    /// <value>The color2.</value>
    public Color Color2 { get; set; }

    /// <summary>
    /// Gets or sets the threshold1.
    /// </summary>
    /// <value>The threshold1.</value>
    public float Threshold1 { get; set; }

    /// <summary>
    /// Gets or sets the threshold2.
    /// </summary>
    /// <value>The threshold2.</value>
    public float Threshold2 { get; set; }

    /// <summary>
    /// Applies the filter to the specified image.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <param name="targetLocation">The target location.</param>
    /// <returns>The image</returns>
    public Image Apply(Image image, Rectangle targetLocation = default)
    {
        targetLocation = targetLocation == default ? new Rectangle(0, 0, image.Width, image.Height) : targetLocation.Clamp(image);
        new Greyscale709().Apply(image, targetLocation);
        var result = new Image(image.Width, image.Height, new Color[image.Pixels.Length]);
        Array.Copy(image.Pixels, result.Pixels, result.Pixels.Length);
        new Drawing.Rectangle(Color2, true, targetLocation).Apply(result);
        Parallel.For(targetLocation.Bottom, targetLocation.Top, y =>
        {
            for (var x = targetLocation.Left; x < targetLocation.Right; ++x)
            {
                if (image.Pixels[y * image.Width + x].Red >= Threshold1)
                    FillPixels(result.Pixels, image, x, y, targetLocation);
            }
        });
        return image.ReCreate(image.Width, image.Height, result.Pixels);
    }

    private void FillPixels(Color[] result, Image image, int x, int y, Rectangle targetLocation)
    {
        var tempPixels = new Stack<Tuple<int, int>>();
        tempPixels.Push(new Tuple<int, int>(x, y));
        while (tempPixels.Count > 0)
        {
            var currentPixel = tempPixels.Pop();
            var left = currentPixel.Item1 - 1;
            if (left >= targetLocation.Left)
            {
                if (image.Pixels[currentPixel.Item2 * image.Width + left].Red > Threshold2
                    && result[currentPixel.Item2 * image.Width + left] != Color1)
                {
                    result[currentPixel.Item2 * image.Width + left] = Color1;
                    tempPixels.Push(new Tuple<int, int>(left, currentPixel.Item2));
                }
            }
            var right = currentPixel.Item1 + 1;
            if (right < targetLocation.Right)
            {
                if (image.Pixels[currentPixel.Item2 * image.Width + right].Red > Threshold2
                    && result[currentPixel.Item2 * image.Width + right] != Color1)
                {
                    result[currentPixel.Item2 * image.Width + right] = Color1;
                    tempPixels.Push(new Tuple<int, int>(right, currentPixel.Item2));
                }
            }
            var bottom = currentPixel.Item2 - 1;
            if (bottom >= targetLocation.Bottom)
            {
                if (image.Pixels[bottom * image.Width + currentPixel.Item1].Red > Threshold2
                    && result[bottom * image.Width + currentPixel.Item1] != Color1)
                {
                    result[bottom * image.Width + currentPixel.Item1] = Color1;
                    tempPixels.Push(new Tuple<int, int>(currentPixel.Item1, bottom));
                }
            }
            var top = currentPixel.Item2 + 1;
            if (top < targetLocation.Top)
            {
                if (image.Pixels[top * image.Width + currentPixel.Item1].Red > Threshold2
                    && result[top * image.Width + currentPixel.Item1] != Color1)
                {
                    result[top * image.Width + currentPixel.Item1] = Color1;
                    tempPixels.Push(new Tuple<int, int>(currentPixel.Item1, top));
                }
            }
            if (left >= targetLocation.Left && bottom >= targetLocation.Bottom)
            {
                if (image.Pixels[bottom * image.Width + left].Red > Threshold2
                    && result[bottom * image.Width + left] != Color1)
                {
                    result[bottom * image.Width + left] = Color1;
                    tempPixels.Push(new Tuple<int, int>(left, bottom));
                }
            }
            if (left >= targetLocation.Left && top < targetLocation.Top)
            {
                if (image.Pixels[top * image.Width + left].Red > Threshold2
                    && result[top * image.Width + left] != Color1)
                {
                    result[top * image.Width + left] = Color1;
                    tempPixels.Push(new Tuple<int, int>(left, top));
                }
            }
            if (right < targetLocation.Right && bottom >= targetLocation.Bottom)
            {
                if (image.Pixels[bottom * image.Width + right].Red > Threshold2
                    && result[bottom * image.Width + right] != Color1)
                {
                    result[bottom * image.Width + right] = Color1;
                    tempPixels.Push(new Tuple<int, int>(right, bottom));
                }
            }
            if (right < targetLocation.Left && top < targetLocation.Top)
            {
                if (image.Pixels[top * image.Width + right].Red > Threshold2
                    && result[top * image.Width + right] != Color1)
                {
                    result[top * image.Width + right] = Color1;
                    tempPixels.Push(new Tuple<int, int>(right, top));
                }
            }
        }
    }
}