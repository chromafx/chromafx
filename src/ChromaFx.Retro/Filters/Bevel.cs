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
/// Soft bevel border: lightens one pair of edges and darkens the other by blending
/// toward white/black over the existing pixels (not a solid chrome stroke).
/// Optionally expands the canvas by <see cref="Depth"/> on each side.
/// </summary>
/// <param name="depth">Border thickness in pixels (clamped to 0…64). Default 2.</param>
/// <param name="style">Raised (light top/left) or pressed (swapped).</param>
/// <param name="expandCanvas">
/// When true (default), grows the image by <c>2×depth</c>, extends edge colors into the
/// new frame, then bevels. When false, bevels the existing outer edge in place.
/// </param>
/// <param name="strength">
/// Blend amount toward white/black (0–1). Default 0.65. Outer rings use full strength;
/// inner rings fall off slightly.
/// </param>
public sealed class Bevel(
    int depth = 2,
    BevelStyle style = BevelStyle.Raised,
    bool expandCanvas = true,
    float strength = 0.65f
) : IFilter
{
    /// <summary>
    /// Gets the bevel thickness in pixels.
    /// </summary>
    public int Depth { get; } = depth;

    /// <summary>
    /// Gets whether the bevel is raised or pressed.
    /// </summary>
    public BevelStyle Style { get; } = style;

    /// <summary>
    /// Gets whether the canvas expands before painting.
    /// </summary>
    public bool ExpandCanvas { get; } = expandCanvas;

    /// <summary>
    /// Gets the blend strength toward white/black (0–1).
    /// </summary>
    public float Strength { get; } = strength;

    /// <summary>
    /// Applies the bevel to the whole image (partial <paramref name="targetLocation"/> is ignored).
    /// </summary>
    public Image Apply(Image image, Rectangle targetLocation = default)
    {
        if (image.Width <= 0 || image.Height <= 0)
            return image;

        var d = Math.Clamp(Depth, 0, 64);
        if (d == 0)
            return image;

        if (ExpandCanvas)
            ExpandAndExtendEdges(image, d);

        if (image.Width < 2 || image.Height < 2)
            return image;

        d = Math.Min(d, Math.Min(image.Width, image.Height) / 2);
        if (d == 0)
            return image;

        var raise = Style == BevelStyle.Raised;
        Paint(image, d, Math.Clamp(Strength, 0f, 1f), raise);
        return image;
    }

    /// <summary>
    /// Grows the canvas and fills the new border by extending (clamping) edge pixels
    /// so the bevel has real color to lighten/darken.
    /// </summary>
    private static void ExpandAndExtendEdges(Image image, int depth)
    {
        var srcW = image.Width;
        var srcH = image.Height;
        var snapshot = image.Clone();
        var dstW = srcW + depth * 2;
        var dstH = srcH + depth * 2;
        image.ReCreate(dstW, dstH);

        for (var y = 0; y < dstH; y++)
        {
            var srcY = Math.Clamp(y - depth, 0, srcH - 1);
            var srcRow = snapshot.GetRowSpan(srcY);
            var dstRow = image.GetRowSpan(y);
            for (var x = 0; x < dstW; x++)
            {
                var srcX = Math.Clamp(x - depth, 0, srcW - 1);
                dstRow[x] = srcRow[srcX];
            }
        }
    }

    private static void Paint(Image image, int depth, float strength, bool raised)
    {
        var w = image.Width;
        var h = image.Height;

        for (var i = 0; i < depth; i++)
        {
            // Outer ring strongest; slight falloff inward.
            var t = strength * (1f - i / (float)(depth + 1));

            var yTop = i;
            var yBot = h - 1 - i;
            var xLeft = i;
            var xRight = w - 1 - i;
            if (xRight < xLeft || yBot < yTop)
                break;

            // Raised: light top/left, dark bottom/right. Pressed: swapped.
            var topDark = !raised;
            var botDark = raised;
            var leftDark = !raised;
            var rightDark = raised;

            BlendRow(image.GetRowSpan(yTop), xLeft, xRight, t, topDark);
            BlendRow(image.GetRowSpan(yBot), xLeft, xRight, t, botDark);

            for (var y = yTop + 1; y <= yBot - 1; y++)
            {
                var row = image.GetRowSpan(y);
                row[xLeft] = BlendChannel(row[xLeft], t, leftDark);
                row[xRight] = BlendChannel(row[xRight], t, rightDark);
            }
        }
    }

    private static void BlendRow(Span<Color> row, int x0, int x1, float amount, bool darken)
    {
        for (var x = x0; x <= x1; x++)
            row[x] = BlendChannel(row[x], amount, darken);
    }

    private static Color BlendChannel(Color c, float amount, bool darken)
    {
        if (amount <= 0f)
            return c;

        // Lerp RGB toward white (lighten) or black (darken); keep alpha.
        if (darken)
        {
            return new Color(
                (byte)(c.Red * (1f - amount) + 0.5f),
                (byte)(c.Green * (1f - amount) + 0.5f),
                (byte)(c.Blue * (1f - amount) + 0.5f),
                c.Alpha
            );
        }

        return new Color(
            (byte)(c.Red + (255 - c.Red) * amount + 0.5f),
            (byte)(c.Green + (255 - c.Green) * amount + 0.5f),
            (byte)(c.Blue + (255 - c.Blue) * amount + 0.5f),
            c.Alpha
        );
    }
}
