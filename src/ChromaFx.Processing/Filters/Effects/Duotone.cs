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
using ChromaFx.Core.Colors;
using ChromaFx.Processing.Numerics;

namespace ChromaFx.Processing.Filters.Effects;

/// <summary>
/// Maps BT.709 luminance onto a two-color gradient (shadow → highlight). Alpha is preserved.
/// </summary>
/// <param name="shadow">Color for dark tones.</param>
/// <param name="highlight">Color for light tones.</param>
public sealed class Duotone(Color shadow, Color highlight) : IFilter
{
    /// <summary>Gets the shadow color.</summary>
    public Color Shadow { get; } = shadow;

    /// <summary>Gets the highlight color.</summary>
    public Color Highlight { get; } = highlight;

    /// <inheritdoc />
    public Image Apply(Image image, Rectangle targetLocation = default)
    {
        targetLocation = targetLocation.Normalize(image);
        var shadow = Shadow;
        var highlight = Highlight;

        FilterParallelism.ForRows(
            targetLocation,
            y =>
            {
                var row = image.GetRowSpan(y);
                for (var x = targetLocation.Left; x < targetLocation.Right; x++)
                {
                    var p = row[x];
                    var t = (p.Red * 0.2126f + p.Green * 0.7152f + p.Blue * 0.0722f) / 255f;
                    row[x] = new Color(
                        Lerp(shadow.Red, highlight.Red, t),
                        Lerp(shadow.Green, highlight.Green, t),
                        Lerp(shadow.Blue, highlight.Blue, t),
                        p.Alpha
                    );
                }
            }
        );

        return image;
    }

    private static byte Lerp(byte a, byte b, float t) =>
        (byte)Math.Clamp((int)(a + (b - a) * t + 0.5f), 0, 255);
}
