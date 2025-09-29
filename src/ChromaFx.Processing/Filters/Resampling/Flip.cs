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
using ChromaFx.Processing.Filters.Interfaces;
using ChromaFx.Processing.Filters.Resampling.BaseClasses;
using ChromaFx.Processing.Filters.Resampling.Enums;
using ChromaFx.Processing.Numerics;
using System.Numerics;

namespace ChromaFx.Processing.Filters.Resampling;

/// <summary>
/// Flips the image
/// </summary>
/// <seealso cref="IFilter"/>
/// <remarks>
/// Initializes a new instance of the <see cref="Flip"/> class.
/// </remarks>
/// <param name="direction">The direction.</param>
/// <param name="filter">The filter.</param>
public class Flip(FlipDirection direction, ResamplingFiltersAvailable filter = ResamplingFiltersAvailable.NearestNeighbor) : AffineBaseClass(filter: filter)
{

    /// <summary>
    /// Gets or sets the direction.
    /// </summary>
    /// <value>The direction.</value>
    public FlipDirection Direction { get; set; } = direction;

    /// <summary>
    /// Gets the matrix.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <param name="targetLocation">The target location.</param>
    /// <returns>The matrix used for the transformation</returns>
    protected override Matrix3x2 GetMatrix(Image image, Rectangle targetLocation)
    {
        var xScale = 1f;
        var yScale = 1f;
        xScale = (FlipDirection.Horizontal & Direction) == FlipDirection.Horizontal ? -xScale : xScale;
        yScale = (FlipDirection.Vertical & Direction) == FlipDirection.Vertical ? -yScale : yScale;
        return Matrix3x2.CreateScale(xScale, yScale, targetLocation.Center);
    }
}