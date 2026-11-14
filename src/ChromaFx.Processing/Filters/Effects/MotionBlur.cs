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
/// Directional motion blur (box average along a line).
/// </summary>
/// <param name="length">Kernel length in pixels (≥ 1). Default 9.</param>
/// <param name="angleDegrees">Blur direction in degrees (0 = right, 90 = down).</param>
public sealed class MotionBlur(int length = 9, float angleDegrees = 0f) : IFilter
{
    /// <summary>Gets the kernel length.</summary>
    public int Length { get; } = length;

    /// <summary>Gets the angle in degrees.</summary>
    public float AngleDegrees { get; } = angleDegrees;

    /// <inheritdoc />
    public Image Apply(Image image, Rectangle targetLocation = default)
    {
        var len = Math.Max(1, Length);
        if (len == 1)
            return image;

        targetLocation = targetLocation.Normalize(image);
        var rad = AngleDegrees * (MathF.PI / 180f);
        var dx = MathF.Cos(rad);
        var dy = MathF.Sin(rad);
        var half = len / 2;
        // Precompute i·(dx,dy); still Round(x + δ) per tap so midpoint rounding matches the old path.
        var deltas = new (float Dx, float Dy)[len];
        for (var i = -half; i <= half; i++)
            deltas[i + half] = (i * dx, i * dy);

        var source = FilterBufferPool.RentTightCopy(image);
        var width = image.Width;
        var height = image.Height;

        try
        {
            FilterParallelism.ForRows(
                targetLocation,
                y =>
                {
                    var row = image.GetRowSpan(y);
                    for (var x = targetLocation.Left; x < targetLocation.Right; x++)
                    {
                        int r = 0,
                            g = 0,
                            b = 0,
                            a = 0,
                            n = 0;
                        foreach (var (ddx, ddy) in deltas)
                        {
                            var sx = (int)MathF.Round(x + ddx);
                            var sy = (int)MathF.Round(y + ddy);
                            if ((uint)sx >= (uint)width || (uint)sy >= (uint)height)
                                continue;
                            var p = source[sy * width + sx];
                            r += p.Red;
                            g += p.Green;
                            b += p.Blue;
                            a += p.Alpha;
                            n++;
                        }

                        if (n == 0)
                            continue;
                        row[x] = new Color(
                            (byte)(r / n),
                            (byte)(g / n),
                            (byte)(b / n),
                            (byte)(a / n)
                        );
                    }
                }
            );
        }
        finally
        {
            FilterBufferPool.Return(source);
        }

        return image;
    }
}
