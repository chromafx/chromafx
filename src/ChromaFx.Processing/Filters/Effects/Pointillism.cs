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

using System.Numerics;
using ChromaFx.Core;
using ChromaFx.Core.Colors;
using ChromaFx.Processing.Filters.Drawing;
using ChromaFx.Processing.Filters.Interfaces;
using ChromaFx.Processing.Numerics;
using SysRandom = System.Random;

namespace ChromaFx.Processing.Filters.Effects;

/// <summary>
/// Pointillism filter
/// </summary>
/// <seealso cref="IFilter"/>
/// <remarks>
/// Initializes a new instance of the <see cref="Pointillism"/> class.
/// </remarks>
/// <param name="pointSize">Size of the points.</param>
public class Pointillism(int pointSize) : IFilter
{
    /// <summary>
    /// Gets or sets the size of the points.
    /// </summary>
    /// <value>The size of the points.</value>
    public int PointSize { get; set; } = pointSize;

    /// <summary>
    /// Applies the filter to the specified image.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <param name="targetLocation">The target location.</param>
    /// <returns>The image</returns>
    public Image Apply(Image image, Numerics.Rectangle targetLocation = default)
    {
        if (PointSize <= 0)
            return image;

        targetLocation = targetLocation.Normalize(image);
        var pointSize2 = PointSize * 2;
        var copy = FilterBufferPool.RentTightCopy(image);
        try
        {
            var ellipseDrawer = new Ellipse(
                Color.AliceBlue,
                true,
                PointSize,
                PointSize,
                new Vector2(0, 0)
            );

            for (var y = targetLocation.Bottom; y < targetLocation.Top; y += pointSize2)
            {
                for (var x = targetLocation.Left; x < targetLocation.Right; x += pointSize2)
                {
                    PaintDot(image, copy, ellipseDrawer, targetLocation, x, y);
                }
            }
            for (var y = targetLocation.Bottom + PointSize; y < targetLocation.Top; y += pointSize2)
            {
                for (var x = targetLocation.Left + PointSize; x < targetLocation.Right; x += pointSize2)
                {
                    PaintDot(image, copy, ellipseDrawer, targetLocation, x, y);
                }
            }
            for (var y = targetLocation.Bottom; y < targetLocation.Top; y += pointSize2)
            {
                var tempY = y + new SysRandom(y).Next(-PointSize, PointSize);
                for (var x = targetLocation.Left + PointSize; x < targetLocation.Right; x += pointSize2)
                {
                    var tempX = x + new SysRandom(x).Next(-PointSize, PointSize);
                    PaintDot(image, copy, ellipseDrawer, targetLocation, tempX, tempY);
                }
            }
        }
        finally
        {
            FilterBufferPool.Return(copy);
        }

        return image;
    }

    private void PaintDot(
        Image image,
        Color[] copy,
        Ellipse ellipseDrawer,
        Numerics.Rectangle targetLocation,
        int x,
        int y
    )
    {
        var minY = Math.Clamp(y - PointSize, targetLocation.Bottom, targetLocation.Top - 1);
        var maxY = Math.Clamp(y + PointSize, targetLocation.Bottom, targetLocation.Top);
        if (maxY <= minY)
            maxY = minY + 1;

        var minX = Math.Clamp(x - PointSize, targetLocation.Left, targetLocation.Right - 1);
        var maxX = Math.Clamp(x + PointSize, targetLocation.Left, targetLocation.Right);
        if (maxX <= minX)
            maxX = minX + 1;

        uint rValue = 0;
        uint gValue = 0;
        uint bValue = 0;
        var numberPixels = 0;
        for (var y2 = minY; y2 < maxY; ++y2)
        {
            var rowStart = y2 * image.Width;
            for (var x2 = minX; x2 < maxX; ++x2)
            {
                var sample = copy[rowStart + x2];
                rValue += sample.Red;
                gValue += sample.Green;
                bValue += sample.Blue;
                ++numberPixels;
            }
        }

        rValue /= (uint)numberPixels;
        gValue /= (uint)numberPixels;
        bValue /= (uint)numberPixels;
        ellipseDrawer.Center = new Vector2(x, y);
        ellipseDrawer.Color = new Color((byte)rValue, (byte)gValue, (byte)bValue);
        ellipseDrawer.Apply(image, targetLocation);
    }
}
