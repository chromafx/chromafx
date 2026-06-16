/*
 * Copyright 2025 Ho Tzin Mein
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
using ChromaFx.Processing.Filters.Effects;
using ChromaFx.Processing.Filters.Interfaces;
using ChromaFx.Processing.Filters.Resampling;
using ChromaFx.Processing.Filters.Resampling.Enums;
using ChromaFx.Processing.Numerics;

namespace ChromaFx.Processing;

/// <summary>
/// Fluent builder for chaining image processing operations.
/// </summary>
public sealed class ImagePipeline
{
    private readonly Image _image;
    private readonly List<IFilter> _filters = [];

    internal ImagePipeline(Image image) => _image = image;

    /// <summary>
    /// Queues a filter to run when the pipeline is executed.
    /// </summary>
    public ImagePipeline Apply(IFilter filter)
    {
        _filters.Add(filter);
        return this;
    }

    /// <summary>
    /// Resizes the image.
    /// </summary>
    public ImagePipeline Resize(
        int width,
        int height,
        ResamplingFiltersAvailable filter = ResamplingFiltersAvailable.Bilinear
    ) => Apply(new Resize(width, height, filter));

    /// <summary>
    /// Rotates the image by the specified angle in degrees.
    /// </summary>
    public ImagePipeline Rotate(float angle) => Apply(new Rotate(angle));

    /// <summary>
    /// Inverts the image colors.
    /// </summary>
    public ImagePipeline Invert() => Apply(new Invert());

    internal Image Execute(Rectangle targetLocation = default)
    {
        foreach (var filter in _filters)
        {
            filter.Apply(_image, targetLocation);
        }

        return _image;
    }
}
