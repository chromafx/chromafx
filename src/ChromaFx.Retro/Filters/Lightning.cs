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
/// Branching lightning bolt overlay (seeded mid-point displacement). Hokey demoscene vibe.
/// </summary>
/// <param name="seed">RNG seed for bolt shape. Default 42.</param>
/// <param name="strength">Glow intensity (0–1). Default 0.9.</param>
/// <param name="bolts">Number of main bolts (1–6). Default 2.</param>
public sealed class Lightning(int seed = 42, float strength = 0.9f, int bolts = 2) : IFilter
{
    public int Seed { get; } = seed;
    public float Strength { get; } = strength;
    public int Bolts { get; } = bolts;

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
        var glow = new float[w * h];
        var count = Math.Clamp(Bolts, 1, 6);
        var rng = new System.Random(Seed);

        for (var b = 0; b < count; b++)
        {
            var x0 = rng.Next(w / 8, w * 7 / 8);
            var y0 = 0;
            var x1 = rng.Next(w / 8, w * 7 / 8);
            var y1 = h - 1;
            DrawBolt(glow, w, h, x0, y0, x1, y1, MathF.Min(w, h) * 0.12f, rng, depth: 0);
        }

        // Soften glow slightly (box blur 1px).
        var soft = new float[w * h];
        for (var y = 1; y < h - 1; y++)
        for (var x = 1; x < w - 1; x++)
        {
            var i = y * w + x;
            soft[i] =
                (
                    glow[i]
                    + glow[i - 1]
                    + glow[i + 1]
                    + glow[i - w]
                    + glow[i + w]
                ) * 0.2f;
        }

        Parallel.For(
            targetLocation.Bottom,
            targetLocation.Top,
            y =>
            {
                var row = image.GetRowSpan(y);
                for (var x = targetLocation.Left; x < targetLocation.Right; x++)
                {
                    var g = soft[y * w + x] * amount;
                    if (g <= 0.001f)
                        continue;
                    ref var p = ref row[x];
                    p = new Color(
                        Clamp(p.Red + g * 180f),
                        Clamp(p.Green + g * 210f),
                        Clamp(p.Blue + g * 255f),
                        p.Alpha
                    );
                }
            }
        );

        return image;
    }

    private static void DrawBolt(
        float[] glow,
        int w,
        int h,
        float x0,
        float y0,
        float x1,
        float y1,
        float displace,
        System.Random rng,
        int depth
    )
    {
        if (displace < 1.5f || depth > 6)
        {
            StampLine(glow, w, h, x0, y0, x1, y1);
            return;
        }

        var mx = (x0 + x1) * 0.5f + (float)(rng.NextDouble() * 2 - 1) * displace;
        var my = (y0 + y1) * 0.5f + (float)(rng.NextDouble() * 2 - 1) * displace * 0.35f;
        DrawBolt(glow, w, h, x0, y0, mx, my, displace * 0.55f, rng, depth + 1);
        DrawBolt(glow, w, h, mx, my, x1, y1, displace * 0.55f, rng, depth + 1);

        // Occasional branch.
        if (depth < 4 && rng.NextDouble() < 0.35)
        {
            var bx = mx + (float)(rng.NextDouble() * 2 - 1) * displace;
            var by = my + displace * (0.4f + (float)rng.NextDouble() * 0.6f);
            DrawBolt(glow, w, h, mx, my, bx, by, displace * 0.4f, rng, depth + 2);
        }
    }

    private static void StampLine(float[] glow, int w, int h, float x0, float y0, float x1, float y1)
    {
        var steps = (int)(MathF.Abs(x1 - x0) + MathF.Abs(y1 - y0)) + 1;
        for (var i = 0; i <= steps; i++)
        {
            var t = i / (float)steps;
            var x = (int)MathF.Round(x0 + (x1 - x0) * t);
            var y = (int)MathF.Round(y0 + (y1 - y0) * t);
            if ((uint)x >= (uint)w || (uint)y >= (uint)h)
                continue;
            glow[y * w + x] = Math.Max(glow[y * w + x], 1f);
            // Soft core
            for (var oy = -1; oy <= 1; oy++)
            for (var ox = -1; ox <= 1; ox++)
            {
                var xx = x + ox;
                var yy = y + oy;
                if ((uint)xx >= (uint)w || (uint)yy >= (uint)h)
                    continue;
                var fall = 1f - (MathF.Abs(ox) + MathF.Abs(oy)) * 0.35f;
                var idx = yy * w + xx;
                glow[idx] = Math.Max(glow[idx], fall * 0.55f);
            }
        }
    }

    private static byte Clamp(float v) => (byte)Math.Clamp((int)(v + 0.5f), 0, 255);
}
