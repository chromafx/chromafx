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

using ChromaFx.Core;
using ChromaFx.Processing.Filters.Interfaces;
using ChromaFx.Processing.Numerics;
using ChromaFx.Processing.Simd;

namespace ChromaFx.Processing.Filters.Normalization;

/// <summary>
/// Adjusts gamma of an image
/// </summary>
/// <seealso cref="IFilter"/>
public class Gamma : IFilter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Gamma"/> class.
    /// </summary>
    /// <param name="value">The value.</param>
    public Gamma(float value)
    {
        Value = value;
        Lut = new byte[256];
        for (var x = 0; x < 256; x++)
        {
            var mapped = (int)(255.0 * Math.Pow(x / 255.0, 1.0 / Value) + 0.5);
            Lut[x] = (byte)Math.Clamp(mapped, 0, 255);
        }
    }

    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    public float Value { get; set; }

    private byte[] Lut { get; }

    /// <inheritdoc />
    public Image Apply(Image image, Rectangle targetLocation = default)
    {
        targetLocation = targetLocation.Normalize(image);

        int left = targetLocation.Left;
        int rowWidth = targetLocation.Right - left;
        var lut = Lut;

        FilterParallelism.ForRows(
            targetLocation,
            y =>
            {
                PixelSimd.ApplyRgbLut(image.GetRowSpan(y).Slice(left, rowWidth), lut);
            }
        );

        return image;
    }
}
