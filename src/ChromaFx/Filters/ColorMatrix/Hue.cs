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
/// Hue matrix
/// </summary>
/// <seealso cref="MatrixBaseClass" />
public class Hue : MatrixBaseClass
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Alpha"/> class.
    /// </summary>
    /// <param name="value">The angle value (0 to 360).</param>
    public Hue(float value)
    {
        value *= (float)(Math.PI / 180f);
        Value = value;
        var cosRadians = Math.Cos(value);
        var sinRadians = Math.Sin(value);

        const float lumR = .213f;
        const float lumG = .715f;
        const float lumB = .072f;

        const float oneMinusLumR = 1f - lumR;
        const float oneMinusLumG = 1f - lumG;
        const float oneMinusLumB = 1f - lumB;
        Matrix = new Matrix5X5(
            (float)(lumR + cosRadians * oneMinusLumR - sinRadians * lumR), // M11
            (float)(lumR - cosRadians * lumR - sinRadians * 0.143),        // M12
            (float)(lumR - cosRadians * lumR - sinRadians * oneMinusLumR), // M13
            0, 0,                                                         // M14, M15
            (float)(lumG - cosRadians * lumG - sinRadians * lumG),         // M21
            (float)(lumG + cosRadians * oneMinusLumG + sinRadians * 0.140),// M22
            (float)(lumG - cosRadians * lumG + sinRadians * lumG),         // M23
            0, 0,                                                         // M24, M25
            (float)(lumB - cosRadians * lumB + sinRadians * oneMinusLumB), // M31
            (float)(lumB - cosRadians * lumB - sinRadians * 0.283),        // M32
            (float)(lumB + cosRadians * oneMinusLumB + sinRadians * lumB), // M33
            0, 0,                                                         // M34, M35
            0, 0, 0, 1, 0,                                                // M41, M42, M43, M44, M45
            0, 0, 0, 0, 1                                                 // M51, M52, M53, M54, M55
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