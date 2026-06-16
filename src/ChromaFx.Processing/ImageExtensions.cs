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
using ChromaFx.Processing.Filters.Interfaces;
using ChromaFx.Processing.Numerics;

namespace ChromaFx.Processing;

/// <summary>
/// Extension methods for applying filters and pipelines to images.
/// </summary>
public static class ImageExtensions
{
    /// <summary>
    /// Applies a filter to the image and returns the same instance for chaining.
    /// </summary>
    public static Image Apply(this Image image, IFilter filter, Rectangle targetLocation = default)
    {
        filter.Apply(image, targetLocation);
        return image;
    }

    /// <summary>
    /// Applies a filter type to the image and returns the same instance for chaining.
    /// </summary>
    public static Image Apply<TFilter>(this Image image, Rectangle targetLocation = default)
        where TFilter : IFilter, new() =>
        image.Apply(new TFilter(), targetLocation);

    /// <summary>
    /// Configures and runs a processing pipeline on the image.
    /// </summary>
    public static Image Process(
        this Image image,
        Action<ImagePipeline> configure,
        Rectangle targetLocation = default
    )
    {
        var pipeline = new ImagePipeline(image);
        configure(pipeline);
        return pipeline.Execute(targetLocation);
    }
}
