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
/// Multi-sine demoscene plasma (palette look-alike). Hokey — not physics.
/// </summary>
/// <param name="strength">Blend over the source (0–1). Default 1 (replace).</param>
/// <param name="scale">Spatial frequency. Default 0.08.</param>
/// <param name="time">Phase offset. Default 0.</param>
public sealed class Plasma(float strength = 1f, float scale = 0.08f, float time = 0f) : IFilter
{
    public float Strength { get; } = strength;
    public float Scale { get; } = scale;
    public float Time { get; } = time;

    /// <inheritdoc />
    public Image Apply(Image image, Rectangle targetLocation = default)
    {
        if (image.Width <= 0 || image.Height <= 0)
            return image;

        var amount = Math.Clamp(Strength, 0f, 1f);
        if (amount <= 0f)
            return image;

        targetLocation = targetLocation.Normalize(image);
        var sc = Math.Clamp(Scale, 0.005f, 1f);
        var t = Time;

        Parallel.For(
            targetLocation.Bottom,
            targetLocation.Top,
            y =>
            {
                var row = image.GetRowSpan(y);
                for (var x = targetLocation.Left; x < targetLocation.Right; x++)
                {
                    var v =
                        MathF.Sin(x * sc + t)
                        + MathF.Sin(y * sc * 1.3f + t * 1.1f)
                        + MathF.Sin((x + y) * sc * 0.7f + t * 0.8f)
                        + MathF.Sin(MathF.Sqrt(x * x + y * y) * sc * 0.9f + t);
                    v = v * 0.25f + 0.5f; // ~0…1
                    var plasma = Palette(v);
                    ref var p = ref row[x];
                    if (amount >= 1f)
                        p = new Color(plasma.Red, plasma.Green, plasma.Blue, p.Alpha);
                    else
                        p = new Color(
                            (byte)(p.Red + (plasma.Red - p.Red) * amount + 0.5f),
                            (byte)(p.Green + (plasma.Green - p.Green) * amount + 0.5f),
                            (byte)(p.Blue + (plasma.Blue - p.Blue) * amount + 0.5f),
                            p.Alpha
                        );
                }
            }
        );

        return image;
    }

    private static Color Palette(float t)
    {
        t = Math.Clamp(t, 0f, 1f);
        // Classic rainbow-ish plasma ramp.
        var r = (MathF.Sin(t * MathF.PI * 2f) * 0.5f + 0.5f) * 255f;
        var g = (MathF.Sin(t * MathF.PI * 2f + 2.1f) * 0.5f + 0.5f) * 255f;
        var b = (MathF.Sin(t * MathF.PI * 2f + 4.2f) * 0.5f + 0.5f) * 255f;
        return new Color((byte)(r + 0.5f), (byte)(g + 0.5f), (byte)(b + 0.5f));
    }
}
