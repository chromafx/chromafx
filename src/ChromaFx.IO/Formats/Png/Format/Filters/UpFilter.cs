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

using ChromaFx.Core.Simd;
using ChromaFx.IO.Formats.Png.Format.Enums;

namespace ChromaFx.IO.Formats.Png.Format.Filters;

/// <summary>
/// Adds the pixel above
/// </summary>
/// <seealso cref="FilterBaseClass"/>
internal class UpFilter : FilterBaseClass
{
    /// <summary>
    /// Gets the filter value.
    /// </summary>
    /// <value>The filter value.</value>
    public override byte FilterValue => (byte)FilterType.Up;

    /// <summary>
    /// Decodes the specified scanline using vectorized wrapping add with the previous row.
    /// </summary>
    public override byte[] Decode(byte[] scanline, byte[] previousScanline, int scanlineStep)
    {
        var result = new byte[scanline.Length];
        ByteSimd.AddWrapping(scanline, previousScanline.AsSpan(0, scanline.Length), result);
        return result;
    }

    /// <summary>
    /// Encodes the specified scanline using vectorized wrapping subtract of the previous row.
    /// </summary>
    public override byte[] Encode(byte[] scanline, byte[] previousScanline, int scanlineStep)
    {
        var result = new byte[scanline.Length + 1];
        result[0] = FilterValue;
        ByteSimd.SubtractWrapping(
            scanline,
            previousScanline.AsSpan(0, scanline.Length),
            result.AsSpan(1)
        );
        return result;
    }

    /// <summary>
    /// Calculates the value to add based on the left, up, and upper left bytes.
    /// </summary>
    /// <param name="left">The left byte.</param>
    /// <param name="above">The above byte.</param>
    /// <param name="upperLeft">The upper left byte.</param>
    /// <returns>The resulting byte.</returns>
    protected override byte Calculate(byte left, byte above, byte upperLeft)
    {
        return above;
    }
}
