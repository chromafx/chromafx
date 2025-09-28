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

using ChromaFx.Core.Colors;
using System.Numerics;
using ChromaFx.Processing.Filters.Drawing.BaseClasses;
using ChromaFx.Core;

namespace ChromaFx.Processing.Filters.Drawing;

/// <summary>
/// Ellipse drawing class
/// </summary>
/// <seealso cref="ShapeBaseClass"/>
/// <remarks>
/// Initializes a new instance of the <see cref="Ellipse"/> class.
/// </remarks>
/// <param name="color">The color.</param>
/// <param name="fill">if set to <c>true</c> [fill].</param>
/// <param name="width">The width.</param>
/// <param name="height">The height.</param>
/// <param name="center">The center.</param>
public class Ellipse(Color color, bool fill, int width, int height, Vector2 center) : ShapeBaseClass(color)
{

    /// <summary>
    /// Gets or sets the center.
    /// </summary>
    /// <value>The center.</value>
    public Vector2 Center { get; set; } = center;

    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="Ellipse"/> is fill.
    /// </summary>
    /// <value><c>true</c> if fill; otherwise, <c>false</c>.</value>
    public bool Fill { get; set; } = fill;

    /// <summary>
    /// Gets or sets the height.
    /// </summary>
    /// <value>The height.</value>
    public int Height { get; set; } = height;

    /// <summary>
    /// Gets or sets the width.
    /// </summary>
    /// <value>The width.</value>
    public int Width { get; set; } = width;

    /// <summary>
    /// Applies the specified image.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <param name="targetLocation">The target location.</param>
    /// <returns></returns>
    public override Image Apply(Image image, Numerics.Rectangle targetLocation = default)
    {
        targetLocation = targetLocation == default ? new Numerics.Rectangle(0, 0, image.Width, image.Height) : targetLocation.Clamp(image);
        if (Fill)
        {
            DrawFilledEllipse(image, targetLocation);
        }
        else
        {
            DrawEllipse(image, targetLocation);
        }
        return image;
    }

    private void DrawEllipse(Image image, Numerics.Rectangle targetLocation)
    {
        var twoASquare = 2 * Width * Width;
        var twoBSquare = 2 * Height * Height;
        var x = Width;
        var y = 0;
        var xChange = Height * Height * (1 - 2 * Width);
        var yChange = Width * Width;
        var ellipseError = 0;
        var stoppingX = twoBSquare * Width;
        var stoppingY = 0;
        while (stoppingX >= stoppingY)
        {
            Plot4EllipsePoints(image, x, y, targetLocation);
            ++y;
            stoppingY += twoASquare;
            ellipseError += yChange;
            yChange += twoASquare;
            if (2 * ellipseError + xChange > 0)
            {
                --x;
                stoppingX -= twoBSquare;
                ellipseError += xChange;
                xChange += twoBSquare;
            }
        }
        x = 0;
        y = Height;
        xChange = Height * Height;
        yChange = Width * Width * (1 - 2 * Height);
        ellipseError = 0;
        stoppingX = 0;
        stoppingY = twoASquare * Height;
        while (stoppingX <= stoppingY)
        {
            Plot4EllipsePoints(image, x, y, targetLocation);
            ++x;
            stoppingX += twoBSquare;
            ellipseError += xChange;
            xChange += twoBSquare;
            if (2 * ellipseError + yChange > 0)
            {
                --y;
                stoppingY -= twoASquare;
                ellipseError += yChange;
                yChange += twoASquare;
            }
        }
    }

    private void DrawFilledEllipse(Image image, Numerics.Rectangle targetLocation)
    {
        var hh = Height * Height;
        var ww = Width * Width;
        var hhww = hh * ww;
        var x0 = Width;
        var dx = 0;

        for (var x = -Width; x <= Width; ++x)
            Plot(image, (int)Center.X + x, (int)Center.Y, 1, targetLocation);

        for (var y = 1; y <= Height; ++y)
        {
            var x1 = x0 - (dx - 1);
            for (; x1 > 0; x1--)
                if (x1 * x1 * hh + y * y * ww <= hhww)
                    break;
            dx = x0 - x1;
            x0 = x1;

            for (var x = -x0; x <= x0; x++)
            {
                Plot(image, (int)Center.X + x, (int)Center.Y - y, 1, targetLocation);
                Plot(image, (int)Center.X + x, (int)Center.Y + y, 1, targetLocation);
            }
        }
    }

    /// <summary>
    /// Plots 4 points along the ellipse.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <param name="targetLocation">The target location.</param>
    private void Plot4EllipsePoints(Image image, int x, int y, Numerics.Rectangle targetLocation)
    {
        Plot(image, (int)(Center.X + x), (int)(Center.Y + y), 1, targetLocation);
        Plot(image, (int)(Center.X - x), (int)(Center.Y + y), 1, targetLocation);
        Plot(image, (int)(Center.X - x), (int)(Center.Y - y), 1, targetLocation);
        Plot(image, (int)(Center.X + x), (int)(Center.Y - y), 1, targetLocation);
    }
}