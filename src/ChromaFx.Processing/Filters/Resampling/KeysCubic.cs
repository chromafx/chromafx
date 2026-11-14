/*
 * Copyright 2017–2020 JaCraig
 * Modifications Copyright 2023–2026 Ho Tzin Mein
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

namespace ChromaFx.Processing.Filters.Resampling;

/// <summary>
/// Shared Keys BC-cubic kernel evaluation (Robidoux family uses float B/C; Mitchell uses double).
/// </summary>
internal static class KeysCubic
{
    /// <summary>
    /// Keys cubic with float B/C (Robidoux / RobidouxSharp / RobidouxSoft).
    /// Float intermediates match the historical per-filter expressions.
    /// </summary>
    internal static double Evaluate(double value, float b, float c)
    {
        value = Math.Abs(value);
        var temp = value * value;
        return value switch
        {
            < 1 => (
                (12 - 9 * b - 6 * c) * (value * temp) + (-18 + 12 * b + 6 * c) * temp + (6 - 2 * b)
            ) / 6,
            < 2 => (
                (-b - 6 * c) * (value * temp)
                + (6 * b + 30 * c) * temp
                + (-12 * b - 48 * c) * value
                + (8 * b + 24 * c)
            ) / 6,
            _ => 0,
        };
    }

    /// <summary>
    /// Keys cubic with double B/C (Mitchell B = C = 1/3).
    /// </summary>
    internal static double Evaluate(double value, double b, double c)
    {
        value = Math.Abs(value);
        var temp = value * value;
        switch (value)
        {
            case < 1:
                value =
                    (12 - 9 * b - 6 * c) * (value * temp)
                    + (-18 + 12 * b + 6 * c) * temp
                    + (6 - 2 * b);
                return value / 6;
            case < 2:
                value =
                    (-b - 6 * c) * (value * temp)
                    + (6 * b + 30 * c) * temp
                    + (-12 * b - 48 * c) * value
                    + (8 * b + 24 * c);
                return value / 6;
            default:
                return 0;
        }
    }
}
