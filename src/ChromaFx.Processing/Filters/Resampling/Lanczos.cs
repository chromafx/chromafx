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
/// Shared Lanczos kernel evaluation.
/// </summary>
internal static class Lanczos
{
    /// <summary>
    /// Lanczos weight for absolute distance <paramref name="value"/> and lobe radius <paramref name="radius"/>.
    /// </summary>
    internal static double GetValue(double value, float radius)
    {
        value = Math.Abs(value);
        if (value < radius)
            return Sin(value) * Sin(value / radius);
        return 0;
    }

    private static double Sin(double value)
    {
        if (value == 0)
            return 1;
        value *= Math.PI;
        return Math.Sin(value) / value;
    }
}
