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

using ChromaFx.Core;

namespace ChromaFx.Processing.Filters.Drawing;

/// <summary>
/// Shared stroke helpers for polyline / polygon outlines.
/// </summary>
internal static class DrawingStroke
{
    /// <summary>
    /// Draws a segment. Width 1 uses Xiaolin Wu anti-aliasing; wider strokes use an opaque square brush.
    /// </summary>
    public static void DrawSegment(
        ShapeBaseClass shape,
        Image image,
        int x1,
        int y1,
        int x2,
        int y2,
        int strokeWidth,
        Numerics.Rectangle targetLocation
    )
    {
        if (strokeWidth <= 1)
        {
            new Line(shape.Color, x1, y1, x2, y2).Apply(image, targetLocation);
            return;
        }

        var radius = strokeWidth / 2;
        Bresenham(
            x1,
            y1,
            x2,
            y2,
            (x, y) => StampSquare(shape, image, x, y, radius, targetLocation)
        );
    }

    private static void StampSquare(
        ShapeBaseClass shape,
        Image image,
        int cx,
        int cy,
        int radius,
        Numerics.Rectangle targetLocation
    )
    {
        for (var dy = -radius; dy <= radius; dy++)
        {
            for (var dx = -radius; dx <= radius; dx++)
                shape.Plot(image, cx + dx, cy + dy, 1f, targetLocation);
        }
    }

    private static void Bresenham(int x0, int y0, int x1, int y1, Action<int, int> plot)
    {
        var dx = Math.Abs(x1 - x0);
        var dy = Math.Abs(y1 - y0);
        var sx = x0 < x1 ? 1 : -1;
        var sy = y0 < y1 ? 1 : -1;
        var err = dx - dy;

        for (; ; )
        {
            plot(x0, y0);
            if (x0 == x1 && y0 == y1)
                break;
            var e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }
}
