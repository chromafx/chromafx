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

namespace ChromaFx.Core;

/// <summary>
/// Float extensions
/// </summary>
public static class FloatExtensions
{
    /// <summary>
    /// Clamps the value based on the minimum and maximum specified.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="min">The minimum.</param>
    /// <param name="max">The maximum.</param>
    /// <returns>The clamped value.</returns>
    public static float Clamp(this float value, float min, float max)
    {
        return value < min ? min
            : value > max ? max
            : value;
    }

    /// <summary>
    /// Clamps the value based on the minimum and maximum specified.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="min">The minimum.</param>
    /// <param name="max">The maximum.</param>
    /// <returns>The clamped value.</returns>
    public static int Clamp(this int value, int min, int max)
    {
        return Math.Clamp(value, min, max);
    }

    /// <summary>
    /// Gets the cubic root of a value
    /// </summary>
    /// <param name="n">The value</param>
    /// <returns>The result</returns>
    public static double CubicRoot(this double n)
    {
        return Math.Pow(n, 1.0 / 3.0);
    }

}
