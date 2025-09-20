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

using ChromaFx.Filters.Drawing;
using ChromaFx.Filters.Interfaces;
using ChromaFx.Colors;
using System.Numerics;

namespace ChromaFx.Filters.Effects;

/// <summary>
/// Pointillism filter
/// </summary>
/// <seealso cref="IFilter"/>
public class Pointillism : IFilter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Pointillism"/> class.
    /// </summary>
    /// <param name="pointSize">Size of the points.</param>
    public Pointillism(int pointSize)
    {
        PointSize = pointSize;
    }

    /// <summary>
    /// Gets or sets the size of the points.
    /// </summary>
    /// <value>The size of the points.</value>
    public int PointSize { get; set; }

    /// <summary>
    /// Applies the filter to the specified image.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <param name="targetLocation">The target location.</param>
    /// <returns>The image</returns>
    public Image Apply(Image image, Numerics.Rectangle targetLocation = default)
    {
        targetLocation = targetLocation == default ? new Numerics.Rectangle(0, 0, image.Width, image.Height) : targetLocation.Clamp(image);
        var pointSize2 = PointSize * 2;
        var copy = new Color[image.Pixels.Length];
        Array.Copy(image.Pixels, copy, copy.Length);
        var ellipseDrawer = new Ellipse(Color.AliceBlue, true, PointSize, PointSize, new Vector2(0, 0));

        for (var y = targetLocation.Bottom; y < targetLocation.Top; y += pointSize2)
        {
            var minY = Math.Clamp((y - PointSize), targetLocation.Bottom, targetLocation.Top - 1);
            var maxY = Math.Clamp((y + PointSize), targetLocation.Bottom, targetLocation.Top - 1);
            for (var x = targetLocation.Left; x < targetLocation.Right; x += pointSize2)
            {
                uint rValue = 0;
                uint gValue = 0;
                uint bValue = 0;
                var minX = Math.Clamp((x - PointSize), targetLocation.Left, targetLocation.Right - 1);
                var maxX = Math.Clamp((x + PointSize), targetLocation.Left, targetLocation.Right - 1);
                var numberPixels = 0;
                for (var x2 = minX; x2 < maxX; ++x2)
                {
                    for (var y2 = minY; y2 < maxY; ++y2)
                    {
                        var offset = y * image.Width + x;
                        rValue += copy[offset].Red;
                        gValue += copy[offset].Green;
                        bValue += copy[offset].Blue;
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
        for (var y = targetLocation.Bottom + PointSize; y < targetLocation.Top; y += pointSize2)
        {
            var minY = Math.Clamp((y - PointSize), targetLocation.Bottom, targetLocation.Top - 1);
            var maxY = Math.Clamp((y + PointSize), targetLocation.Bottom, targetLocation.Top - 1);
            for (var x = targetLocation.Left + PointSize; x < targetLocation.Right; x += pointSize2)
            {
                uint rValue = 0;
                uint gValue = 0;
                uint bValue = 0;
                var minX = Math.Clamp((x - PointSize), targetLocation.Left, targetLocation.Right - 1);
                var maxX = Math.Clamp((x + PointSize), targetLocation.Left, targetLocation.Right - 1);
                var numberPixels = 0;
                for (var x2 = minX; x2 < maxX; ++x2)
                {
                    for (var y2 = minY; y2 < maxY; ++y2)
                    {
                        var offset = y * image.Width + x;
                        rValue += copy[offset].Red;
                        gValue += copy[offset].Green;
                        bValue += copy[offset].Blue;
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
        for (var y = targetLocation.Bottom; y < targetLocation.Top; y += pointSize2)
        {
            var tempY = y + new Random(y).Next(-PointSize, PointSize);
            var minY = Math.Clamp((tempY - PointSize), targetLocation.Bottom, targetLocation.Top - 1);
            var maxY = Math.Clamp((tempY + PointSize), targetLocation.Bottom, targetLocation.Top - 1);
            for (var x = targetLocation.Left + PointSize; x < targetLocation.Right; x += pointSize2)
            {
                uint rValue = 0;
                uint gValue = 0;
                uint bValue = 0;
                var tempX = x + new Random(x).Next(-PointSize, PointSize);
                var minX = Math.Clamp((tempX - PointSize), targetLocation.Left, targetLocation.Right - 1);
                var maxX = Math.Clamp((tempX + PointSize), targetLocation.Left, targetLocation.Right - 1);
                var numberPixels = 0;
                for (var x2 = minX; x2 < maxX; ++x2)
                {
                    for (var y2 = minY; y2 < maxY; ++y2)
                    {
                        var offset = y * image.Width + x;
                        rValue += copy[offset].Red;
                        gValue += copy[offset].Green;
                        bValue += copy[offset].Blue;
                        ++numberPixels;
                    }
                }
                rValue /= (uint)numberPixels;
                gValue /= (uint)numberPixels;
                bValue /= (uint)numberPixels;
                ellipseDrawer.Center = new Vector2(tempX, tempY);
                ellipseDrawer.Color = new Color((byte)rValue, (byte)gValue, (byte)bValue);
                ellipseDrawer.Apply(image, targetLocation);
            }
        }
        return image;
    }
}
