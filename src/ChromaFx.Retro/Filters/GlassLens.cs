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
/// Circular glass lens: refraction bulge inside the disc plus a bright rim.
/// Stylized nostalgia — not Snell's law.
/// </summary>
/// <param name="refraction">Bulge amount (0–2). Default 0.65.</param>
/// <param name="radius">Lens radius as fraction of half-min-dimension (0–1). Default 0.45.</param>
/// <param name="centerX">Center X fraction (0–1). Default 0.5.</param>
/// <param name="centerY">Center Y fraction (0–1). Default 0.5.</param>
/// <param name="rim">Rim highlight strength (0–1). Default 0.7.</param>
public sealed class GlassLens(
    float refraction = 0.65f,
    float radius = 0.45f,
    float centerX = 0.5f,
    float centerY = 0.5f,
    float rim = 0.7f
) : IFilter
{
    public float Refraction { get; } = refraction;
    public float Radius { get; } = radius;
    public float CenterX { get; } = centerX;
    public float CenterY { get; } = centerY;
    public float Rim { get; } = rim;

    /// <inheritdoc />
    public Image Apply(Image image, Rectangle targetLocation = default)
    {
        if (image.Width <= 0 || image.Height <= 0)
            return image;

        targetLocation = targetLocation.Normalize(image);
        var source = image.Clone();
        var w = image.Width;
        var h = image.Height;
        var cx = Math.Clamp(CenterX, 0f, 1f) * (w - 1);
        var cy = Math.Clamp(CenterY, 0f, 1f) * (h - 1);
        var maxR = Math.Max(1f, MathF.Min(w, h) * 0.5f * Math.Clamp(Radius, 0.05f, 1.2f));
        var bulge = Math.Clamp(Refraction, 0f, 2f);
        var rimAmt = Math.Clamp(Rim, 0f, 1f);

        Parallel.For(
            targetLocation.Bottom,
            targetLocation.Top,
            y =>
            {
                var row = image.GetRowSpan(y);
                for (var x = targetLocation.Left; x < targetLocation.Right; x++)
                {
                    var dx = x - cx;
                    var dy = y - cy;
                    var dist = MathF.Sqrt(dx * dx + dy * dy);
                    if (dist > maxR)
                    {
                        row[x] = source.GetRowSpan(y)[x];
                        continue;
                    }

                    var t = dist / maxR;
                    var z = MathF.Sqrt(Math.Max(0f, 1f - t * t));
                    var k = 1f / (1f + bulge * z);
                    var sx = cx + dx * k;
                    var sy = cy + dy * k;
                    var sample = SampleBilinear(source, sx, sy);

                    // Rim highlight near the edge.
                    var rimGlow = MathF.Pow(Math.Clamp(t, 0f, 1f), 8f) * rimAmt;
                    // Soft vignette inside the glass.
                    var glass = 1f - t * t * 0.12f;
                    var r = sample.Red * glass + rimGlow * 255f;
                    var g = sample.Green * glass + rimGlow * 250f;
                    var b = sample.Blue * glass + rimGlow * 255f;
                    row[x] = new Color(Clamp(r), Clamp(g), Clamp(b), sample.Alpha);
                }
            }
        );

        return image;
    }

    private static Color SampleBilinear(Image image, float fx, float fy)
    {
        var w = image.Width;
        var h = image.Height;
        fx = Math.Clamp(fx, 0f, w - 1);
        fy = Math.Clamp(fy, 0f, h - 1);
        var x0 = (int)MathF.Floor(fx);
        var y0 = (int)MathF.Floor(fy);
        var x1 = Math.Min(x0 + 1, w - 1);
        var y1 = Math.Min(y0 + 1, h - 1);
        var tx = fx - x0;
        var ty = fy - y0;
        var c00 = image.GetRowSpan(y0)[x0];
        var c10 = image.GetRowSpan(y0)[x1];
        var c01 = image.GetRowSpan(y1)[x0];
        var c11 = image.GetRowSpan(y1)[x1];
        return new Color(
            Bilerp(c00.Red, c10.Red, c01.Red, c11.Red, tx, ty),
            Bilerp(c00.Green, c10.Green, c01.Green, c11.Green, tx, ty),
            Bilerp(c00.Blue, c10.Blue, c01.Blue, c11.Blue, tx, ty),
            Bilerp(c00.Alpha, c10.Alpha, c01.Alpha, c11.Alpha, tx, ty)
        );
    }

    private static byte Bilerp(byte a, byte b, byte c, byte d, float tx, float ty)
    {
        var top = a + (b - a) * tx;
        var bot = c + (d - c) * tx;
        return (byte)(top + (bot - top) * ty + 0.5f);
    }

    private static byte Clamp(float v) => (byte)Math.Clamp((int)(v + 0.5f), 0, 255);
}
