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

using ChromaFx.Core;
using ChromaFx.Core.Colors;
using ChromaFx.Processing.Filters.Drawing.BaseClasses;

namespace ChromaFx.Processing.Filters.Drawing;

/// <summary>
/// Line drawing item.
/// </summary>
/// <seealso cref="ShapeBaseClass"/>
/// <remarks>
/// Initializes a new instance of the <see cref="Line"/> class.
/// </remarks>
/// <param name="color">The color.</param>
/// <param name="x1">The x1.</param>
/// <param name="y1">The y1.</param>
/// <param name="x2">The x2.</param>
/// <param name="y2">The y2.</param>
public class Line(Color color, int x1, int y1, int x2, int y2) : ShapeBaseClass(color)
{

    /// <summary>
    /// Gets or sets the x1.
    /// </summary>
    /// <value>The x1.</value>
    public int X1 { get; set; } = x1;

    /// <summary>
    /// Gets or sets the x2.
    /// </summary>
    /// <value>The x2.</value>
    public int X2 { get; set; } = x2;

    /// <summary>
    /// Gets or sets the y1.
    /// </summary>
    /// <value>The y1.</value>
    public int Y1 { get; set; } = y1;

    /// <summary>
    /// Gets or sets the y2.
    /// </summary>
    /// <value>The y2.</value>
    public int Y2 { get; set; } = y2;

    /// <summary>
    /// Applies the specified image.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <param name="targetLocation">The target location.</param>
    /// <returns></returns>
    public override Image Apply(Image image, Numerics.Rectangle targetLocation = default)
    {
        targetLocation = targetLocation == default ? new Numerics.Rectangle(0, 0, image.Width, image.Height) : targetLocation.Clamp(image);
        var isSteep = Math.Abs(Y2 - Y1) > Math.Abs(X2 - X1);
        if (isSteep)
        {
            (Y1, X1) = (X1, Y1);
            (Y2, X2) = (X2, Y2);
        }
        if (X1 > X2)
        {
            (X2, X1) = (X1, X2);
            (Y2, Y1) = (Y1, Y2);
        }
        var changeX = X2 - X1;
        var changeY = Y2 - Y1;
        var gradiant = changeY / (float)changeX;
        var xEnd = Round(X1);
        var yEnd = Y1 + gradiant * (xEnd - X1);
        var xGap = RfPart(X1 + 0.5);
        var xPixel1 = (int)xEnd;
        var yPixel1 = (int)yEnd;
        if (isSteep)
        {
            Plot(image, yPixel1, xPixel1, (float)(RfPart(yEnd) * xGap), targetLocation);
            Plot(image, yPixel1 + 1, xPixel1, (float)(FractionalPart(yEnd) * xGap), targetLocation);
        }
        else
        {
            Plot(image, xPixel1, yPixel1, (float)(RfPart(yEnd) * xGap), targetLocation);
            Plot(image, xPixel1, yPixel1 + 1, (float)(FractionalPart(yEnd) * xGap), targetLocation);
        }
        var intery = yEnd + gradiant;

        xEnd = Round(X2);
        yEnd = Y2 + gradiant * (xEnd - X2);
        xGap = FractionalPart(X2 + 0.5);
        var xPixel2 = (int)xEnd;
        var yPixel2 = (int)yEnd;
        if (isSteep)
        {
            Plot(image, yPixel2, xPixel2, (float)(RfPart(yEnd) * xGap), targetLocation);
            Plot(image, yPixel2 + 1, xPixel2, (float)(FractionalPart(yEnd) * xGap), targetLocation);
        }
        else
        {
            Plot(image, xPixel2, yPixel2, (float)(RfPart(yEnd) * xGap), targetLocation);
            Plot(image, xPixel2, yPixel2 + 1, (float)(FractionalPart(yEnd) * xGap), targetLocation);
        }
        if (isSteep)
        {
            for (var x = xPixel1 + 1; x < xPixel2; ++x)
            {
                Plot(image, (int)intery, x, (float)RfPart(intery), targetLocation);
                Plot(image, (int)intery + 1, x, (float)FractionalPart(intery), targetLocation);
                intery += gradiant;
            }
        }
        else
        {
            for (var x = xPixel1 + 1; x < xPixel2; ++x)
            {
                Plot(image, x, (int)intery, (float)RfPart(intery), targetLocation);
                Plot(image, x, (int)intery + 1, (float)FractionalPart(intery), targetLocation);
                intery += gradiant;
            }
        }
        return image;
    }
}