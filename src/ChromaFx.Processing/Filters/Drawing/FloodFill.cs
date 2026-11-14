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
using ChromaFx.Core.Colors;

namespace ChromaFx.Processing.Filters.Drawing;

/// <summary>
/// 4-connected flood fill from a seed pixel (exact color match).
/// </summary>
/// <seealso cref="ShapeBaseClass"/>
/// <param name="color">Replacement color.</param>
/// <param name="x">Seed X.</param>
/// <param name="y">Seed Y.</param>
public class FloodFill(Color color, int x, int y) : ShapeBaseClass(color)
{
    /// <summary>
    /// Gets or sets the seed X coordinate.
    /// </summary>
    public int X { get; set; } = x;

    /// <summary>
    /// Gets or sets the seed Y coordinate.
    /// </summary>
    public int Y { get; set; } = y;

    /// <inheritdoc />
    public override Image Apply(Image image, Numerics.Rectangle targetLocation = default)
    {
        targetLocation =
            targetLocation == default
                ? new Numerics.Rectangle(0, 0, image.Width, image.Height)
                : targetLocation.Clamp(image);

        if (!targetLocation.Contains(X, Y))
            return image;

        var target = image.GetRowSpan(Y)[X];
        if (target == Color)
            return image;

        var queue = new Queue<(int X, int Y)>();
        queue.Enqueue((X, Y));
        image.GetRowSpan(Y)[X] = Color;

        while (queue.Count > 0)
        {
            var (cx, cy) = queue.Dequeue();
            TryEnqueue(image, targetLocation, target, queue, cx - 1, cy);
            TryEnqueue(image, targetLocation, target, queue, cx + 1, cy);
            TryEnqueue(image, targetLocation, target, queue, cx, cy - 1);
            TryEnqueue(image, targetLocation, target, queue, cx, cy + 1);
        }

        return image;
    }

    private void TryEnqueue(
        Image image,
        Numerics.Rectangle targetLocation,
        Color target,
        Queue<(int X, int Y)> queue,
        int x,
        int y
    )
    {
        if (!targetLocation.Contains(x, y))
            return;
        ref var pixel = ref image.GetRowSpan(y)[x];
        if (pixel != target)
            return;
        pixel = Color;
        queue.Enqueue((x, y));
    }
}
