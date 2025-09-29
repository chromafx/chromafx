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
using ChromaFx.Processing.Filters.Resampling.BaseClasses;
using ChromaFx.Processing.Filters.Resampling.Enums;
using ChromaFx.Processing.Numerics;
using System.Numerics;

namespace ChromaFx.Processing.Filters.Resampling;

/// <summary>
/// Scales an image to the specified width/height
/// </summary>
/// <seealso cref="AffineBaseClass"/>
/// <remarks>
/// Initializes a new instance of the <see cref="Scale"/> class.
/// </remarks>
/// <param name="width">The width.</param>
/// <param name="height">The height.</param>
/// <param name="filter">The filter to use (defaults to nearest neighbor).</param>
public class Scale(int width, int height, ResamplingFiltersAvailable filter = ResamplingFiltersAvailable.NearestNeighbor) : AffineBaseClass(width, height, filter)
{

    /// <summary>
    /// Gets the matrix.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <param name="targetLocation">The target location.</param>
    /// <returns>The transformation matrix</returns>
    protected override Matrix3x2 GetMatrix(Image image, Rectangle targetLocation)
    {
        var xScale = (float)image.Width / Width;
        var yScale = (float)image.Height / Height;
        return Matrix3x2.CreateScale(xScale, yScale, targetLocation.Center);
    }
}