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
/// Liquid-metal chrome look-alike: luma drives a cool metal ramp plus a fake specular.
/// Stylized nostalgia — not a BRDF.
/// </summary>
/// <param name="strength">
/// Blend toward the chrome result (0–1). Default 1.
/// </param>
public sealed class LiquidChrome(float strength = 1f) : IFilter
{
    /// <summary>Gets the blend strength (0–1).</summary>
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
        var cx = (w - 1) * 0.5f;
        var cy = (h - 1) * 0.5f;
        var inv = 1f / MathF.Sqrt(cx * cx + cy * cy + 1e-6f);

        Parallel.For(
            targetLocation.Bottom,
            targetLocation.Top,
            y =>
            {
                var row = image.GetRowSpan(y);
                for (var x = targetLocation.Left; x < targetLocation.Right; x++)
                {
                    var src = source.GetRowSpan(y)[x];
                    var luma = (0.299f * src.Red + 0.587f * src.Green + 0.114f * src.Blue) / 255f;

                    // Soft metal ramp: deep steel → silver → hot highlight.
                    var chrome = ChromeRamp(luma);

                    // Fake environment specular from spherical coords + luma bump.
                    var nx = (x - cx) * inv;
                    var ny = (y - cy) * inv;
                    var nz = MathF.Sqrt(Math.Max(0f, 1f - nx * nx - ny * ny));
                    // Light from upper-left.
                    var ndotl = Math.Clamp(nx * -0.45f + ny * -0.55f + nz * 0.7f + luma * 0.25f, 0f, 1f);
                    var spec = MathF.Pow(ndotl, 18f) * 0.85f + MathF.Pow(ndotl, 4f) * 0.2f;

                    var r = chrome.Red + spec * 255f;
                    var g = chrome.Green + spec * 250f;
                    var b = chrome.Blue + spec * 245f;
                    var metal = new Color(ClampByte(r), ClampByte(g), ClampByte(b), src.Alpha);

                    row[x] = amount >= 1f ? metal : Lerp(src, metal, amount);
                }
            }
        );

        return image;
    }

    private static Color ChromeRamp(float t)
    {
        t = Math.Clamp(t, 0f, 1f);
        // Piecewise cool metal.
        float r, g, b;
        if (t < 0.35f)
        {
            var u = t / 0.35f;
            r = Lerp(18f, 70f, u);
            g = Lerp(22f, 78f, u);
            b = Lerp(32f, 95f, u);
        }
        else if (t < 0.7f)
        {
            var u = (t - 0.35f) / 0.35f;
            r = Lerp(70f, 175f, u);
            g = Lerp(78f, 185f, u);
            b = Lerp(95f, 200f, u);
        }
        else
        {
            var u = (t - 0.7f) / 0.3f;
            r = Lerp(175f, 245f, u);
            g = Lerp(185f, 248f, u);
            b = Lerp(200f, 255f, u);
        }

        return new Color(ClampByte(r), ClampByte(g), ClampByte(b));
    }

    private static float Lerp(float a, float b, float t) => a + (b - a) * t;

    private static Color Lerp(Color a, Color b, float t) =>
        new(
            (byte)(a.Red + (b.Red - a.Red) * t + 0.5f),
            (byte)(a.Green + (b.Green - a.Green) * t + 0.5f),
            (byte)(a.Blue + (b.Blue - a.Blue) * t + 0.5f),
            a.Alpha
        );

    private static byte ClampByte(float v) =>
        (byte)Math.Clamp((int)(v + 0.5f), 0, 255);
}
