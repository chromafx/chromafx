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

using System.Numerics;

namespace ChromaFx.ExtensionMethods;

/// <summary>
/// Float extensions
/// </summary>
public static class Extensions
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
        return value < min ? min : value > max ? max : value;
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

    /// <summary>
    /// Converts a float array to a vector4 array.
    /// </summary>
    /// <param name="values">The values.</param>
    /// <returns>The resulting array after converting</returns>
    public static Vector4[] ToVector4(this float[] values)
    {
        if (values == null || values.Length == 0)
        {
            return [];
        }
        var newData = new List<Vector4>();
        for (var x = 0; x < values.Length; x += 4)
        {
            newData.Add(new Vector4(values[x], values[x + 1], values[x + 2], values[x + 3]));
        }
        return newData.ToArray();
    }
}