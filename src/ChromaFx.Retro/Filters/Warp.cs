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
/// Classic plug-in warp: twirl, pinch, or sphere via inverse mapping. Stylized — not physics.
/// </summary>
/// <param name="mode">Warp shape. Default <see cref="WarpMode.Twirl"/>.</param>
/// <param name="amount">
/// Strength (twirl: radians-ish scale; pinch: −1…1; sphere: 0…1 bulge). Default 1.
/// </param>
/// <param name="radius">
/// Effect radius as a fraction of half-min-dimension (0–1). Default 0.85.
/// </param>
/// <param name="centerX">Center X fraction (0–1). Default 0.5.</param>
/// <param name="centerY">Center Y fraction (0–1). Default 0.5.</param>
public sealed class Warp(
    WarpMode mode = WarpMode.Twirl,
    float amount = 1f,
    float radius = 0.85f,
    float centerX = 0.5f,
    float centerY = 0.5f
) : IFilter
{
    public WarpMode Mode { get; } = mode;
    public float Amount { get; } = amount;
    public float Radius { get; } = radius;
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
        var maxR = Math.Max(1f, MathF.Min(w, h) * 0.5f * Math.Clamp(Radius, 0.05f, 1.5f));
        var amt = Amount;

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
                    float sx = x,
                        sy = y;

                    if (dist < maxR && dist > 1e-4f)
                    {
                        var t = dist / maxR;
                        switch (Mode)
                        {
                            case WarpMode.Twirl:
                            {
                                var angle = MathF.Atan2(dy, dx) + amt * (1f - t);
                                sx = cx + MathF.Cos(angle) * dist;
                                sy = cy + MathF.Sin(angle) * dist;
                                break;
                            }
                            case WarpMode.Pinch:
                            {
                                // Positive amount pinches in; negative punches out.
                                var pinch = Math.Clamp(amt, -2f, 2f);
                                var factor = MathF.Pow(t, 1f + pinch);
                                var newDist = factor * maxR;
                                var scale = newDist / dist;
                                sx = cx + dx * scale;
                                sy = cy + dy * scale;
                                break;
                            }
                            case WarpMode.Sphere:
                            {
                                var bulge = Math.Clamp(amt, 0f, 2f);
                                // Inverse sphere: map destination back through a bulge.
                                var z = MathF.Sqrt(Math.Max(0f, 1f - t * t));
                                var k = 1f / (1f + bulge * z);
                                sx = cx + dx * k;
                                sy = cy + dy * k;
                                break;
                            }
                        }
                    }

                    row[x] = SampleBilinear(source, sx, sy);
                }
            }
        );

        return image;
    }

    private static Color SampleBilinear(Image image, float fx, float fy)
    {
        var w = image.Width;
        var h = image.Height;
        if (fx < 0f || fy < 0f || fx > w - 1 || fy > h - 1)
        {
            var ix = Math.Clamp((int)MathF.Round(fx), 0, w - 1);
            var iy = Math.Clamp((int)MathF.Round(fy), 0, h - 1);
            return image.GetRowSpan(iy)[ix];
        }

        var x0 = (int)MathF.Floor(fx);
        var y0 = (int)MathF.Floor(fy);
        var x1 = Math.Min(x0 + 1, w - 1);
        var y1 = Math.Min(y0 + 1, h - 1);
        var tx = fx - x0;
        var ty = fy - y0;
        var c00 = image.GetRowSpan(y0)[x0];
        var c10 = image.GetRowSpan(y0)[x1];
        var c01 = image.GetRowSpan(y1)[x0];
        var c11 = image.GetRowSpan(y1)[x1];
        return new Color(
            Bilerp(c00.Red, c10.Red, c01.Red, c11.Red, tx, ty),
            Bilerp(c00.Green, c10.Green, c01.Green, c11.Green, tx, ty),
            Bilerp(c00.Blue, c10.Blue, c01.Blue, c11.Blue, tx, ty),
            Bilerp(c00.Alpha, c10.Alpha, c01.Alpha, c11.Alpha, tx, ty)
        );
    }

    private static byte Bilerp(byte a, byte b, byte c, byte d, float tx, float ty)
    {
        var top = a + (b - a) * tx;
        var bot = c + (d - c) * tx;
        return (byte)(top + (bot - top) * ty + 0.5f);
    }
}
