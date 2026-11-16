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
/// Polar vortex / tile remapping — radius and angle swirl into source UVs. Lite plug-in vibe.
/// </summary>
/// <param name="twist">Angular twist strength. Default 2.</param>
/// <param name="zoom">Radial zoom into the vortex (0.25–4). Default 1.</param>
/// <param name="centerX">Center X fraction (0–1). Default 0.5.</param>
/// <param name="centerY">Center Y fraction (0–1). Default 0.5.</param>
public sealed class VortexTile(
    float twist = 2f,
    float zoom = 1f,
    float centerX = 0.5f,
    float centerY = 0.5f
) : IFilter
{
    public float Twist { get; } = twist;
    public float Zoom { get; } = zoom;
    public float CenterX { get; } = centerX;
    public float CenterY { get; } = centerY;

    /// <inheritdoc />
    public Image Apply(Image image, Rectangle targetLocation = default)
    {
        if (image.Width <= 0 || image.Height <= 0)
            return image;

        targetLocation = targetLocation.Normalize(image);
        var source = image.Clone();
        var w = image.Width;
        var h = image.Height;
        var cx = Math.Clamp(CenterX, 0f, 1f) * (w - 1);
        var cy = Math.Clamp(CenterY, 0f, 1f) * (h - 1);
        var maxR = MathF.Sqrt(cx * cx + cy * cy) + 1f;
        var twist = Twist;
        var zoom = Math.Clamp(Zoom, 0.25f, 4f);

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
                    var angle = MathF.Atan2(dy, dx) + twist * (1f - dist / maxR);
                    var r = dist / zoom;
                    // Wrap radius for a tiled vortex feel.
                    var tile = maxR * 0.85f;
                    r = ModPositive(r, tile);
                    var sx = cx + MathF.Cos(angle) * r;
                    var sy = cy + MathF.Sin(angle) * r;
                    // Soft wrap into image bounds.
                    sx = ModPositive(sx, w);
                    sy = ModPositive(sy, h);
                    row[x] = Sample(source, sx, sy);
                }
            }
        );

        return image;
    }

    private static float ModPositive(float value, float modulus)
    {
        var m = value % modulus;
        return m < 0f ? m + modulus : m;
    }

    private static Color Sample(Image image, float fx, float fy)
    {
        var x = Math.Clamp((int)MathF.Floor(fx), 0, image.Width - 1);
        var y = Math.Clamp((int)MathF.Floor(fy), 0, image.Height - 1);
        return image.GetRowSpan(y)[x];
    }
}
