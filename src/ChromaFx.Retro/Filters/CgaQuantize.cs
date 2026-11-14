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
using ChromaFx.Core.Quantization;
using ChromaFx.Processing;
using ChromaFx.Processing.Filters;
using ChromaFx.Processing.Numerics;

namespace ChromaFx.Retro.Filters;

/// <summary>
/// Quantizes pixels to specific historical CGA palette modes using nearest-color matching.
/// Optional ordered dither biases RGB before nearest-color.
/// </summary>
/// <param name="mode">CGA palette mode.</param>
/// <param name="dither">Ordered dither method; default is none.</param>
/// <param name="strength">Dither strength (0 = none; ~0.3–0.6 typical).</param>
public sealed class CgaQuantize(
    CgaPaletteMode mode = CgaPaletteMode.Palette1High,
    OrderedDitherMethod dither = OrderedDitherMethod.None,
    float strength = 0.5f
) : IFilter
{
    private readonly Color[] _palette = mode switch
    {
        CgaPaletteMode.Standard16 =>
        [
            new Color(0, 0, 0),
            new Color(0, 0, 170),
            new Color(0, 170, 0),
            new Color(0, 170, 170),
            new Color(170, 0, 0),
            new Color(170, 0, 170),
            new Color(170, 85, 0),
            new Color(170, 170, 170),
            new Color(85, 85, 85),
            new Color(85, 85, 255),
            new Color(85, 255, 85),
            new Color(85, 255, 255),
            new Color(255, 85, 85),
            new Color(255, 85, 255),
            new Color(255, 255, 85),
            new Color(255, 255, 255),
        ],
        CgaPaletteMode.Palette1Low =>
        [
            new Color(0, 0, 0),
            new Color(0, 170, 170),
            new Color(170, 0, 170),
            new Color(170, 170, 170),
        ],
        CgaPaletteMode.Palette1High =>
        [
            new Color(0, 0, 0),
            new Color(85, 255, 255),
            new Color(255, 85, 255),
            new Color(255, 255, 255),
        ],
        CgaPaletteMode.Palette2Low =>
        [
            new Color(0, 0, 0),
            new Color(0, 170, 0),
            new Color(170, 0, 0),
            new Color(170, 85, 0),
        ],
        CgaPaletteMode.Palette2High =>
        [
            new Color(0, 0, 0),
            new Color(85, 255, 85),
            new Color(255, 85, 85),
            new Color(255, 255, 85),
        ],
        CgaPaletteMode.Monochrome2 => [new Color(0, 0, 0), new Color(255, 255, 255)],
        _ => throw new ArgumentOutOfRangeException(nameof(mode)),
    };

    /// <summary>Gets the palette mode.</summary>
    public CgaPaletteMode Mode { get; } = mode;

    /// <summary>Gets the dither method.</summary>
    public OrderedDitherMethod Dither { get; } = dither;

    /// <summary>Gets the dither strength.</summary>
    public float Strength { get; } = strength;

    /// <inheritdoc />
    public Image Apply(Image image, Rectangle targetLocation = default)
    {
        targetLocation = targetLocation.Normalize(image);
        var palette = _palette;
        var method = Dither;
        var amount = Strength;
        var useDither = method != OrderedDitherMethod.None && amount != 0f;
        var biases = useDither
            ? OrderedDither.BuildBiasTable(method, amount, OrderedDither.RetroBiasScale, round: true)
            : [];
        var side = method == OrderedDitherMethod.Bayer8x8 ? 8 : 4;
        var mask = side - 1;

        FilterParallelism.ForRows(
            targetLocation,
            y =>
            {
                var row = image.GetRowSpan(y);
                for (var x = targetLocation.Left; x < targetLocation.Right; ++x)
                {
                    ref var pixel = ref row[x];
                    int r = pixel.Red;
                    int g = pixel.Green;
                    int b = pixel.Blue;
                    if (useDither)
                    {
                        var bias = biases[(y & mask) * side + (x & mask)];
                        r = Math.Clamp(pixel.Red + bias, 0, 255);
                        g = Math.Clamp(pixel.Green + bias, 0, 255);
                        b = Math.Clamp(pixel.Blue + bias, 0, 255);
                    }

                    var nearest = Palette.NearestRgb(palette, r, g, b);
                    pixel.Red = nearest.Red;
                    pixel.Green = nearest.Green;
                    pixel.Blue = nearest.Blue;
                }
            }
        );

        return image;
    }
}
