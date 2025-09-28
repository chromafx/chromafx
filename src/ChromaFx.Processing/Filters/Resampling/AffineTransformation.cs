/*
Copyright 2025 Ho Tzin Mein

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using ChromaFx.Core;
using ChromaFx.Processing.Filters.Resampling.BaseClasses;
using ChromaFx.Processing.Filters.Resampling.Enums;
using ChromaFx.Processing.Numerics;
using System.Numerics;

namespace ChromaFx.Processing.Filters.Resampling;

/// <summary>
/// Affine transformation
/// </summary>
/// <seealso cref="AffineBaseClass"/>
/// <remarks>
/// Initializes a new instance of the <see cref="AffineTransformation"/> class.
/// </remarks>
/// <param name="matrix">The matrix.</param>
/// <param name="width">The new width.</param>
/// <param name="height">The new height.</param>
/// <param name="filter">The filter to use (defaults to nearest neighbor).</param>
public class AffineTransformation(Matrix3x2 matrix, int width = -1, int height = -1, ResamplingFiltersAvailable filter = ResamplingFiltersAvailable.NearestNeighbor) : AffineBaseClass(width, height, filter)
{

    /// <summary>
    /// Gets or sets the matrix.
    /// </summary>
    /// <value>The matrix.</value>
    public Matrix3x2 Matrix { get; set; } = matrix;

    /// <summary>
    /// Implements the operator *.
    /// </summary>
    /// <param name="value1">The value1.</param>
    /// <param name="value2">The value2.</param>
    /// <returns>The result of the operator.</returns>
    public static AffineTransformation operator *(AffineTransformation value1, AffineTransformation value2)
    {
        return new AffineTransformation(value1.Matrix * value2.Matrix);
    }

    /// <summary>
    /// Gets the matrix.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <param name="targetLocation">The target location.</param>
    /// <returns>The matrix used for the transformation</returns>
    protected override Matrix3x2 GetMatrix(Image image, Rectangle targetLocation)
    {
        return Matrix;
    }
}