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
/// Plastic-wrap / jelly sheen look-alike: edge highlights and a glossy lift on bright contours.
/// Stylized nostalgia — not a refraction sim.
/// </summary>
/// <param name="strength">
/// Sheen intensity (0–1). Default 0.75.
/// </param>
public sealed class PlasticWrap(float strength = 0.75f) : IFilter
{
    /// <summary>Gets the sheen strength (0–1).</summary>
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
        var source = image.Clone();
        var w = image.Width;
        var h = image.Height;

        Parallel.For(
            targetLocation.Bottom,
            targetLocation.Top,
            y =>
            {
                var row = image.GetRowSpan(y);
                for (var x = targetLocation.Left; x < targetLocation.Right; x++)
                {
                    var src = source.GetRowSpan(y)[x];
                    var luma = Luma(src);

                    // Sobel-ish magnitude on luma (clamped neighbors).
                    var tl = LumaAt(source, x - 1, y - 1, w, h);
                    var tc = LumaAt(source, x, y - 1, w, h);
                    var tr = LumaAt(source, x + 1, y - 1, w, h);
                    var ml = LumaAt(source, x - 1, y, w, h);
                    var mr = LumaAt(source, x + 1, y, w, h);
                    var bl = LumaAt(source, x - 1, y + 1, w, h);
                    var bc = LumaAt(source, x, y + 1, w, h);
                    var br = LumaAt(source, x + 1, y + 1, w, h);

                    var gx = -tl - 2f * ml - bl + tr + 2f * mr + br;
                    var gy = -tl - 2f * tc - tr + bl + 2f * bc + br;
                    var edge = MathF.Sqrt(gx * gx + gy * gy);
                    edge = Math.Clamp(edge * 1.8f, 0f, 1f);

                    // Gloss: white/cyan lift on edges; slight darken in flats for wrapped depth.
                    var sheen = edge * amount;
                    var wrapDark = (1f - edge) * amount * 0.12f * (1f - luma);

                    var r = src.Red * (1f - wrapDark) + sheen * 255f;
                    var g = src.Green * (1f - wrapDark) + sheen * 250f;
                    var b = src.Blue * (1f - wrapDark) + sheen * 255f;
                    // Extra jelly highlight on already-bright edges.
                    if (luma > 0.55f && edge > 0.2f)
                    {
                        var jelly = (luma - 0.55f) * edge * amount * 0.5f;
                        r += jelly * 40f;
                        g += jelly * 55f;
                        b += jelly * 60f;
                    }

                    row[x] = new Color(ClampByte(r), ClampByte(g), ClampByte(b), src.Alpha);
                }
            }
        );

        return image;
    }

    private static float Luma(Color c) =>
        (0.299f * c.Red + 0.587f * c.Green + 0.114f * c.Blue) / 255f;

    private static float LumaAt(Image image, int x, int y, int w, int h)
    {
        x = Math.Clamp(x, 0, w - 1);
        y = Math.Clamp(y, 0, h - 1);
        return Luma(image.GetRowSpan(y)[x]);
    }

    private static byte ClampByte(float v) =>
        (byte)Math.Clamp((int)(v + 0.5f), 0, 255);
}
