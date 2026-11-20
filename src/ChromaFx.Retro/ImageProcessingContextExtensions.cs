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
    /// Snaps RGB to the approximate EGA 64-color cube (<c>00/55/AA/FF</c> per channel),
    /// or adaptively selects 16 of 64 by image frequency. Alpha is preserved.
    /// </summary>
    public static IImageProcessingContext EgaQuantize(
        this IImageProcessingContext context,
        EgaPaletteMode mode = EgaPaletteMode.Full64,
        OrderedDitherMethod dither = OrderedDitherMethod.None,
        float strength = 0.5f
    ) => context.Apply(new EgaQuantize(mode, dither, strength));

    /// <summary>
    /// Snaps RGB to 6-bit VGA DAC levels (multiples of 4, max 252),
    /// or adaptively selects 256 of ~262 144 (Mode 13h style). Alpha is preserved.
    /// </summary>
    public static IImageProcessingContext VgaQuantize(
        this IImageProcessingContext context,
        VgaPaletteMode mode = VgaPaletteMode.DacLevels,
        OrderedDitherMethod dither = OrderedDitherMethod.None,
        float strength = 0.5f
    ) => context.Apply(new VgaQuantize(mode, dither, strength));

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

    /// <summary>
    /// Dramatic lens flare (hot spot, anamorphic streak, colored ghosts).
    /// </summary>
    public static IImageProcessingContext LensFlare(
        this IImageProcessingContext context,
        float centerX = 0.32f,
        float centerY = 0.28f,
        float strength = 0.85f
    ) => context.Apply(new LensFlare(centerX, centerY, strength));

    /// <summary>
    /// Liquid-metal chrome (luma metal ramp + fake specular).
    /// </summary>
    public static IImageProcessingContext LiquidChrome(
        this IImageProcessingContext context,
        float strength = 1f
    ) => context.Apply(new LiquidChrome(strength));

    /// <summary>
    /// Plastic-wrap / jelly sheen (edge highlights).
    /// </summary>
    public static IImageProcessingContext PlasticWrap(
        this IImageProcessingContext context,
        float strength = 0.75f
    ) => context.Apply(new PlasticWrap(strength));

    /// <summary>
    /// Early low-quality JPEG macroblock look-alike (NxN crush; not a DCT codec).
    /// </summary>
    public static IImageProcessingContext JpegBlocks(
        this IImageProcessingContext context,
        int blockSize = 8,
        float strength = 0.85f
    ) => context.Apply(new JpegBlocks(blockSize, strength));

    /// <summary>Page curl from a bottom corner.</summary>
    public static IImageProcessingContext PageCurl(
        this IImageProcessingContext context,
        float amount = 0.55f,
        bool fromRight = true
    ) => context.Apply(new PageCurl(amount, fromRight));

    /// <summary>Twirl / pinch / sphere warp.</summary>
    public static IImageProcessingContext Warp(
        this IImageProcessingContext context,
        WarpMode mode = WarpMode.Twirl,
        float amount = 1f,
        float radius = 0.85f,
        float centerX = 0.5f,
        float centerY = 0.5f
    ) => context.Apply(new Warp(mode, amount, radius, centerX, centerY));

    /// <summary>Circular glass lens refraction + rim.</summary>
    public static IImageProcessingContext GlassLens(
        this IImageProcessingContext context,
        float refraction = 0.65f,
        float radius = 0.45f,
        float centerX = 0.5f,
        float centerY = 0.5f,
        float rim = 0.7f
    ) => context.Apply(new GlassLens(refraction, radius, centerX, centerY, rim));

    /// <summary>Polar kaleidoscope mirror.</summary>
    public static IImageProcessingContext Kaleidoscope(
        this IImageProcessingContext context,
        int segments = 8,
        float centerX = 0.5f,
        float centerY = 0.5f
    ) => context.Apply(new Kaleidoscope(segments, centerX, centerY));

    /// <summary>Polar vortex / tile remapping.</summary>
    public static IImageProcessingContext VortexTile(
        this IImageProcessingContext context,
        float twist = 2f,
        float zoom = 1f,
        float centerX = 0.5f,
        float centerY = 0.5f
    ) => context.Apply(new VortexTile(twist, zoom, centerX, centerY));

    /// <summary>Branching lightning bolt overlay.</summary>
    public static IImageProcessingContext Lightning(
        this IImageProcessingContext context,
        int seed = 42,
        float strength = 0.9f,
        int bolts = 2
    ) => context.Apply(new Lightning(seed, strength, bolts));

    /// <summary>Fractal noise texture overlay.</summary>
    public static IImageProcessingContext FractalNoise(
        this IImageProcessingContext context,
        float strength = 0.55f,
        float frequency = 0.02f,
        int seed = 1337,
        int octaves = 4
    ) => context.Apply(new FractalNoise(strength, frequency, seed, octaves));

    /// <summary>Multi-sine demoscene plasma.</summary>
    public static IImageProcessingContext Plasma(
        this IImageProcessingContext context,
        float strength = 1f,
        float scale = 0.08f,
        float time = 0f
    ) => context.Apply(new Plasma(strength, scale, time));

    /// <summary>Cooling-buffer fire overlay (optional luma-seeded heat).</summary>
    public static IImageProcessingContext Fire(
        this IImageProcessingContext context,
        float strength = 0.85f,
        int seed = 7,
        float cooling = 1f,
        float imageInfluence = 0f
    ) => context.Apply(new Fire(strength, seed, cooling, imageInfluence));
}
