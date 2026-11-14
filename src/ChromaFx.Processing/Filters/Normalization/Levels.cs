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

using ChromaFx.Core;
using ChromaFx.Processing.Numerics;
using ChromaFx.Processing.Simd;

namespace ChromaFx.Processing.Filters.Normalization;

/// <summary>
/// Photoshop-style levels: remap input black/white through a midtone gamma into output black/white.
/// Alpha is preserved. Parameters are 0–1.
/// </summary>
public sealed class Levels : IFilter
{
    private readonly byte[] _lut = new byte[256];

    /// <summary>
    /// Initializes a new instance of the <see cref="Levels"/> class.
    /// </summary>
    /// <param name="inBlack">Input black point (0–1).</param>
    /// <param name="inWhite">Input white point (0–1).</param>
    /// <param name="gamma">Midtone gamma (&gt; 0). 1 = linear.</param>
    /// <param name="outBlack">Output black point (0–1).</param>
    /// <param name="outWhite">Output white point (0–1).</param>
    public Levels(
        float inBlack = 0f,
        float inWhite = 1f,
        float gamma = 1f,
        float outBlack = 0f,
        float outWhite = 1f
    )
    {
        InBlack = Math.Clamp(inBlack, 0f, 1f);
        InWhite = Math.Clamp(inWhite, 0f, 1f);
        if (InWhite <= InBlack)
            InWhite = Math.Min(1f, InBlack + 1f / 255f);
        Gamma = gamma <= 0f ? 1f : gamma;
        OutBlack = Math.Clamp(outBlack, 0f, 1f);
        OutWhite = Math.Clamp(outWhite, 0f, 1f);

        var invGamma = 1.0 / Gamma;
        var inRange = InWhite - InBlack;
        var outRange = OutWhite - OutBlack;

        for (var x = 0; x < 256; x++)
        {
            var v = x / 255f;
            var t = (v - InBlack) / inRange;
            t = Math.Clamp(t, 0f, 1f);
            t = (float)Math.Pow(t, invGamma);
            var o = OutBlack + t * outRange;
            _lut[x] = (byte)Math.Clamp((int)(o * 255f + 0.5f), 0, 255);
        }
    }

    /// <summary>Gets the input black point.</summary>
    public float InBlack { get; }

    /// <summary>Gets the input white point.</summary>
    public float InWhite { get; }

    /// <summary>Gets the midtone gamma.</summary>
    public float Gamma { get; }

    /// <summary>Gets the output black point.</summary>
    public float OutBlack { get; }

    /// <summary>Gets the output white point.</summary>
    public float OutWhite { get; }

    /// <inheritdoc />
    public Image Apply(Image image, Rectangle targetLocation = default)
    {
        targetLocation = targetLocation.Normalize(image);
        int left = targetLocation.Left;
        int rowWidth = targetLocation.Right - left;
        var lut = _lut;

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
