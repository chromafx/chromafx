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

using ChromaFx.Processing.Filters.ColorMatrix.BaseClasses;
using ChromaFx.Processing.Numerics;

namespace ChromaFx.Processing.Filters.ColorMatrix;

/// <summary>
/// Contrast matrix
/// </summary>
/// <seealso cref="MatrixBaseClass" />
public class Contrast : MatrixBaseClass
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Contrast"/> class.
    /// </summary>
    /// <param name="value">The Contrast value (0 to 1).</param>
    public Contrast(float value)
    {
        Value = value;
        var tempValue = 0.5f * (1f - value);
        Matrix = new Matrix5X5
        (
            value, 0f, 0f, 0f, 0f,
            0f, value, 0f, 0f, 0f,
            0f, 0f, value, 0f, 0f,
            0f, 0f, 0f, 1f, 0f,
            tempValue, tempValue, tempValue, 0f, 1f
        );
    }

    /// <summary>
    /// Gets the matrix.
    /// </summary>
    /// <value>The matrix.</value>
    public override Matrix5X5 Matrix { get; }

    /// <summary>
    /// Gets the value.
    /// </summary>
    /// <value>The value.</value>
    public float Value { get; }
}