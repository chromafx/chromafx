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
using System.Numerics;
using ChromaFx.Processing.Filters;
using ChromaFx.Processing.Filters.Binary;
using ChromaFx.Processing.Filters.ColorMatrix;
using ChromaFx.Processing.Filters.Convolution;
using ChromaFx.Processing.Filters.Effects;
using ChromaFx.Processing.Filters.Morphology;
using ChromaFx.Processing.Filters.Normalization;
using ChromaFx.Processing.Filters.Overlays;
using ChromaFx.Processing.Filters.Pipelines;
using ChromaFx.Processing.Filters.Resampling;
using ChromaFx.Processing.Filters.Sharpening;
using ChromaFx.Processing.Filters.Smoothing;
using ChromaFx.Processing.Numerics;

namespace ChromaFx.Processing;

/// <summary>
/// Additional fluent operations for <see cref="IImageProcessingContext"/>.
/// </summary>
public static class ImageProcessingContextExtensions
{
    /// <summary>
    /// US spelling alias for <see cref="IImageProcessingContext.Greyscale"/>.
    /// </summary>
    public static IImageProcessingContext Grayscale(this IImageProcessingContext context) =>
        context.Greyscale();

    /// <summary>
    /// Applies a box blur of the given kernel size.
    /// </summary>
    public static IImageProcessingContext BoxBlur(
        this IImageProcessingContext context,
        int size = 3
    ) => context.Apply(new BoxBlur(size));

    /// <summary>
    /// Adjusts saturation (−1…1).
    /// </summary>
    public static IImageProcessingContext Saturation(
        this IImageProcessingContext context,
        float value
    ) => context.Apply(new Saturation(value));

    /// <summary>
    /// Rotates hue by degrees (0…360).
    /// </summary>
    public static IImageProcessingContext Hue(this IImageProcessingContext context, float degrees) =>
        context.Apply(new Hue(degrees));

    /// <summary>
    /// Morphological dilation.
    /// </summary>
    public static IImageProcessingContext Dilate(
        this IImageProcessingContext context,
        int apertureRadius = 1
    ) => context.Apply(new Dilate(apertureRadius));

    /// <summary>
    /// Morphological erosion (constrict).
    /// </summary>
    public static IImageProcessingContext Erode(
        this IImageProcessingContext context,
        int apertureRadius = 1
    ) => context.Apply(new Constrict(apertureRadius));

    /// <summary>
    /// Adds noise (0–255 channel jitter amount).
    /// </summary>
    public static IImageProcessingContext Noise(this IImageProcessingContext context, byte amount) =>
        context.Apply(new Noise(amount));

    /// <summary>
    /// Pixellates the image.
    /// </summary>
    public static IImageProcessingContext Pixellate(
        this IImageProcessingContext context,
        int pixelSize
    ) => context.Apply(new Pixellate(pixelSize));

    /// <summary>
    /// Sepia tone.
    /// </summary>
    public static IImageProcessingContext Sepia(this IImageProcessingContext context) =>
        context.Apply(new Sepiatone());

    /// <summary>
    /// Emboss convolution.
    /// </summary>
    public static IImageProcessingContext Emboss(this IImageProcessingContext context) =>
        context.Apply(new Emboss());

    /// <summary>
    /// Sobel emboss.
    /// </summary>
    public static IImageProcessingContext SobelEmboss(this IImageProcessingContext context) =>
        context.Apply(new SobelEmboss());

    /// <summary>
    /// Stretch contrast normalization.
    /// </summary>
    public static IImageProcessingContext StretchContrast(this IImageProcessingContext context) =>
        context.Apply(new StretchContrast());

    /// <summary>
    /// Histogram equalize.
    /// </summary>
    public static IImageProcessingContext Equalize(this IImageProcessingContext context) =>
        context.Apply(new Equalize());

    /// <summary>
    /// Adaptive histogram equalize.
    /// </summary>
    public static IImageProcessingContext AdaptiveEqualize(
        this IImageProcessingContext context,
        int radius = 64
    ) => context.Apply(new AdaptiveEqualize(radius));

