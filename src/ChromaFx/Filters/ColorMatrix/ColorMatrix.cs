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

using ChromaFx.Filters.ColorMatrix.BaseClasses;
using ChromaFx.Numerics;

namespace ChromaFx.Filters.ColorMatrix;

/// <summary>
/// Generic ColorMatrix
/// </summary>
/// <seealso cref="MatrixBaseClass"/>
public class ColorMatrix : MatrixBaseClass
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ColorMatrix"/> class.
    /// </summary>
    /// <param name="matrix">The matrix.</param>
    public ColorMatrix(Matrix5X5 matrix)
    {
        Matrix = matrix;
    }

    /// <summary>
    /// Gets the matrix.
    /// </summary>
    /// <value>The matrix.</value>
    public override Matrix5X5 Matrix { get; }
}