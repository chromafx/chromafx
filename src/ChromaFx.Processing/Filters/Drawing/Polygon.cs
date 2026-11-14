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
/// Closed polygon (filled or stroked).
/// </summary>
/// <seealso cref="ShapeBaseClass"/>
/// <param name="color">Fill or stroke color.</param>
/// <param name="fill">When true, even-odd scanline fill; otherwise outline.</param>
/// <param name="points">Vertices in order (closed automatically).</param>
public class Polygon(Color color, bool fill, params Vector2[] points) : ShapeBaseClass(color)
{
    /// <summary>
    /// Gets or sets a value indicating whether the polygon is filled.
    /// </summary>
    public bool Fill { get; set; } = fill;

    /// <summary>
    /// Gets or sets the vertices.
    /// </summary>
    public Vector2[] Points { get; set; } = points;

    /// <summary>
    /// Gets or sets the stroke width used when <see cref="Fill"/> is false.
    /// </summary>
    public int StrokeWidth { get; set; } = 1;

    /// <inheritdoc />
    public override Image Apply(Image image, Numerics.Rectangle targetLocation = default)
    {
        targetLocation =
            targetLocation == default
                ? new Numerics.Rectangle(0, 0, image.Width, image.Height)
                : targetLocation.Clamp(image);

        if (Points is null || Points.Length < 3)
            return image;

        return Fill ? FillPolygon(image, targetLocation) : StrokePolygon(image, targetLocation);
    }

    private Image StrokePolygon(Image image, Numerics.Rectangle targetLocation)
    {
        var width = Math.Max(1, StrokeWidth);
        for (var i = 0; i < Points.Length; i++)
        {
            var next = (i + 1) % Points.Length;
            DrawingStroke.DrawSegment(
                this,
                image,
                (int)Points[i].X,
                (int)Points[i].Y,
                (int)Points[next].X,
                (int)Points[next].Y,
                width,
                targetLocation
            );
        }

        return image;
    }

    private Image FillPolygon(Image image, Numerics.Rectangle targetLocation)
    {
        var n = Points.Length;
        var xs = new int[n];
        var ys = new int[n];
        var minY = int.MaxValue;
        var maxY = int.MinValue;
        for (var i = 0; i < n; i++)
        {
            xs[i] = (int)Points[i].X;
            ys[i] = (int)Points[i].Y;
            if (ys[i] < minY)
                minY = ys[i];
            if (ys[i] > maxY)
                maxY = ys[i];
        }

        minY = Math.Max(minY, targetLocation.Bottom);
        maxY = Math.Min(maxY, targetLocation.Top - 1);
        if (minY > maxY)
            return image;

        var intersections = new List<int>(n);
        for (var y = minY; y <= maxY; y++)
        {
            intersections.Clear();
            for (var i = 0; i < n; i++)
            {
                var j = (i + 1) % n;
                var y0 = ys[i];
                var y1 = ys[j];
                if (y0 == y1)
                    continue;
                if ((y < y0 && y < y1) || (y >= y0 && y >= y1))
                    continue;

                var x0 = xs[i];
                var x1 = xs[j];
                var x = x0 + (int)((long)(y - y0) * (x1 - x0) / (y1 - y0));
                intersections.Add(x);
            }

            intersections.Sort();
            var row = image.GetRowSpan(y);
            for (var k = 0; k + 1 < intersections.Count; k += 2)
            {
                var left = Math.Max(intersections[k], targetLocation.Left);
                var right = Math.Min(intersections[k + 1], targetLocation.Right - 1);
                for (var x = left; x <= right; x++)
                    row[x] = Color;
            }
        }

        return image;
    }
}
