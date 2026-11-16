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
using ChromaFx.Processing.Procedural;

namespace ChromaFx.Retro.Filters;

/// <summary>
/// Fractal noise texture overlay (blend over the image). Texture-explorer result, not a UI.
/// </summary>
/// <param name="strength">Overlay mix (0–1). Default 0.55.</param>
/// <param name="frequency">Perlin frequency. Default 0.02.</param>
/// <param name="seed">Perlin seed. Default 1337.</param>
/// <param name="octaves">FBM octaves (1–8). Default 4.</param>
public sealed class FractalNoise(
    float strength = 0.55f,
    float frequency = 0.02f,
    int seed = 1337,
    int octaves = 4
) : IFilter
{
    public float Strength { get; } = strength;
    public float Frequency { get; } = frequency;
    public int Seed { get; } = seed;
    public int Octaves { get; } = octaves;

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
        var oct = Math.Clamp(Octaves, 1, 8);
        var freq = Math.Clamp(Frequency, 0.001f, 1f);

        var noiseImg = PerlinNoise.Generate(
            w,
            h,
            maxRgbValue: 255f,
            minRgbValue: 0f,
            frequency: freq,
            amplitude: 1f,
            persistence: 0.5f,
            octaves: oct,
            seed: Seed
        );

        Parallel.For(
            targetLocation.Bottom,
            targetLocation.Top,
            y =>
            {
                var row = image.GetRowSpan(y);
                var nRow = noiseImg.GetRowSpan(y);
                for (var x = targetLocation.Left; x < targetLocation.Right; x++)
                {
                    var n = nRow[x].Red / 255f;
                    var nr = n * 0.75f + 0.1f;
                    var ng = n * 0.7f + 0.12f;
                    var nb = n * 0.85f + 0.15f;
                    ref var p = ref row[x];
                    p = new Color(
                        (byte)(p.Red + (nr * 255f - p.Red) * amount + 0.5f),
                        (byte)(p.Green + (ng * 255f - p.Green) * amount + 0.5f),
                        (byte)(p.Blue + (nb * 255f - p.Blue) * amount + 0.5f),
                        p.Alpha
                    );
                }
            }
        );

        return image;
    }
}
