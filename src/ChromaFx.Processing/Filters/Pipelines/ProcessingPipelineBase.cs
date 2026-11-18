/*
 * Copyright 2017–2020 JaCraig
 * Modifications Copyright 2023–2025 Ho Tzin Mein
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
using ChromaFx.Processing.Filters.ColorMatrix;
using ChromaFx.Processing.Filters.Convolution;
using ChromaFx.Processing.Numerics;

namespace ChromaFx.Processing.Filters.Pipelines;

/// <summary>
/// Processing pipeline base class
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ProcessingPipelineBase"/> class.
/// </remarks>
/// <param name="combine">
/// if set to <c>true</c> [combine] the convolution filters when possible.
/// </param>
public abstract class ProcessingPipelineBase(bool combine) : IFilter
{
    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="ProcessingPipelineBase"/>
    /// should combine the filters or not.
    /// </summary>
    /// <value><c>true</c> if combine; otherwise, <c>false</c>.</value>
    public bool Combine { get; } = combine;

    /// <summary>
    /// Gets the filters.
    /// </summary>
    /// <value>The filters.</value>
    public List<IFilter> Filters { get; } = [];

    /// <summary>
    /// Adds the filter to the pipeline
    /// </summary>
    /// <param name="filter">The filter.</param>
    /// <returns>This</returns>
    public ProcessingPipelineBase AddFilter(IFilter filter)
    {
        if (filter as MatrixFilterBase != null && Filters.Count > 0)
        {
            var lastFilter = Filters[^1];
            if (lastFilter as MatrixFilterBase != null)
            {
                Filters.Remove(lastFilter);
                filter = (MatrixFilterBase)lastFilter * (MatrixFilterBase)filter;
            }
        }
        else if (Combine && filter as ConvolutionBase != null && Filters.Count > 0)
        {
            var lastFilter = Filters[^1];
            if (lastFilter as ConvolutionBase != null)
            {
                Filters.Remove(lastFilter);
                filter = (ConvolutionBase)lastFilter * (ConvolutionBase)filter;
            }
        }
        Filters.Add(filter);
        return this;
    }

    /// <summary>
    /// Executes the specified image.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <param name="targetLocation">The target location.</param>
    /// <returns>The resulting image</returns>
    public Image Apply(Image image, Rectangle targetLocation = default)
    {
        for (var x = 0; x < Filters.Count; ++x)
        {
            Filters[x].Apply(image, targetLocation);
        }
        return image;
    }
}
