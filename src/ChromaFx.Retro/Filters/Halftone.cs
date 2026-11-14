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
/// Rotated circular-dot halftone screen (newspaper / zine print look-alike).
/// Darker tones get larger black dots on a light paper; not a true CMYK plate sim.
/// </summary>
/// <param name="cellSize">
/// Dot cell size in pixels (clamped to 2…64). Default 6.
/// </param>
/// <param name="angleDegrees">
/// Screen rotation in degrees. Default 45 (classic black plate angle vibe).
/// </param>
/// <param name="strength">
/// Blend toward the screened result (0–1). Default 1 (full replace).
/// </param>
public sealed class Halftone(int cellSize = 6, float angleDegrees = 45f, float strength = 1f)
    : IFilter
{
    /// <summary>
    /// Gets the dot cell size in pixels.
    /// </summary>
    public int CellSize { get; } = cellSize;

    /// <summary>
    /// Gets the screen angle in degrees.
    /// </summary>
    public float AngleDegrees { get; } = angleDegrees;

    /// <summary>
    /// Gets the blend strength toward the screened result (0–1).
    /// </summary>
    public float Strength { get; } = strength;

    /// <inheritdoc />
    public Image Apply(Image image, Rectangle targetLocation = default)
    {
        if (image.Width <= 0 || image.Height <= 0)
            return image;

        var cell = Math.Clamp(CellSize, 2, 64);
        var amount = Math.Clamp(Strength, 0f, 1f);
        if (amount <= 0f)
            return image;

        targetLocation = targetLocation.Normalize(image);
        var source = image.Clone();
        var rad = AngleDegrees * (MathF.PI / 180f);
        var cos = MathF.Cos(rad);
        var sin = MathF.Sin(rad);
        var half = cell * 0.5f;
        // Max radius so a full-black cell still tiles without large gaps at 45°.
        var maxRadius = half * MathF.Sqrt(2f);

        Parallel.For(
            targetLocation.Bottom,
            targetLocation.Top,
            y =>
            {
                var row = image.GetRowSpan(y);
                for (var x = targetLocation.Left; x < targetLocation.Right; x++)
                {
                    var src = source.GetRowSpan(y)[x];
                    // BT.601 luma → ink coverage (darker = more ink).
                    var luma = (0.299f * src.Red + 0.587f * src.Green + 0.114f * src.Blue) / 255f;
                    var coverage = 1f - luma;

                    var xr = x * cos + y * sin;
                    var yr = -x * sin + y * cos;
                    var dx = ModPositive(xr, cell) - half;
                    var dy = ModPositive(yr, cell) - half;
                    var dist = MathF.Sqrt(dx * dx + dy * dy);
                    var radius = maxRadius * MathF.Sqrt(Math.Clamp(coverage, 0f, 1f));

                    // Soft edge of ~0.75px so cells don't look like hard binary stamps at small sizes.
                    var edge = 0.75f;
                    var t = radius <= 0f ? 0f : 1f - SmoothStep(radius - edge, radius + edge, dist);
                    var ink = (byte)(t * 255f + 0.5f);
                    // Black ink on white paper; keep source alpha.
                    var screened = new Color(
                        (byte)(255 - ink),
                        (byte)(255 - ink),
                        (byte)(255 - ink),
                        src.Alpha
                    );

                    row[x] = amount >= 1f ? screened : Lerp(src, screened, amount);
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

    private static float SmoothStep(float edge0, float edge1, float x)
    {
        if (edge1 <= edge0)
            return x < edge0 ? 0f : 1f;
        var t = Math.Clamp((x - edge0) / (edge1 - edge0), 0f, 1f);
        return t * t * (3f - 2f * t);
    }

    private static Color Lerp(Color a, Color b, float t) =>
        new(
            (byte)(a.Red + (b.Red - a.Red) * t + 0.5f),
            (byte)(a.Green + (b.Green - a.Green) * t + 0.5f),
            (byte)(a.Blue + (b.Blue - a.Blue) * t + 0.5f),
            a.Alpha
        );
}
