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
/// When <see cref="ImageInfluence"/> is &gt; 0, bottom sparks and upward heat are
/// conditioned by source luma/alpha so flame shape follows the photo.
/// </summary>
/// <param name="strength">Overlay mix (0–1). Default 0.85.</param>
/// <param name="seed">Noise seed for the fire base. Default 7.</param>
/// <param name="cooling">Cooling rate (0.5–2). Default 1.</param>
/// <param name="imageInfluence">
/// How strongly source luma seeds and gates heat (0 = classic RNG-only, 1 = fully
/// content-conditioned). Default 0 (bit-compatible with pre-image-aware Fire).
/// </param>
public sealed class Fire(
    float strength = 0.85f,
    int seed = 7,
    float cooling = 1f,
    float imageInfluence = 0f
) : IFilter
{
    public float Strength { get; } = strength;
    public int Seed { get; } = seed;
    public float Cooling { get; } = cooling;

    /// <summary>
    /// Gets how strongly source luma seeds and gates heat (0–1).
    /// </summary>
    public float ImageInfluence { get; } = imageInfluence;

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
        var influence = Math.Clamp(ImageInfluence, 0f, 1f);
        var heat = new float[w * h];
        var rng = new System.Random(Seed);

        // Per-column bottom-band fuel (relative structure); no per-cell multiply —
        // that extinguished tall images. Influence modulates spark strength + cooling.
        var columnFuel = new float[w];
        if (influence > 0f)
        {
            var band = Math.Clamp(Math.Max(4, h / 8), 4, Math.Min(24, h));
            var fuelSum = new float[w];
            var fuelCount = new int[w];
            var fuelMin = float.MaxValue;
            var fuelMax = 0f;

            for (var y = h - band; y < h; y++)
            {
                var row = image.GetRowSpan(y);
                for (var x = 0; x < w; x++)
                {
                    var p = row[x];
                    var luma = (0.299f * p.Red + 0.587f * p.Green + 0.114f * p.Blue) / 255f;
                    var energy = luma * (1f - 0.35f * luma) * (p.Alpha / 255f);
                    fuelSum[x] += energy;
                    fuelCount[x]++;
                }
            }

            for (var x = 0; x < w; x++)
            {
                var f = fuelCount[x] > 0 ? fuelSum[x] / fuelCount[x] : 0f;
                columnFuel[x] = f;
                if (f < fuelMin)
                    fuelMin = f;
                if (f > fuelMax)
                    fuelMax = f;
            }

            // Remap to 0.35…1 so dark bottoms still burn; bright columns stay hotter.
            var span = Math.Max(1e-4f, fuelMax - fuelMin);
            for (var x = 0; x < w; x++)
                columnFuel[x] = 0.35f + 0.65f * ((columnFuel[x] - fuelMin) / span);
        }

        // Bottom row sparks (RNG, optionally mixed with column fuel).
        for (var x = 0; x < w; x++)
        {
            var noise = (float)rng.NextDouble();
            heat[(h - 1) * w + x] = Spark(noise, columnFuel[x], influence);
        }

        // Propagate upward with cooling + neighbor average (a few iterations for one frame).
        // Scale cooling to frame height so tall photos still get demoscene-height flames
        // (the classic 0.012 step was tuned for ~200px buffers).
        var coolStep = 0.012f * cool * Math.Clamp(220f / h, 0.2f, 1f);
        var iterations = Math.Max(8, h / 3);
        for (var iter = 0; iter < iterations; iter++)
        {
            // Re-spark bottom occasionally.
            if (iter % 3 == 0)
            {
                for (var x = 0; x < w; x++)
                {
                    var noise = (float)rng.NextDouble() * 0.9f;
                    var sparked = Spark(noise, columnFuel[x], influence);
                    heat[(h - 1) * w + x] = Math.Max(heat[(h - 1) * w + x], sparked);
                }
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
                    // Milder cooling in fueled columns; empty columns die a bit sooner.
                    var step = coolStep;
                    if (influence > 0f)
                        step *= 1f + influence * (1f - columnFuel[x]) * 0.35f;
                    var v = (left + mid + right + below) * 0.25f - step;
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

    /// <summary>
    /// Mix classic RNG spark with remapped column fuel. At influence 1, noise keeps jitter
    /// and fuel stays in a burnable range (see remap above).
    /// </summary>
    private static float Spark(float noise, float fuel, float influence)
    {
        if (influence <= 0f)
            return noise;

        // fuel is already ~0.35…1; scale noise so sparks stay hot enough to rise.
        var content = fuel * (0.55f + 0.45f * noise);
        return (1f - influence) * noise + influence * content;
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
