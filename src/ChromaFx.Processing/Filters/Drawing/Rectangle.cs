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
/// Rectangle drawing class
/// </summary>
/// <seealso cref="ShapeBaseClass"/>
/// <remarks>
/// Initializes a new instance of the <see cref="Rectangle"/> class.
/// </remarks>
/// <param name="color">The color.</param>
/// <param name="fill">if set to <c>true</c> [fill].</param>
/// <param name="bounds">The bounds.</param>
public class Rectangle(Color color, bool fill, Numerics.Rectangle bounds) : ShapeBaseClass(color)
{

    /// <summary>
    /// Gets or sets the bounds.
    /// </summary>
    /// <value>The bounds.</value>
    public Numerics.Rectangle Bounds { get; set; } = bounds;

    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="Rectangle"/> is fill.
    /// </summary>
    /// <value><c>true</c> if fill; otherwise, <c>false</c>.</value>
    public bool Fill { get; set; } = fill;

    /// <summary>
    /// Applies the specified image.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <param name="targetLocation">The target location.</param>
    /// <returns></returns>
    public override Image Apply(Image image, Numerics.Rectangle targetLocation = default)
    {
        targetLocation =
            targetLocation == default
                ? new Numerics.Rectangle(0, 0, image.Width, image.Height)
                : targetLocation.Clamp(image);
        Bounds = Bounds.Clamp(targetLocation);
        return Fill
            ? DrawFilledRectangle(image, Bounds)
            : DrawRectangleOutline(image, targetLocation);
    }

    /// <summary>
    /// Draws the filled rectangle.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <param name="targetLocation">The target location.</param>
    /// <returns>The resulting image</returns>
    private Image DrawFilledRectangle(Image image, Numerics.Rectangle targetLocation)
    {
        Parallel.For(
            targetLocation.Bottom,
            targetLocation.Top,
            y =>
            {
                for (var x = targetLocation.Left; x < targetLocation.Right; ++x)
                {
                    var index = y * image.Width + x;
                    image.Pixels[index] = Color;
                }
            }
        );

        return image;
    }

    /// <summary>
    /// Draws the rectangle outline.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <param name="targetLocation">The target location.</param>
    /// <returns>The resulting image</returns>
    private Image DrawRectangleOutline(Image image, Numerics.Rectangle targetLocation)
    {
        new Line(Color, Bounds.Left, Bounds.Bottom, Bounds.Right, Bounds.Bottom).Apply(
            image,
            targetLocation
        );
        new Line(Color, Bounds.Left, Bounds.Top, Bounds.Right, Bounds.Top).Apply(
            image,
            targetLocation
        );
        new Line(Color, Bounds.Left, Bounds.Bottom, Bounds.Left, Bounds.Top).Apply(
            image,
            targetLocation
        );
        new Line(Color, Bounds.Right, Bounds.Bottom, Bounds.Right, Bounds.Top).Apply(
            image,
            targetLocation
        );
        return image;
    }
}
