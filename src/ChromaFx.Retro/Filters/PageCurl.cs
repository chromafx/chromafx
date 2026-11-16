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
/// Classic page-curl from a corner: rolled wedge, underside tint, and soft shadow.
/// Stylized nostalgia — not a 3D paper sim.
/// </summary>
/// <param name="amount">
/// How far the curl advances (0–1). Default 0.55.
/// </param>
/// <param name="fromRight">
/// When true, curl from bottom-right (default); otherwise bottom-left.
/// </param>
/// <param name="underside">
/// Color peeking under the curl. Default warm paper.
/// </param>
public sealed class PageCurl(
    float amount = 0.55f,
    bool fromRight = true,
    Color underside = default
) : IFilter
{
    public float Amount { get; } = amount;
    public bool FromRight { get; } = fromRight;
    public Color Underside { get; } = underside;

    /// <inheritdoc />
    public Image Apply(Image image, Rectangle targetLocation = default)
    {
        if (image.Width <= 0 || image.Height <= 0)
            return image;

        var curl = Math.Clamp(Amount, 0f, 1f);
        if (curl <= 0f)
            return image;

        targetLocation = targetLocation.Normalize(image);
        var source = image.Clone();
        var w = image.Width;
        var h = image.Height;
        var paper =
            Underside.Alpha == 0 && Underside.Red == 0 && Underside.Green == 0 && Underside.Blue == 0
                ? new Color(232, 220, 198)
                : Underside;

        // Curl frontier: diagonal band from chosen bottom corner.
        var progress = curl * (w + h) * 0.55f;
        var band = Math.Max(8f, MathF.Min(w, h) * 0.12f);

        Parallel.For(
            targetLocation.Bottom,
            targetLocation.Top,
            y =>
            {
                var row = image.GetRowSpan(y);
                for (var x = targetLocation.Left; x < targetLocation.Right; x++)
                {
                    var src = source.GetRowSpan(y)[x];
                    // Distance along the curl diagonal from the corner.
                    float along = FromRight ? (w - 1 - x) + (h - 1 - y) : x + (h - 1 - y);

                    if (along > progress + band)
                    {
                        row[x] = src;
                        continue;
                    }

                    if (along < progress - band * 0.35f)
                    {
                        // Revealed underside + soft shadow near the fold.
                        var shadow = Math.Clamp((progress - along) / (band * 2f), 0f, 1f);
                        row[x] = new Color(
                            (byte)(paper.Red * (1f - shadow * 0.35f)),
                            (byte)(paper.Green * (1f - shadow * 0.35f)),
                            (byte)(paper.Blue * (1f - shadow * 0.35f)),
                            src.Alpha
                        );
                        continue;
                    }

                    // Curl cylinder: map into a rolled wedge (sample mirrored toward corner).
                    var local = (along - (progress - band)) / (band * 1.35f);
                    local = Math.Clamp(local, 0f, 1f);
                    var roll = local * MathF.PI; // 0…π fold
                    var lift = MathF.Sin(roll);
                    var back = MathF.Cos(roll);

                    // Sample a bit toward the corner for the curling face.
                    float sx = x,
                        sy = y;
                    var pull = (1f - back) * band * 0.65f;
                    if (FromRight)
                    {
                        sx = x + pull;
                        sy = y + pull * 0.35f;
                    }
                    else
                    {
                        sx = x - pull;
                        sy = y + pull * 0.35f;
                    }

                    var curled = Sample(source, sx, sy);
                    // Highlight on the tube + shade the underside of the roll.
                    var highlight = MathF.Pow(MathF.Max(0f, lift), 1.5f);
                    var shade = Math.Clamp(1f - lift * 0.45f, 0.55f, 1f);
                    var r = curled.Red * shade + highlight * 40f;
                    var g = curled.Green * shade + highlight * 36f;
                    var b = curled.Blue * shade + highlight * 30f;

                    // Mix a little underside near the peel.
                    if (local < 0.35f)
                    {
                        var u = 1f - local / 0.35f;
                        r = r * (1f - u * 0.5f) + paper.Red * u * 0.5f;
                        g = g * (1f - u * 0.5f) + paper.Green * u * 0.5f;
                        b = b * (1f - u * 0.5f) + paper.Blue * u * 0.5f;
                    }

                    row[x] = new Color(Clamp(r), Clamp(g), Clamp(b), src.Alpha);
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

    private static byte Clamp(float v) => (byte)Math.Clamp((int)(v + 0.5f), 0, 255);
}
