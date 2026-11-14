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

using System.Buffers;
using ChromaFx.Core;
using ChromaFx.Core.Colors;
using ChromaFx.Processing.Filters;
using ChromaFx.Processing.Numerics;

namespace ChromaFx.Retro.Filters;

/// <summary>
/// Early-digicam flash glare: harsh clipped whites plus a faint horizontal/vertical cross streak
/// from bright sources. Stylized nostalgia — not a CCD sensor model.
/// </summary>
/// <param name="threshold">
/// Luma (0–1) above which pixels contribute bloom. Default 0.85.
/// </param>
/// <param name="radius">
/// Half-length of the cross streak in pixels (clamped to 1…64). Default 12.
/// </param>
/// <param name="strength">
/// Bloom / clip intensity (0–1). Default 0.65.
/// </param>
public sealed class CcdBloom(float threshold = 0.85f, int radius = 12, float strength = 0.65f)
    : IFilter
{
    /// <summary>
    /// Gets the luma threshold for bloom sources.
    /// </summary>
    public float Threshold { get; } = threshold;

    /// <summary>
    /// Gets the cross streak half-length in pixels.
    /// </summary>
    public int Radius { get; } = radius;

    /// <summary>
    /// Gets the bloom / clip strength (0–1).
    /// </summary>
    public float Strength { get; } = strength;

    /// <inheritdoc />
    public Image Apply(Image image, Rectangle targetLocation = default)
    {
        if (image.Width <= 0 || image.Height <= 0)
            return image;

        var amount = Math.Clamp(Strength, 0f, 1f);
        if (amount <= 0f)
            return image;

        var rad = Math.Clamp(Radius, 1, 64);
        var thresh = Math.Clamp(Threshold, 0f, 1f);
        targetLocation = targetLocation.Normalize(image);

        var w = image.Width;
        var h = image.Height;
        var pixelCount = w * h;
        var bloom = ArrayPool<float>.Shared.Rent(pixelCount);
        Array.Clear(bloom, 0, pixelCount);

        try
        {
            // Pass 1: collect excess luma above threshold as streak energy.
            for (var y = 0; y < h; y++)
            {
                var row = image.GetRowSpan(y);
                for (var x = 0; x < w; x++)
                {
                    var p = row[x];
                    var luma = (0.299f * p.Red + 0.587f * p.Green + 0.114f * p.Blue) / 255f;
                    var excess = luma - thresh;
                    if (excess <= 0f)
                        continue;

                    var energy = excess / Math.Max(1e-6f, 1f - thresh);
                    SpreadCross(bloom, w, h, x, y, rad, energy);
                }
            }

            // Pass 2: harsh clip on bright pixels + add streak (partial target supported).
            Parallel.For(
                targetLocation.Bottom,
                targetLocation.Top,
                y =>
                {
                    var row = image.GetRowSpan(y);
                    for (var x = targetLocation.Left; x < targetLocation.Right; x++)
                    {
                        ref var p = ref row[x];
                        var luma = (0.299f * p.Red + 0.587f * p.Green + 0.114f * p.Blue) / 255f;
                        var r = (float)p.Red;
                        var g = (float)p.Green;
                        var b = (float)p.Blue;

                        // Push near-whites toward hard clip.
                        if (luma > thresh)
                        {
                            var clipT = amount * ((luma - thresh) / Math.Max(1e-6f, 1f - thresh));
                            r = r + (255f - r) * clipT;
                            g = g + (255f - g) * clipT;
                            b = b + (255f - b) * clipT;
                        }

                        var streak = bloom[y * w + x] * amount * 255f;
                        if (streak > 0f)
                        {
                            r = Math.Min(255f, r + streak);
                            g = Math.Min(255f, g + streak * 0.92f);
                            b = Math.Min(255f, b + streak * 0.85f);
                        }

                        p = new Color((byte)(r + 0.5f), (byte)(g + 0.5f), (byte)(b + 0.5f), p.Alpha);
                    }
                }
            );
        }
        finally
        {
            ArrayPool<float>.Shared.Return(bloom);
        }

        return image;
    }

    private static void SpreadCross(
        float[] bloom,
        int w,
        int h,
        int cx,
        int cy,
        int radius,
        float energy
    )
    {
        bloom[cy * w + cx] += energy;

        for (var i = 1; i <= radius; i++)
        {
            var falloff = energy * (1f - i / (float)(radius + 1));
            var xL = cx - i;
            var xR = cx + i;
            var yU = cy - i;
            var yD = cy + i;

            if (xL >= 0)
                bloom[cy * w + xL] += falloff;
            if (xR < w)
                bloom[cy * w + xR] += falloff;
            if (yU >= 0)
                bloom[yU * w + cx] += falloff;
            if (yD < h)
                bloom[yD * w + cx] += falloff;
        }
    }
}
