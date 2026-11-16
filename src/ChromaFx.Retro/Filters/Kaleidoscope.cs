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
using ChromaFx.Processing.Filters;
using ChromaFx.Processing.Numerics;

namespace ChromaFx.Retro.Filters;

/// <summary>
/// Polar/sector kaleidoscope mirror. Stylized plug-in feel.
/// </summary>
/// <param name="segments">Number of pie slices (clamped 2…24). Default 8.</param>
/// <param name="centerX">Center X fraction (0–1). Default 0.5.</param>
/// <param name="centerY">Center Y fraction (0–1). Default 0.5.</param>
public sealed class Kaleidoscope(int segments = 8, float centerX = 0.5f, float centerY = 0.5f)
    : IFilter
{
    public int Segments { get; } = segments;
    public float CenterX { get; } = centerX;
    public float CenterY { get; } = centerY;

    /// <inheritdoc />
    public Image Apply(Image image, Rectangle targetLocation = default)
    {
        if (image.Width <= 0 || image.Height <= 0)
            return image;

        var n = Math.Clamp(Segments, 2, 24);
        targetLocation = targetLocation.Normalize(image);
        var source = image.Clone();
        var w = image.Width;
        var h = image.Height;
        var cx = Math.Clamp(CenterX, 0f, 1f) * (w - 1);
        var cy = Math.Clamp(CenterY, 0f, 1f) * (h - 1);
        var sector = MathF.PI * 2f / n;

        Parallel.For(
            targetLocation.Bottom,
            targetLocation.Top,
            y =>
            {
                var row = image.GetRowSpan(y);
                for (var x = targetLocation.Left; x < targetLocation.Right; x++)
                {
                    var dx = x - cx;
                    var dy = y - cy;
                    var dist = MathF.Sqrt(dx * dx + dy * dy);
                    var angle = MathF.Atan2(dy, dx);
                    if (angle < 0f)
                        angle += MathF.PI * 2f;

                    var inSector = angle % sector;
                    // Mirror every other half-sector for a classic kaleidoscope.
                    if (inSector > sector * 0.5f)
                        inSector = sector - inSector;

                    var sx = cx + MathF.Cos(inSector) * dist;
                    var sy = cy + MathF.Sin(inSector) * dist;
                    row[x] = Sample(source, sx, sy);
                }
            }
        );

        return image;
    }

    private static Color Sample(Image image, float fx, float fy)
    {
        var x = Math.Clamp((int)MathF.Round(fx), 0, image.Width - 1);
        var y = Math.Clamp((int)MathF.Round(fy), 0, image.Height - 1);
        return image.GetRowSpan(y)[x];
    }
}
