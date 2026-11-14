/*
 * Copyright 2017–2020 JaCraig
 * Modifications Copyright 2023–2026 Ho Tzin Mein
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
using ChromaFx.Processing.Filters.ColorMatrix;
using ChromaFx.Processing.Numerics;

namespace ChromaFx.Processing.Filters.Overlays;

/// <summary>
/// Blends two images together with optional Photoshop-style modes.
/// </summary>
/// <seealso cref="IFilter"/>
public class Blend : IFilter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Blend"/> class (normal alpha blend).
    /// </summary>
    /// <param name="image">The image to apply.</param>
    /// <param name="alpha">The alpha / opacity value for the image (0–1).</param>
    /// <param name="sourceLocation">The source location.</param>
    public Blend(Image image, float alpha, Rectangle sourceLocation = default)
        : this(image, alpha, BlendMode.Normal, sourceLocation) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Blend"/> class.
    /// </summary>
    /// <param name="image">The overlay image.</param>
    /// <param name="alpha">Opacity (0–1). For <see cref="BlendMode.Normal"/> this scales overlay alpha first.</param>
    /// <param name="mode">Blend mode.</param>
    /// <param name="sourceLocation">Region of the overlay to sample.</param>
    public Blend(Image image, float alpha, BlendMode mode, Rectangle sourceLocation = default)
    {
        Alpha = alpha;
        Mode = mode;
        image = image.Clone();
        if (mode == BlendMode.Normal)
            new Alpha(alpha).Apply(image);
        Image = image;
        SourceLocation = sourceLocation;
        SourceLocation =
            SourceLocation == default
                ? new Rectangle(0, 0, Image.Width, Image.Height)
                : SourceLocation.Clamp(Image);
    }

    /// <summary>
    /// Gets the opacity / alpha parameter.
    /// </summary>
    public float Alpha { get; }

    /// <summary>
    /// Gets the blend mode.
    /// </summary>
    public BlendMode Mode { get; }

    /// <summary>
    /// Gets the overlay image.
    /// </summary>
    public Image Image { get; }

    /// <summary>
    /// Gets the source location on the overlay.
    /// </summary>
    public Rectangle SourceLocation { get; }

    /// <summary>
    /// Applies the blend.
    /// </summary>
    public Image Apply(Image image, Rectangle targetLocation = default)
    {
        targetLocation = targetLocation.Normalize(image);

        for (
            int y1 = targetLocation.Bottom, y2 = SourceLocation.Bottom;
            y1 < targetLocation.Top && y2 < SourceLocation.Top;
            ++y1, ++y2
        )
        {
            var targetRow = image.GetRowSpan(y1).Slice(targetLocation.Left, targetLocation.Width);
            var sourceRow = Image.GetRowSpan(y2).Slice(SourceLocation.Left, SourceLocation.Width);

            var width = Math.Min(targetLocation.Width, SourceLocation.Width);
            if (Mode == BlendMode.Normal)
            {
                for (var x = 0; x < width; ++x)
                {
                    var tempAlpha = sourceRow[x].Alpha / 255f;
                    targetRow[x] = targetRow[x] * (1f - tempAlpha) + sourceRow[x] * tempAlpha;
                }
            }
            else
            {
                var opacity = Math.Clamp(Alpha, 0f, 1f);
                for (var x = 0; x < width; ++x)
                {
                    var dst = targetRow[x];
                    var src = sourceRow[x];
                    var factor = opacity * (src.Alpha / 255f);
                    if (factor <= 0f)
                        continue;

                    var blended = BlendChannels(dst, src, Mode);
                    targetRow[x] = new Color(
                        Lerp(dst.Red, blended.Red, factor),
                        Lerp(dst.Green, blended.Green, factor),
                        Lerp(dst.Blue, blended.Blue, factor),
                        dst.Alpha
                    );
                }
            }
        }

        return image;
    }

    private static Color BlendChannels(Color dst, Color src, BlendMode mode)
    {
        return mode switch
        {
            BlendMode.Multiply => Multiply(dst, src),
            BlendMode.Screen => Screen(dst, src),
            BlendMode.Overlay => Overlay(dst, src),
            BlendMode.SoftLight => SoftLight(dst, src),
            _ => src,
        };
    }

    private static Color Multiply(Color a, Color b) =>
        new(
            Mul(a.Red, b.Red),
            Mul(a.Green, b.Green),
            Mul(a.Blue, b.Blue),
            a.Alpha
        );

    private static Color Screen(Color a, Color b) =>
        new(
            Scr(a.Red, b.Red),
            Scr(a.Green, b.Green),
            Scr(a.Blue, b.Blue),
            a.Alpha
        );

    private static Color Overlay(Color a, Color b) =>
        new(
            Ovl(a.Red, b.Red),
            Ovl(a.Green, b.Green),
            Ovl(a.Blue, b.Blue),
            a.Alpha
        );

    private static Color SoftLight(Color a, Color b) =>
        new(
            Soft(a.Red, b.Red),
            Soft(a.Green, b.Green),
            Soft(a.Blue, b.Blue),
            a.Alpha
        );

    private static byte Mul(byte a, byte b) => (byte)(a * b / 255);

    private static byte Scr(byte a, byte b) => (byte)(255 - (255 - a) * (255 - b) / 255);

    private static byte Ovl(byte a, byte b) =>
        a < 128 ? (byte)(2 * a * b / 255) : (byte)(255 - 2 * (255 - a) * (255 - b) / 255);

    // Pegtop soft-light: (1-2b)*a² + 2b*a  (channels 0–1).
    private static byte Soft(byte a, byte b)
    {
        var af = a / 255f;
        var bf = b / 255f;
        var r = (1f - 2f * bf) * af * af + 2f * bf * af;
        return (byte)Math.Clamp((int)(r * 255f + 0.5f), 0, 255);
    }

    private static byte Lerp(byte a, byte b, float t) =>
        (byte)Math.Clamp((int)(a + (b - a) * t + 0.5f), 0, 255);
}
