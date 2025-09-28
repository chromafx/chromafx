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

using ChromaFx.Processing.Filters.Resampling.ResamplingFilters.BaseClasses;
using ChromaFx.Processing.Filters.Resampling.ResamplingFilters.Interfaces;

namespace ChromaFx.Processing.Filters.Resampling.ResamplingFilters;

/// <summary>
/// Robidoux sharp filter
/// </summary>
/// <seealso cref="IResamplingFilter"/>
public class RobidouxSharpFilter : ResamplingFilterBase
{
    /// <summary>
    /// Gets the filter radius.
    /// </summary>
    /// <value>The filter radius.</value>
    public override float FilterRadius => 2f;

    /// <summary>
    /// Gets the value based on the resampling filter.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The new value based on the input.</returns>
    public override double GetValue(double value)
    {
        const float b = 0.26201451F;
        const float c = 0.36899274F;

        value = Math.Abs(value);
        var temp = value * value;
        return value switch
        {
            < 1 => ((12 - 9 * b - 6 * c) * (value * temp) + (-18 + 12 * b + 6 * c) * temp + (6 - 2 * b)) / 6,
            < 2 => ((-b - 6 * c) * (value * temp) + (6 * b + 30 * c) * temp + (-12 * b - 48 * c) * value +
                    (8 * b + 24 * c)) / 6,
            _ => 0
        };
    }
}