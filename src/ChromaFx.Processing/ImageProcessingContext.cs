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
using ChromaFx.Processing.Filters.Binary;
using ChromaFx.Processing.Filters.ColorMatrix;
using ChromaFx.Processing.Filters.Convolution;
using ChromaFx.Processing.Filters.Effects;
using ChromaFx.Processing.Filters.Interfaces;
using ChromaFx.Processing.Filters.Normalization;
using ChromaFx.Processing.Filters.Pipelines;
using ChromaFx.Processing.Filters.Resampling;
using ChromaFx.Processing.Filters.Resampling.Enums;
using ChromaFx.Processing.Numerics;

namespace ChromaFx.Processing;

internal sealed class ImageProcessingContext(Image image, Rectangle region)
    : IImageProcessingContext
{
    private readonly Rectangle _region = region;

    public Image Image { get; } = image;

    public IImageProcessingContext Apply(IFilter filter, Rectangle targetLocation = default)
    {
        var loc = targetLocation == default ? _region : targetLocation;
        filter.Apply(Image, loc);
        return this;
    }

    public IImageProcessingContext Resize(
        int width,
        int height,
        ResamplingFiltersAvailable filter = ResamplingFiltersAvailable.Bilinear
    ) => Apply(new Resize(width, height, filter));

    public IImageProcessingContext Rotate(float degrees) => Apply(new Rotate(degrees));

    public IImageProcessingContext Crop(Rectangle bounds = default)
    {
        var loc = bounds == default ? _region : bounds;
        return Apply(new Crop(), loc);
    }

    public IImageProcessingContext Flip(FlipDirection direction) => Apply(new Flip(direction));

    public IImageProcessingContext Invert() => Apply(new Invert());

    public IImageProcessingContext Brightness(float value) => Apply(new Brightness(value));

    public IImageProcessingContext Contrast(float value) => Apply(new Contrast(value));

    public IImageProcessingContext Gamma(float value) => Apply(new Gamma(value));

    public IImageProcessingContext Greyscale() => Apply(new Greyscale709());

    public IImageProcessingContext GaussianBlur(float radius)
    {
        var size = Math.Max(1, (int)Math.Ceiling(Math.Abs(radius)));
        return Apply(new GaussianBlur(size));
    }

    public IImageProcessingContext Sharpen() => Apply(new Sharpen());

    public IImageProcessingContext Threshold(float threshold) =>
        Apply(new Threshold(Color.Black, Color.White, threshold));

    public IImageProcessingContext DrawRectangle(Color color, bool filled, Rectangle bounds) =>
        Apply(new Filters.Drawing.Rectangle(color, filled, bounds));

    public IImageProcessingContext DrawEllipse(Color color, bool filled, Rectangle bounds) =>
        Apply(
            new Filters.Drawing.Ellipse(color, filled, bounds.Width, bounds.Height, bounds.Center)
        );
}
