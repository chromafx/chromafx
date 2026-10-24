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

using ChromaFx.Processing.Filters.ColorMatrix;
using ChromaFx.Processing.Filters.Convolution;
using ChromaFx.Processing.Filters.Effects;
using ChromaFx.Processing.Filters.Morphology;
using ChromaFx.Processing.Filters.Normalization;
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
}
