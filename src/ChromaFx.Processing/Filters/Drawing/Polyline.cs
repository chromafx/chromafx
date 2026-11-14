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

using System.Numerics;
using ChromaFx.Core;
using ChromaFx.Core.Colors;

namespace ChromaFx.Processing.Filters.Drawing;

/// <summary>
/// Open polyline (connected segments; not closed).
/// </summary>
/// <seealso cref="ShapeBaseClass"/>
/// <param name="color">Stroke color.</param>
/// <param name="points">Vertices in order.</param>
public class Polyline(Color color, params Vector2[] points) : ShapeBaseClass(color)
{
    /// <summary>
    /// Gets or sets the vertices.
    /// </summary>
    public Vector2[] Points { get; set; } = points;

    /// <summary>
    /// Gets or sets the stroke width in pixels. Width 1 is anti-aliased; larger widths use an opaque square brush.
    /// </summary>
    public int StrokeWidth { get; set; } = 1;

    /// <inheritdoc />
    public override Image Apply(Image image, Numerics.Rectangle targetLocation = default)
    {
        targetLocation =
            targetLocation == default
                ? new Numerics.Rectangle(0, 0, image.Width, image.Height)
                : targetLocation.Clamp(image);

        if (Points is null || Points.Length < 2)
            return image;

        var width = Math.Max(1, StrokeWidth);
        for (var i = 0; i < Points.Length - 1; i++)
        {
            DrawingStroke.DrawSegment(
                this,
                image,
                (int)Points[i].X,
                (int)Points[i].Y,
                (int)Points[i + 1].X,
                (int)Points[i + 1].Y,
                width,
                targetLocation
            );
        }

        return image;
    }
}
