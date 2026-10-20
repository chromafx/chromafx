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
using ChromaFx.Processing.Filters.Interfaces;
using ChromaFx.Processing.Filters.Resampling.Enums;
using ChromaFx.Processing.Numerics;

namespace ChromaFx.Processing;

/// <summary>
/// Fluent image processing operations applied inside <see cref="ImageExtensions.Mutate"/> / <see cref="ImageExtensions.Clone(Image, Action{IImageProcessingContext})"/>.
/// </summary>
public interface IImageProcessingContext
{
    /// <summary>
    /// Gets the image being processed.
    /// </summary>
    Image Image { get; }

    /// <summary>
    /// Applies an arbitrary filter (escape hatch for the full filter set).
    /// </summary>
    IImageProcessingContext Apply(IFilter filter, Rectangle targetLocation = default);

    /// <summary>
    /// Resizes the image.
    /// </summary>
    IImageProcessingContext Resize(
        int width,
        int height,
        ResamplingFiltersAvailable filter = ResamplingFiltersAvailable.Bilinear
    );

    /// <summary>
    /// Rotates the image by degrees.
    /// </summary>
    IImageProcessingContext Rotate(float degrees);

    /// <summary>
    /// Crops to the given rectangle (or the context region if default).
    /// </summary>
    IImageProcessingContext Crop(Rectangle bounds = default);

    /// <summary>
    /// Flips the image.
    /// </summary>
    IImageProcessingContext Flip(FlipDirection direction);

    /// <summary>
    /// Inverts RGB channels.
    /// </summary>
    IImageProcessingContext Invert();

    /// <summary>
    /// Adjusts brightness (−1…1).
    /// </summary>
    IImageProcessingContext Brightness(float value);

    /// <summary>
    /// Adjusts contrast (0…1+).
    /// </summary>
    IImageProcessingContext Contrast(float value);

    /// <summary>
    /// Applies gamma correction.
    /// </summary>
    IImageProcessingContext Gamma(float value);

    /// <summary>
    /// Converts to greyscale (BT.709).
    /// </summary>
    IImageProcessingContext Greyscale();

    /// <summary>
    /// Box-based Gaussian blur approximation.
    /// </summary>
    /// <param name="radius">Blur radius; mapped to an integer kernel size ≥ 1.</param>
    IImageProcessingContext GaussianBlur(float radius);

    /// <summary>
    /// Sharpens the image.
    /// </summary>
    IImageProcessingContext Sharpen();

    /// <summary>
    /// Binary threshold using black/white.
    /// </summary>
    IImageProcessingContext Threshold(float threshold);

    /// <summary>
    /// Draws a filled or stroked rectangle.
    /// </summary>
    IImageProcessingContext DrawRectangle(Color color, bool filled, Rectangle bounds);

    /// <summary>
    /// Draws an ellipse.
    /// </summary>
    IImageProcessingContext DrawEllipse(Color color, bool filled, Rectangle bounds);
}
