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

using ChromaFx.Processing.Filters.Resampling.ResamplingFilters.HelperClasses;
using ChromaFx.Processing.Filters.Resampling.ResamplingFilters.Interfaces;

namespace ChromaFx.Processing.Filters.Resampling.ResamplingFilters.BaseClasses;

/// <summary>
/// Resampling filter base class
/// </summary>
/// <seealso cref="IResamplingFilter"/>
public abstract class ResamplingFilterBase : IResamplingFilter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ResamplingFilterBase"/> class.
    /// </summary>
    protected ResamplingFilterBase()
    {
    }

    /// <summary>
    /// Gets the filter radius.
    /// </summary>
    /// <value>The filter radius.</value>
    public abstract float FilterRadius { get; }

    /// <summary>
    /// Gets the precomputed weights.
    /// </summary>
    /// <value>The precomputed weights.</value>
    public Weights[] XWeights { get; private set; }

    /// <summary>
    /// Gets the precomputed y axis weights.
    /// </summary>
    /// <value>The precomputed y axis weights.</value>
    public Weights[] YWeights { get; private set; }

    /// <summary>
    /// Gets the value based on the resampling filter.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The new value based on the input.</returns>
    public abstract double GetValue(double value);

    /// <summary>
    /// Precomputes the weights based on the values passed in.
    /// </summary>
    /// <param name="oldWidth">The old width.</param>
    /// <param name="oldHeight">The old height.</param>
    /// <param name="newWidth">The new width.</param>
    /// <param name="newHeight">The new height.</param>
    public void Precompute(int oldWidth, int oldHeight, int newWidth, int newHeight)
    {
        var destinationWidth = newWidth < 0 ? oldWidth : newWidth;
        var destinationHeight = newHeight < 0 ? oldWidth : newHeight;
        XWeights = PrecomputeWeights(destinationWidth, oldWidth);
        YWeights = PrecomputeWeights(destinationHeight, oldHeight);
    }

    private Weights[] PrecomputeWeights(int destinationSize, int sourceSize)
    {
        var scale = destinationSize / (double)sourceSize;
        var radius = scale < 1f ? (FilterRadius / scale) : FilterRadius;
        var result = new Weights[sourceSize];

        for (var x = 0; x < sourceSize; ++x)
        {
            var left = (int)(x - radius);
            var right = (int)(x + radius);
            if (left < 0)
                left = 0;
            if (right >= sourceSize)
                right = sourceSize - 1;
            result[x] = new Weights
            {
                Values = new double[(right - left) + 1]
            };
            for (int y = left, count = 0; y <= right; ++y, ++count)
            {
                result[x].Values[count] = scale < 1f ?
                    GetValue((x - y) * scale) :
                    GetValue(x - y);
                result[x].TotalWeight += result[x].Values[count];
            }

            if (result[x].TotalWeight > 0)
            {
                for (var y = 0; y < result[x].Values.Length; ++y)
                {
                    result[x].Values[y] /= result[x].TotalWeight;
                }
            }
        }
        return result;
    }
}