    /// <summary>
    /// Logarithm normalization.
    /// </summary>
    public static IImageProcessingContext Logarithm(this IImageProcessingContext context) =>
        context.Apply(new Logarithm());

    /// <summary>
    /// Multiplies RGB by <paramref name="value"/> (alpha matrix).
    /// </summary>
    public static IImageProcessingContext Alpha(this IImageProcessingContext context, float value) =>
        context.Apply(new Alpha(value));

    /// <summary>
    /// Tints toward a blackbody illuminant (Helland Kelvin→RGB multiply). Not white balance.
    /// </summary>
    public static IImageProcessingContext KelvinTint(
        this IImageProcessingContext context,
        double kelvin
    ) => context.Apply(new KelvinTint(kelvin));

    /// <summary>
    /// Green↔magenta tint (−1…1; positive = green).
    /// </summary>
    public static IImageProcessingContext Tint(this IImageProcessingContext context, float amount) =>
        context.Apply(new Tint(amount));

    /// <summary>
    /// Corrects white balance for a known illuminant Kelvin (optional green↔magenta tint).
    /// </summary>
    public static IImageProcessingContext WhiteBalance(
        this IImageProcessingContext context,
        double kelvin,
        float tint = 0f
    ) => context.Apply(new WhiteBalance(kelvin, tint));

    /// <summary>
    /// Auto white-balance via Gray World or White Patch.
    /// </summary>
    public static IImageProcessingContext AutoWhiteBalance(
        this IImageProcessingContext context,
        AutoWhiteBalanceMethod method = AutoWhiteBalanceMethod.GrayWorld,
        float whitePatchPercentile = 99f
    ) => context.Apply(new AutoWhiteBalance(method, whitePatchPercentile));

    /// <summary>
    /// Kodachrome color matrix.
    /// </summary>
    public static IImageProcessingContext Kodachrome(this IImageProcessingContext context) =>
        context.Apply(new Kodachrome());

    /// <summary>
    /// Polaroid processing pipeline.
    /// </summary>
    public static IImageProcessingContext Polaroid(this IImageProcessingContext context) =>
        context.Apply(new Polaroid());

    /// <summary>
    /// Lomograph processing pipeline.
    /// </summary>
    public static IImageProcessingContext Lomograph(this IImageProcessingContext context) =>
        context.Apply(new Lomograph());

    /// <summary>
    /// Median smooth.
    /// </summary>
    public static IImageProcessingContext Median(
        this IImageProcessingContext context,
        int apertureRadius = 1
    ) => context.Apply(new Median(apertureRadius));

    /// <summary>
    /// Kuwahara smooth.
    /// </summary>
    public static IImageProcessingContext Kuwahara(
        this IImageProcessingContext context,
        int apertureRadius = 3
    ) => context.Apply(new Kuwahara(apertureRadius));

    /// <summary>
    /// Symmetric nearest-neighbor blur.
    /// </summary>
    public static IImageProcessingContext SnnBlur(
        this IImageProcessingContext context,
        int apertureRadius = 1
    ) => context.Apply(new SnnBlur(apertureRadius));

    /// <summary>
    /// Unsharp mask.
    /// </summary>
    public static IImageProcessingContext Unsharp(
        this IImageProcessingContext context,
        int size = 3,
        float amount = 0.5f
    ) => context.Apply(new Unsharp(size, amount));

    /// <summary>
    /// Posterize to the given number of tonal divisions.
    /// </summary>
    public static IImageProcessingContext Posterize(
        this IImageProcessingContext context,
        int divisions = 4
    ) => context.Apply(new Posterize(divisions));

    /// <summary>
    /// Solarize above the threshold (0…1).
    /// </summary>
    public static IImageProcessingContext Solarize(
        this IImageProcessingContext context,
        float threshold = 0.5f
    ) => context.Apply(new Solarize(threshold));

    /// <summary>
    /// Spatial jitter.
    /// </summary>
    public static IImageProcessingContext Jitter(
        this IImageProcessingContext context,
        int amount = 5
    ) => context.Apply(new Jitter(amount));

    /// <summary>
    /// Soft glow overlay.
    /// </summary>
    public static IImageProcessingContext Glow(
        this IImageProcessingContext context,
        Color color,
        float xRadius = 0.5f,
        float yRadius = 0.5f
    ) => context.Apply(new Glow(color, xRadius, yRadius));

