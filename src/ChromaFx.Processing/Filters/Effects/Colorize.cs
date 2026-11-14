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
using ChromaFx.Processing.Numerics;

namespace ChromaFx.Processing.Filters.Effects;

/// <summary>
/// Tints the image toward a color while keeping luminance (HSV value). Alpha is preserved.
/// </summary>
/// <param name="tint">Tint color (hue/saturation taken from this).</param>
/// <param name="amount">Blend of original vs colorized (0–1). Default 1.</param>
public sealed class Colorize(Color tint, float amount = 1f) : IFilter
{
    /// <summary>Gets the tint color.</summary>
    public Color Tint { get; } = tint;

    /// <summary>Gets the blend amount (0–1).</summary>
    public float Amount { get; } = amount;

    /// <inheritdoc />
    public Image Apply(Image image, Rectangle targetLocation = default)
    {
        targetLocation = targetLocation.Normalize(image);
        var amount = Math.Clamp(Amount, 0f, 1f);
        if (amount <= 0f)
            return image;

        Hsv tintHsv = Tint;
        var hue = tintHsv.Hue;
        var sat = tintHsv.Saturation;

        FilterParallelism.ForRows(
            targetLocation,
            y =>
            {
                var row = image.GetRowSpan(y);
                for (var x = targetLocation.Left; x < targetLocation.Right; x++)
                {
                    var p = row[x];
                    Hsv hsv = p;
                    Color tinted = new Hsv(hue, sat, hsv.Value);
                    tinted = new Color(tinted.Red, tinted.Green, tinted.Blue, p.Alpha);
                    if (amount >= 1f)
                    {
                        row[x] = tinted;
                        continue;
                    }

                    row[x] = new Color(
                        Lerp(p.Red, tinted.Red, amount),
                        Lerp(p.Green, tinted.Green, amount),
                        Lerp(p.Blue, tinted.Blue, amount),
                        p.Alpha
                    );
                }
            }
        );

        return image;
    }

    private static byte Lerp(byte a, byte b, float t) =>
        (byte)Math.Clamp((int)(a + (b - a) * t + 0.5f), 0, 255);
}
