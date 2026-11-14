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

using ChromaFx.Core.Quantization;
using ChromaFx.Processing;
using ChromaFx.Retro.Filters;

namespace ChromaFx.Retro;

/// <summary>
/// Fluent retro effects for <see cref="IImageProcessingContext"/>.
/// </summary>
public static class ImageProcessingContextExtensions
{
    /// <summary>
    /// Snaps RGB channels to the Netscape web-safe 6×6×6 color cube. Alpha is preserved.
    /// </summary>
    public static IImageProcessingContext WebSafeQuantize(
        this IImageProcessingContext context,
        OrderedDitherMethod dither = OrderedDitherMethod.None,
        float strength = 0.5f
    ) => context.Apply(new WebSafeQuantize(dither, strength));

    /// <summary>
    /// Quantizes to a historical CGA palette (Mode 4 sets, RGBI 16, or 2-color mono). Alpha is preserved.
    /// </summary>
    public static IImageProcessingContext CgaQuantize(
        this IImageProcessingContext context,
        CgaPaletteMode mode = CgaPaletteMode.Palette1High,
        OrderedDitherMethod dither = OrderedDitherMethod.None,
        float strength = 0.5f
    ) => context.Apply(new CgaQuantize(mode, dither, strength));

    /// <summary>
    /// Snaps RGB to the approximate EGA 64-color cube (<c>00/55/AA/FF</c> per channel). Alpha is preserved.
    /// </summary>
    public static IImageProcessingContext EgaQuantize(
        this IImageProcessingContext context,
        OrderedDitherMethod dither = OrderedDitherMethod.None,
        float strength = 0.5f
    ) => context.Apply(new EgaQuantize(dither, strength));

    /// <summary>
    /// Snaps RGB to 6-bit VGA DAC levels (multiples of 4, max 252). Alpha is preserved.
    /// </summary>
    public static IImageProcessingContext VgaQuantize(
        this IImageProcessingContext context,
        OrderedDitherMethod dither = OrderedDitherMethod.None,
        float strength = 0.5f
    ) => context.Apply(new VgaQuantize(dither, strength));

    /// <summary>
    /// Nearest-neighbor downsample then upsample (optional web-safe on the small image).
    /// </summary>
    public static IImageProcessingContext BadWebScale(
        this IImageProcessingContext context,
        float factor = 0.25f,
        bool webSafe = false,
        OrderedDitherMethod dither = OrderedDitherMethod.None,
        float strength = 0.5f
    ) => context.Apply(new BadWebScale(factor, webSafe, dither, strength));

    /// <summary>
    /// Soft bevel (lighten/darken edges over existing pixels). Expands the canvas by default.
    /// </summary>
    public static IImageProcessingContext Bevel(
        this IImageProcessingContext context,
        int depth = 2,
        BevelStyle style = BevelStyle.Raised,
        bool expandCanvas = true,
        float strength = 0.65f
    ) => context.Apply(new Bevel(depth, style, expandCanvas, strength));

    /// <summary>
    /// Nearest-neighbor snap to a target resolution (default 320×200).
    /// </summary>
    public static IImageProcessingContext RetroMode(
        this IImageProcessingContext context,
        int width = 320,
        int height = 200
    ) => context.Apply(new RetroMode(width, height));

    /// <summary>
    /// Rotated circular-dot halftone screen (black ink on white paper look-alike).
    /// </summary>
    public static IImageProcessingContext Halftone(
        this IImageProcessingContext context,
        int cellSize = 6,
        float angleDegrees = 45f,
        float strength = 1f
    ) => context.Apply(new Halftone(cellSize, angleDegrees, strength));

    /// <summary>
    /// Radial red/blue channel shift from the image center (purple-fringe look-alike).
    /// </summary>
    public static IImageProcessingContext ChromaticAberration(
        this IImageProcessingContext context,
        float amount = 3f,
        float strength = 1f
    ) => context.Apply(new ChromaticAberration(amount, strength));

    /// <summary>
    /// Digicam flash glare: clipped whites plus a faint cross streak from bright sources.
    /// </summary>
    public static IImageProcessingContext CcdBloom(
        this IImageProcessingContext context,
        float threshold = 0.85f,
        int radius = 12,
        float strength = 0.65f
    ) => context.Apply(new CcdBloom(threshold, radius, strength));
}
