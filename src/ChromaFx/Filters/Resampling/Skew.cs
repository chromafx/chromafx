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

using ChromaFx.Filters.Interfaces;
using ChromaFx.Filters.Resampling.BaseClasses;
using ChromaFx.Filters.Resampling.Enums;
using ChromaFx.Numerics;
using System.Numerics;

namespace ChromaFx.Filters.Resampling;

/// <summary>
/// Skews an image
/// </summary>
/// <seealso cref="AffineBaseClass"/>
/// <seealso cref="IFilter"/>
/// <remarks>
/// Initializes a new instance of the <see cref="Skew"/> class.
/// </remarks>
/// <param name="xAngle">The x angle.</param>
/// <param name="yAngle">The y angle.</param>
/// <param name="filter">The filter.</param>
public class Skew(float xAngle, float yAngle, ResamplingFiltersAvailable filter = ResamplingFiltersAvailable.NearestNeighbor) : AffineBaseClass(filter: filter)
{

    /// <summary>
    /// Gets or sets the x angle.
    /// </summary>
    /// <value>The x angle.</value>
    public float XAngle { get; } = -xAngle * (float)(Math.PI / 180f);

    /// <summary>
    /// Gets or sets the y angle.
    /// </summary>
    /// <value>The y angle.</value>
    public float YAngle { get; } = -yAngle * (float)(Math.PI / 180f);

    /// <summary>
    /// Gets the matrix.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <param name="targetLocation">The target location.</param>
    /// <returns>The matrix used for the transformation</returns>
    protected override Matrix3x2 GetMatrix(Image image, Rectangle targetLocation)
    {
        return Matrix3x2.CreateSkew(XAngle, YAngle, targetLocation.Center);
    }
}