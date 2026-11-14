/*
 * Copyright 2026 Ho Tzin Mein
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
/// Shifts along the green↔magenta axis (companion to <see cref="KelvinTint"/>'s
/// blue↔orange / Kelvin axis). Positive values favor green; negative favor magenta.
/// </summary>
/// <seealso cref="MatrixBaseClass" />
public class Tint : MatrixBaseClass
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Tint"/> class.
    /// </summary>
    /// <param name="amount">Tint amount in −1…1 (positive = green, negative = magenta).</param>
    public Tint(float amount)
    {
        amount = Math.Clamp(amount, -1f, 1f);
        // Boost green while easing red/blue (or the reverse for magenta).
        var green = 1f + amount * 0.5f;
        var redBlue = 1f - amount * 0.25f;
        Matrix = new Matrix5X5(
            redBlue,
            0,
            0,
            0,
            0,
            0,
            green,
            0,
            0,
            0,
            0,
            0,
            redBlue,
            0,
            0,
            0,
            0,
            0,
            1,
            0,
            0,
            0,
            0,
            0,
            1
        );
    }

    /// <inheritdoc />
    public override Matrix5X5 Matrix { get; }
}
