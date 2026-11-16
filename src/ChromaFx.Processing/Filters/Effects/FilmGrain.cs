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
using ChromaFx.Processing.Numerics;
using Random = ChromaFx.Processing.Numerics.Random;

namespace ChromaFx.Processing.Filters.Effects;

/// <summary>
/// Soft film-like grain: multi-octave value noise, stronger in shadows, mostly luminance-correlated
/// with optional dye-cloud chroma. Alpha is preserved.
/// </summary>
public sealed class FilmGrain : IFilter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FilmGrain"/> class.
    /// </summary>
    /// <param name="amount">Overall strength (0–1). Default 0.35.</param>
    /// <param name="size">Grain clump size in pixels (0.25–16). Default 1.25.</param>
    /// <param name="colorAmount">Per-channel dye variation (0–1). Default 0.12.</param>
    /// <param name="seed">Optional seed; when null a seed is chosen per apply.</param>
    public FilmGrain(
        float amount = 0.35f,
        float size = 1.25f,
        float colorAmount = 0.12f,
        int? seed = null
    )
    {
        Amount = Math.Clamp(amount, 0f, 1f);
        Size = Math.Clamp(size, 0.25f, 16f);
        ColorAmount = Math.Clamp(colorAmount, 0f, 1f);
        Seed = seed;
    }

    /// <summary>Gets the overall strength (0–1).</summary>
    public float Amount { get; }

    /// <summary>Gets the grain clump size in pixels.</summary>
    public float Size { get; }

    /// <summary>Gets the chromatic dye-cloud amount (0–1).</summary>
    public float ColorAmount { get; }

    /// <summary>Gets the optional seed.</summary>
    public int? Seed { get; }

    /// <inheritdoc />
    public Image Apply(Image image, Rectangle targetLocation = default)
    {
        if (Amount <= 0f)
            return image;

        targetLocation = targetLocation.Normalize(image);
        var seed = Seed ?? Random.ThreadSafeNext();
        var invSize = 1f / Size;
        var amount = Amount;
        var colorAmount = ColorAmount;
        // Peak additive swing at amount=1 in deep shadows (~±72).
        const float Peak = 72f;

        FilterParallelism.ForRows(
            targetLocation,
            y =>
            {
                var row = image.GetRowSpan(y);
                for (var x = targetLocation.Left; x < targetLocation.Right; x++)
                {
                    ref var pixel = ref row[x];
                    var luma =
                        (pixel.Red * 0.2126f + pixel.Green * 0.7152f + pixel.Blue * 0.0722f)
                        / 255f;
                    // Emulsion response: strongest in shadows, some midtone texture.
                    var shadow = 1f - luma;
                    var response = 0.28f + 0.72f * shadow * shadow;

                    var fx = x * invSize;
                    var fy = y * invSize;
                    var n =
                        ValueNoise(fx, fy, seed) * 0.55f
                        + ValueNoise(fx * 2.13f, fy * 2.13f, seed + 101) * 0.30f
                        + ValueNoise(fx * 4.37f, fy * 4.37f, seed + 203) * 0.15f;

                    var strength = amount * response * Peak;
                    var mono = n * strength;

                    float dr = mono,
                        dg = mono,
                        db = mono;
                    if (colorAmount > 0f)
                    {
                        var chroma = strength * colorAmount;
                        dr += ValueNoise(fx + 17.1f, fy + 3.7f, seed + 307) * chroma;
                        dg += ValueNoise(fx + 5.3f, fy + 19.9f, seed + 409) * chroma;
                        db += ValueNoise(fx + 11.7f, fy + 13.1f, seed + 503) * chroma;
                    }

                    pixel.Red = (pixel.Red + dr).ToByte();
                    pixel.Green = (pixel.Green + dg).ToByte();
                    pixel.Blue = (pixel.Blue + db).ToByte();
                }
            }
        );

        return image;
    }

    private static float ValueNoise(float x, float y, int seed)
    {
        var x0 = (int)MathF.Floor(x);
        var y0 = (int)MathF.Floor(y);
        var fx = x - x0;
        var fy = y - y0;
        var u = fx * fx * (3f - 2f * fx);
        var v = fy * fy * (3f - 2f * fy);

        var n00 = HashSigned(x0, y0, seed);
        var n10 = HashSigned(x0 + 1, y0, seed);
        var n01 = HashSigned(x0, y0 + 1, seed);
        var n11 = HashSigned(x0 + 1, y0 + 1, seed);

        var nx0 = n00 + (n10 - n00) * u;
        var nx1 = n01 + (n11 - n01) * u;
        return nx0 + (nx1 - nx0) * v;
    }

    private static float HashSigned(int x, int y, int seed)
    {
        unchecked
        {
            var h = (uint)(x * 374761393 + y * 668265263 + seed * 982451653);
            h = (h ^ (h >> 13)) * 1274126177u;
            h ^= h >> 16;
            return (h & 0xFFFFFFu) * (2f / 0xFFFFFFu) - 1f;
        }
    }
}
