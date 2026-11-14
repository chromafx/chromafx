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
/// Stylized purple fringing / chromatic aberration: red and blue channels are sampled
/// outward from the image center (green stays put). Not a lens model.
/// </summary>
/// <param name="amount">
/// Maximum channel shift in pixels at the corners (clamped to 0…32). Default 3.
/// </param>
/// <param name="strength">
/// Blend toward the aberrated result (0–1). Default 1.
/// </param>
public sealed class ChromaticAberration(float amount = 3f, float strength = 1f) : IFilter
{
    /// <summary>
    /// Gets the maximum corner shift in pixels.
    /// </summary>
    public float Amount { get; } = amount;

    /// <summary>
    /// Gets the blend strength (0–1).
    /// </summary>
    public float Strength { get; } = strength;

    /// <inheritdoc />
    public Image Apply(Image image, Rectangle targetLocation = default)
    {
        if (image.Width <= 0 || image.Height <= 0)
            return image;

        var shiftMax = Math.Clamp(Amount, 0f, 32f);
        var blend = Math.Clamp(Strength, 0f, 1f);
        if (shiftMax <= 0f || blend <= 0f)
            return image;

        targetLocation = targetLocation.Normalize(image);
        var source = image.Clone();
        var w = image.Width;
        var h = image.Height;
        var cx = (w - 1) * 0.5f;
        var cy = (h - 1) * 0.5f;
        // Normalize so corner distance ≈ 1.
        var invCorner = 1f / MathF.Sqrt(cx * cx + cy * cy + 1e-6f);

        Parallel.For(
            targetLocation.Bottom,
            targetLocation.Top,
            y =>
            {
                var row = image.GetRowSpan(y);
                for (var x = targetLocation.Left; x < targetLocation.Right; x++)
                {
                    var center = source.GetRowSpan(y)[x];
                    var dx = x - cx;
                    var dy = y - cy;
                    var radial = MathF.Sqrt(dx * dx + dy * dy) * invCorner;
                    var shift = shiftMax * radial;
                    if (shift < 0.01f)
                        continue;

                    var ux = dx * invCorner;
                    var uy = dy * invCorner;
                    var red = Sample(source, x - ux * shift, y - uy * shift);
                    var blue = Sample(source, x + ux * shift, y + uy * shift);
                    var aberrated = new Color(red.Red, center.Green, blue.Blue, center.Alpha);
                    row[x] = blend >= 1f ? aberrated : Lerp(center, aberrated, blend);
                }
            }
        );

        return image;
    }

    private static Color Sample(Image image, float fx, float fy)
    {
        var x = Math.Clamp((int)MathF.Round(fx), 0, image.Width - 1);
        var y = Math.Clamp((int)MathF.Round(fy), 0, image.Height - 1);
        return image.GetRowSpan(y)[x];
    }

    private static Color Lerp(Color a, Color b, float t) =>
        new(
            (byte)(a.Red + (b.Red - a.Red) * t + 0.5f),
            (byte)(a.Green + (b.Green - a.Green) * t + 0.5f),
            (byte)(a.Blue + (b.Blue - a.Blue) * t + 0.5f),
            a.Alpha
        );
}
