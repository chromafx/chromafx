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

using ChromaFx.Processing.Filters.ColorMatrix.BaseClasses;
using ChromaFx.Processing.Numerics;

namespace ChromaFx.Processing.Filters.ColorMatrix;

/// <summary>
/// Saturation matrix
/// </summary>
/// <seealso cref="MatrixBaseClass" />
public class Saturation : MatrixBaseClass
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Saturation"/> class.
    /// </summary>
    /// <param name="value">The saturation value (-1 to 1).</param>
    public Saturation(float value)
    {
        ++value;
        Value = 1f - value;
        var saturationComplementR = 0.3086f * Value;
        var saturationComplementG = 0.6094f * Value;
        var saturationComplementB = 0.0820f * Value;
        Matrix = new Matrix5X5
        (
            saturationComplementR + value, saturationComplementR, saturationComplementR, 0f, 0f,
            saturationComplementG, saturationComplementG + value, saturationComplementG, 0f, 0f,
            saturationComplementB, saturationComplementB, saturationComplementB + value, 0f, 0f,
            0f, 0f, 0f, 1f, 0f,
            0f, 0f, 0f, 0f, 1f
        );
    }

    /// <summary>
    /// Gets the matrix.
    /// </summary>
    /// <value>The matrix.</value>
    public override Matrix5X5 Matrix { get; } = new(
        1f, 0f, 0f, 0f, 0f,
        0f, 1f, 0f, 0f, 0f,
        0f, 0f, 1f, 0f, 0f,
        0f, 0f, 0f, 1f, 0f,
        0f, 0f, 0f, 0f, 1f
    );

    /// <summary>
    /// Gets the value.
    /// </summary>
    /// <value>The value.</value>
    public float Value { get; }
}