    /// <summary>
    /// Vignette overlay.
    /// </summary>
    public static IImageProcessingContext Vignette(
        this IImageProcessingContext context,
        Color color,
        float xRadius = 0.5f,
        float yRadius = 0.5f
    ) => context.Apply(new Vignette(color, xRadius, yRadius));

    /// <summary>
    /// Blend another image (normal alpha composite by default).
    /// </summary>
    public static IImageProcessingContext Blend(
        this IImageProcessingContext context,
        Image overlay,
        float alpha,
        Rectangle sourceLocation = default
    ) => context.Apply(new Blend(overlay, alpha, sourceLocation));

    /// <summary>
    /// Blend another image with a Photoshop-style mode (Multiply / Screen / Overlay / SoftLight).
    /// </summary>
    public static IImageProcessingContext Blend(
        this IImageProcessingContext context,
        Image overlay,
        float alpha,
        BlendMode mode,
        Rectangle sourceLocation = default
    ) => context.Apply(new Blend(overlay, alpha, mode, sourceLocation));

    /// <summary>
    /// Remap tonal levels (input/output black–white + midtone gamma). Parameters are 0–1.
    /// </summary>
    public static IImageProcessingContext Levels(
        this IImageProcessingContext context,
        float inBlack = 0f,
        float inWhite = 1f,
        float gamma = 1f,
        float outBlack = 0f,
        float outWhite = 1f
    ) => context.Apply(new Levels(inBlack, inWhite, gamma, outBlack, outWhite));

    /// <summary>
    /// Greyscale + Laplace edge detect preset.
    /// </summary>
    public static IImageProcessingContext EdgeDetect(this IImageProcessingContext context) =>
        context.Apply(new EdgeDetect());

    /// <summary>
    /// Morphological opening (erode then dilate).
    /// </summary>
    public static IImageProcessingContext MorphOpen(
        this IImageProcessingContext context,
        int apertureRadius = 1
    ) => context.Apply(new MorphOpen(apertureRadius));

    /// <summary>
    /// Morphological closing (dilate then erode).
    /// </summary>
    public static IImageProcessingContext MorphClose(
        this IImageProcessingContext context,
        int apertureRadius = 1
    ) => context.Apply(new MorphClose(apertureRadius));

    /// <summary>
    /// Maps luminance onto a two-color gradient. Defaults to a classic cool→warm look.
    /// </summary>
    public static IImageProcessingContext Duotone(
        this IImageProcessingContext context,
        Color? shadow = null,
        Color? highlight = null
    ) =>
        context.Apply(
            new Duotone(shadow ?? new Color(20, 40, 90), highlight ?? new Color(245, 210, 150))
        );

    /// <summary>
    /// Tints toward a color while keeping luminance.
    /// </summary>
    public static IImageProcessingContext Colorize(
        this IImageProcessingContext context,
        Color tint,
        float amount = 1f
    ) => context.Apply(new Colorize(tint, amount));

    /// <summary>
    /// Directional motion blur.
    /// </summary>
    public static IImageProcessingContext MotionBlur(
        this IImageProcessingContext context,
        int length = 9,
        float angleDegrees = 0f
    ) => context.Apply(new MotionBlur(length, angleDegrees));

    /// <summary>
    /// Lift shadows / compress highlights (−1…1 amounts).
    /// </summary>
    public static IImageProcessingContext ShadowsHighlights(
        this IImageProcessingContext context,
        float shadows = 0.25f,
        float highlights = -0.25f
    ) => context.Apply(new ShadowsHighlights(shadows, highlights));

    /// <summary>
    /// Classic oil-paint look.
    /// </summary>
    public static IImageProcessingContext OilPaint(
        this IImageProcessingContext context,
        int radius = 3,
        int levels = 20
    ) => context.Apply(new OilPaint(radius, levels));

    /// <summary>
    /// Octree palette quantization (2–256 colors) with optional dither. Alpha preserved.
    /// </summary>
    public static IImageProcessingContext Quantize(
        this IImageProcessingContext context,
        int maxColors = 256,
        DitherMethod dither = DitherMethod.None,
        float strength = 0.5f
    ) => context.Apply(new Quantize(maxColors, dither, strength));

