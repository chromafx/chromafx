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

namespace ChromaFx.IO.Formats.Png.Format.Filters;

/// <summary>
/// Scanline filter base class
/// </summary>
/// <seealso cref="IScanFilter"/>
internal abstract class FilterBaseClass : IScanFilter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FilterBaseClass"/> class.
    /// </summary>
    protected FilterBaseClass() { }

    /// <summary>
    /// Gets the filter value.
    /// </summary>
    /// <value>The filter value.</value>
    public abstract byte FilterValue { get; }

    /// <summary>
    /// Decodes the specified scanline.
    /// </summary>
    /// <param name="scanline">The scanline.</param>
    /// <param name="previousScanline">The previous scanline.</param>
    /// <param name="scanlineStep">The scanline step.</param>
    /// <returns>The resulting byte array</returns>
    public virtual byte[] Decode(byte[] scanline, byte[] previousScanline, int scanlineStep)
    {
        var result = new byte[scanline.Length];
        for (var x = 0; x < result.Length; ++x)
        {
            var left = (x >= scanlineStep) ? result[x - scanlineStep] : (byte)0;
            var above = previousScanline[x];
            var upperLeft = (x >= scanlineStep) ? previousScanline[x - scanlineStep] : (byte)0;

            result[x] = (byte)(scanline[x] + Calculate(left, above, upperLeft));
        }
        return result;
    }

    /// <summary>
    /// Encodes the specified scanline.
    /// </summary>
    /// <param name="scanline">The scanline.</param>
    /// <param name="previousScanline">The previous scanline.</param>
    /// <param name="scanlineStep">The scanline step.</param>
    /// <returns>The resulting byte array</returns>
    public virtual byte[] Encode(byte[] scanline, byte[] previousScanline, int scanlineStep)
    {
        var result = new byte[scanline.Length + 1];
        result[0] = FilterValue;
        for (var x = 0; x < scanline.Length; ++x)
        {
            var left = (x >= scanlineStep) ? result[x - scanlineStep] : (byte)0;
            var above = previousScanline[x];
            var upperLeft = (x >= scanlineStep) ? previousScanline[x - scanlineStep] : (byte)0;

            result[x + 1] = (byte)(scanline[x] - Calculate(left, above, upperLeft));
        }

        return result;
    }

    /// <summary>
    /// Calculates the value to add based on the left, up, and upper left bytes.
    /// </summary>
    /// <param name="left">The left byte.</param>
    /// <param name="above">The above byte.</param>
    /// <param name="upperLeft">The upper left byte.</param>
    /// <returns>The resulting byte.</returns>
    protected abstract byte Calculate(byte left, byte above, byte upperLeft);
}
