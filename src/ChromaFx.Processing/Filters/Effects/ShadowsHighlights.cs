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

namespace ChromaFx.Processing.Filters.Effects;

/// <summary>
/// Lifts shadows and/or compresses highlights via a luminance-weighted LUT. Amounts are −1…1
/// (positive shadows lighten darks; negative highlights darken brights). Alpha is preserved.
/// </summary>
public sealed class ShadowsHighlights : IFilter
{
    private readonly byte[] _lut = new byte[256];

    /// <summary>
    /// Initializes a new instance of the <see cref="ShadowsHighlights"/> class.
    /// </summary>
    /// <param name="shadows">Shadow lift (−1…1). Default 0.25.</param>
    /// <param name="highlights">Highlight compression (−1…1). Default −0.25.</param>
    public ShadowsHighlights(float shadows = 0.25f, float highlights = -0.25f)
    {
        Shadows = Math.Clamp(shadows, -1f, 1f);
        Highlights = Math.Clamp(highlights, -1f, 1f);
        for (var i = 0; i < 256; i++)
        {
            var t = i / 255f;
            var sw = (1f - t) * (1f - t);
            var hw = t * t;
            var delta = Shadows * sw + Highlights * hw;
            var o = Math.Clamp(t + delta, 0f, 1f);
            _lut[i] = (byte)(o * 255f + 0.5f);
        }
    }

    /// <summary>Gets the shadow amount.</summary>
    public float Shadows { get; }

    /// <summary>Gets the highlight amount.</summary>
    public float Highlights { get; }

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
