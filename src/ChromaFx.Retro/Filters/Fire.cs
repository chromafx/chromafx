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
/// Classic cooling-buffer fire (demoscene hokey — not CFD). Blended over the image.
/// </summary>
/// <param name="strength">Overlay mix (0–1). Default 0.85.</param>
/// <param name="seed">Noise seed for the fire base. Default 7.</param>
/// <param name="cooling">Cooling rate (0.5–2). Default 1.</param>
public sealed class Fire(float strength = 0.85f, int seed = 7, float cooling = 1f) : IFilter
{
    public float Strength { get; } = strength;
    public int Seed { get; } = seed;
    public float Cooling { get; } = cooling;

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
        var cool = Math.Clamp(Cooling, 0.5f, 2f);
        var heat = new float[w * h];
        var rng = new System.Random(Seed);

        // Bottom row sparks.
        for (var x = 0; x < w; x++)
            heat[(h - 1) * w + x] = (float)rng.NextDouble();

        // Propagate upward with cooling + neighbor average (a few iterations for one frame).
        var iterations = Math.Max(8, h / 4);
        for (var iter = 0; iter < iterations; iter++)
        {
            // Re-spark bottom occasionally.
            if (iter % 3 == 0)
            {
                for (var x = 0; x < w; x++)
                    heat[(h - 1) * w + x] = Math.Max(
                        heat[(h - 1) * w + x],
                        (float)rng.NextDouble() * 0.9f
                    );
            }

            for (var y = 1; y < h; y++)
            {
                var srcY = y;
                var dstY = y - 1;
                for (var x = 0; x < w; x++)
                {
                    var left = heat[srcY * w + Math.Max(0, x - 1)];
                    var mid = heat[srcY * w + x];
                    var right = heat[srcY * w + Math.Min(w - 1, x + 1)];
                    var below = y + 1 < h ? heat[(y + 1) * w + x] : mid;
                    var v = (left + mid + right + below) * 0.25f - 0.012f * cool;
                    heat[dstY * w + x] = Math.Clamp(v, 0f, 1f);
                }
            }
        }

        Parallel.For(
            targetLocation.Bottom,
            targetLocation.Top,
            y =>
            {
                var row = image.GetRowSpan(y);
                for (var x = targetLocation.Left; x < targetLocation.Right; x++)
                {
                    var t = heat[y * w + x];
                    if (t <= 0.001f && amount < 1f)
                        continue;
                    var fire = FirePalette(t);
                    ref var p = ref row[x];
                    // Screen-ish blend so flames sit on top.
                    var r = Screen(p.Red, fire.Red, amount);
                    var g = Screen(p.Green, fire.Green, amount);
                    var b = Screen(p.Blue, fire.Blue, amount);
                    p = new Color(r, g, b, p.Alpha);
                }
            }
        );

        return image;
    }

    private static Color FirePalette(float t)
    {
        t = Math.Clamp(t, 0f, 1f);
        float r, g, b;
        if (t < 0.3f)
        {
            var u = t / 0.3f;
            r = u * 180f;
            g = 0f;
            b = 0f;
        }
        else if (t < 0.6f)
        {
            var u = (t - 0.3f) / 0.3f;
            r = 180f + u * 75f;
            g = u * 120f;
            b = 0f;
        }
        else
        {
            var u = (t - 0.6f) / 0.4f;
            r = 255f;
            g = 120f + u * 135f;
            b = u * 200f;
        }

        return new Color((byte)(r + 0.5f), (byte)(g + 0.5f), (byte)(b + 0.5f));
    }

    private static byte Screen(byte baseC, byte blend, float amount)
    {
        var screen = 255 - (255 - baseC) * (255 - blend) / 255;
        return (byte)(baseC + (screen - baseC) * amount + 0.5f);
    }
}