    /// <summary>
    /// Adaptive threshold.
    /// </summary>
    public static IImageProcessingContext AdaptiveThreshold(
        this IImageProcessingContext context,
        int apertureRadius = 10,
        float threshold = 0.5f,
        Color? dark = null,
        Color? light = null
    ) =>
        context.Apply(
            new AdaptiveThreshold(
                apertureRadius,
                dark ?? Color.Black,
                light ?? Color.White,
                threshold
            )
        );

    /// <summary>
    /// Canny edge detection pipeline.
    /// </summary>
    public static IImageProcessingContext Canny(
        this IImageProcessingContext context,
        float threshold1 = 0.01f,
        float threshold2 = 0.17f,
        Color? edge = null,
        Color? background = null
    ) =>
        context.Apply(
            new CannyEdgeDetection(
                edge ?? Color.White,
                background ?? Color.Black,
                threshold1,
                threshold2
            )
        );

    /// <summary>
    /// Draws an anti-aliased line.
    /// </summary>
    public static IImageProcessingContext DrawLine(
        this IImageProcessingContext context,
        Color color,
        int x1,
        int y1,
        int x2,
        int y2
    ) => context.Apply(new Filters.Drawing.Line(color, x1, y1, x2, y2));

    /// <summary>
    /// Draws an open polyline. <paramref name="strokeWidth"/> 1 is anti-aliased; larger widths use an opaque square brush.
    /// </summary>
    public static IImageProcessingContext DrawPolyline(
        this IImageProcessingContext context,
        Color color,
        int strokeWidth,
        params Vector2[] points
    ) =>
        context.Apply(new Filters.Drawing.Polyline(color, points) { StrokeWidth = strokeWidth });

    /// <summary>
    /// Draws an open polyline with stroke width 1.
    /// </summary>
    public static IImageProcessingContext DrawPolyline(
        this IImageProcessingContext context,
        Color color,
        params Vector2[] points
    ) => context.DrawPolyline(color, 1, points);

    /// <summary>
    /// Draws a closed polygon (filled or stroked).
    /// </summary>
    public static IImageProcessingContext DrawPolygon(
        this IImageProcessingContext context,
        Color color,
        bool filled,
        params Vector2[] points
    ) => context.Apply(new Filters.Drawing.Polygon(color, filled, points));

    /// <summary>
    /// Draws a closed polygon outline with the given stroke width.
    /// </summary>
    public static IImageProcessingContext DrawPolygon(
        this IImageProcessingContext context,
        Color color,
        bool filled,
        int strokeWidth,
        params Vector2[] points
    ) =>
        context.Apply(
            new Filters.Drawing.Polygon(color, filled, points) { StrokeWidth = strokeWidth }
        );

    /// <summary>
    /// 4-connected flood fill from a seed pixel (exact color match).
    /// </summary>
    public static IImageProcessingContext FloodFill(
        this IImageProcessingContext context,
        Color color,
        int x,
        int y
    ) => context.Apply(new Filters.Drawing.FloodFill(color, x, y));

    /// <summary>
    /// Skews by X/Y angles in degrees.
    /// </summary>
    public static IImageProcessingContext Skew(
        this IImageProcessingContext context,
        float xAngle,
        float yAngle,
        ResamplingFilter filter = ResamplingFilter.NearestNeighbor
    ) => context.Apply(new Skew(xAngle, yAngle, filter));

    /// <summary>
    /// Translates by pixel deltas.
    /// </summary>
    public static IImageProcessingContext Translate(
        this IImageProcessingContext context,
        int xDelta,
        int yDelta
    ) => context.Apply(new Translate(xDelta, yDelta));

    /// <summary>
    /// Resizes the canvas without resampling the existing pixels.
    /// </summary>
    public static IImageProcessingContext ResizeCanvas(
        this IImageProcessingContext context,
        int width,
        int height,
        ResizeOptions options = ResizeOptions.UpperLeft
    ) => context.Apply(new ResizeCanvas(width, height, options));
}
