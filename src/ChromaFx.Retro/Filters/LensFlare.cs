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
/// Dramatic lens flare look-alike: hot spot, anamorphic streak, and a few colored ghosts.
/// Stylized nostalgia — not a physical lens model.
/// </summary>
/// <param name="centerX">
/// Hot-spot X as a fraction of width (0–1). Default 0.32.
/// </param>
/// <param name="centerY">
/// Hot-spot Y as a fraction of height (0–1). Default 0.28.
/// </param>
/// <param name="strength">
/// Flare intensity (0–1). Default 0.85.
/// </param>
public sealed class LensFlare(float centerX = 0.32f, float centerY = 0.28f, float strength = 0.85f)
    : IFilter
{
    /// <summary>Gets the hot-spot X fraction (0–1).</summary>
    public float CenterX { get; } = centerX;

    /// <summary>Gets the hot-spot Y fraction (0–1).</summary>
    public float CenterY { get; } = centerY;

    /// <summary>Gets the flare strength (0–1).</summary>
    public float Strength { get; } = strength;

    /// <inheritdoc />
    public Image Apply(Image image, Rectangle targetLocation = default)
    {
        if (image.Width <= 0 || image.Height <= 0)
            return image;

        var amount = Math.Clamp(Strength, 0f, 1f);
        if (amount <= 0f)
            return image;

        targetLocation = targetLocation.Normalize(image);
        var w = image.Width;
        var h = image.Height;
        var hx = Math.Clamp(CenterX, 0f, 1f) * (w - 1);
        var hy = Math.Clamp(CenterY, 0f, 1f) * (h - 1);
        // Opposite point through image center — ghosts sit along this line.
        var ox = (w - 1) - hx;
        var oy = (h - 1) - hy;
        var scale = MathF.Min(w, h);

        Parallel.For(
            targetLocation.Bottom,
            targetLocation.Top,
            y =>
            {
                var row = image.GetRowSpan(y);
                for (var x = targetLocation.Left; x < targetLocation.Right; x++)
                {
                    ref var p = ref row[x];
                    var addR = 0f;
                    var addG = 0f;
                    var addB = 0f;

                    // Primary hot spot.
                    AddDisc(ref addR, ref addG, ref addB, x, y, hx, hy, scale * 0.07f, 1f, 0.95f, 0.75f, 1.15f);
                    // Soft outer glow.
                    AddDisc(ref addR, ref addG, ref addB, x, y, hx, hy, scale * 0.22f, 0.35f, 0.45f, 0.55f, 0.55f);
                    // Anamorphic horizontal streak.
                    var dy = (y - hy) / (scale * 0.018f + 1e-3f);
                    var dx = (x - hx) / (scale * 0.45f + 1e-3f);
                    var streak = MathF.Exp(-(dx * dx + dy * dy)) * 0.55f;
                    addR += streak * 0.9f;
                    addG += streak * 0.75f;
                    addB += streak * 1.05f;

                    // Ghosts along hot→opposite.
                    AddGhost(ref addR, ref addG, ref addB, x, y, hx, hy, ox, oy, 0.25f, scale * 0.04f, 0.55f, 0.35f, 0.9f);
                    AddGhost(ref addR, ref addG, ref addB, x, y, hx, hy, ox, oy, 0.5f, scale * 0.055f, 0.9f, 0.45f, 0.35f);
                    AddGhost(ref addR, ref addG, ref addB, x, y, hx, hy, ox, oy, 0.72f, scale * 0.03f, 0.4f, 0.85f, 0.55f);
                    AddGhost(ref addR, ref addG, ref addB, x, y, hx, hy, ox, oy, 0.9f, scale * 0.08f, 0.25f, 0.4f, 0.7f);

                    if (addR <= 0f && addG <= 0f && addB <= 0f)
                        continue;

                    addR *= amount;
                    addG *= amount;
                    addB *= amount;
                    p = new Color(
                        ClampByte(p.Red + addR * 255f),
                        ClampByte(p.Green + addG * 255f),
                        ClampByte(p.Blue + addB * 255f),
                        p.Alpha
                    );
                }
            }
        );

        return image;
    }

    private static void AddGhost(
        ref float addR,
        ref float addG,
        ref float addB,
        int x,
        int y,
        float hx,
        float hy,
        float ox,
        float oy,
        float t,
        float radius,
        float r,
        float g,
        float b
    )
    {
        var gx = hx + (ox - hx) * t;
        var gy = hy + (oy - hy) * t;
        AddDisc(ref addR, ref addG, ref addB, x, y, gx, gy, radius, r, g, b, 0.65f);
    }

    private static void AddDisc(
        ref float addR,
        ref float addG,
        ref float addB,
        int x,
        int y,
        float cx,
        float cy,
        float radius,
        float r,
        float g,
        float b,
        float gain
    )
    {
        var dx = (x - cx) / (radius + 1e-3f);
        var dy = (y - cy) / (radius + 1e-3f);
        var d2 = dx * dx + dy * dy;
        if (d2 > 9f)
            return;
        var falloff = MathF.Exp(-d2) * gain;
        addR += falloff * r;
        addG += falloff * g;
        addB += falloff * b;
    }

    private static byte ClampByte(float v) =>
        (byte)Math.Clamp((int)(v + 0.5f), 0, 255);
}
