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
/// Snaps each RGB channel to 6-bit VGA DAC levels (64 levels per channel,
/// multiples of 4, max 252). Alpha is preserved.
/// <see cref="VgaPaletteMode.Adaptive256"/> then reduces to 256 Mode 13h-style palette colors.
/// Optional ordered dither biases channels before the LUT snap (DacLevels) or before
/// nearest-palette remap (Adaptive256).
/// </summary>
/// <param name="mode">VGA palette mode; default is the full DAC cube.</param>
/// <param name="dither">Ordered dither method; default is none.</param>
/// <param name="strength">Dither strength (0 = none; ~0.25–0.5 typical for subtle VGA looks).</param>
public sealed class VgaQuantize(
    VgaPaletteMode mode = VgaPaletteMode.DacLevels,
    OrderedDitherMethod dither = OrderedDitherMethod.None,
    float strength = 0.5f
) : IFilter
{
    private const int AdaptiveColors = 256;

    private static readonly byte[] Lut = CreateLut();

    /// <summary>Gets the palette mode.</summary>
    public VgaPaletteMode Mode { get; } = mode;

    /// <summary>Gets the dither method.</summary>
    public OrderedDitherMethod Dither { get; } = dither;

    /// <summary>Gets the dither strength.</summary>
    public float Strength { get; } = strength;

    /// <inheritdoc />
    public Image Apply(Image image, Rectangle targetLocation = default)
    {
        if (Mode == VgaPaletteMode.DacLevels)
            return ChannelLutFilter.Apply(image, targetLocation, Lut, Dither, Strength);

        return ApplyAdaptive256(image, targetLocation);
    }

    /// <summary>Maps a single channel to the nearest 6-bit VGA DAC level (0..252 step 4).</summary>
    internal static byte SnapChannel(byte value) => Lut[value];

    private Image ApplyAdaptive256(Image image, Rectangle targetLocation)
    {
        // Snap onto the DAC lattice first (no dither) so octree only sees legal VGA levels.
        ChannelLutFilter.Apply(
            image,
            targetLocation,
            Lut,
            OrderedDitherMethod.None,
            0f
        );

        targetLocation = targetLocation.Normalize(image);
        var bits = Math.Max(1, Octree.BitsNeeded(AdaptiveColors));
        var octree = new Octree(bits);
        for (var y = targetLocation.Bottom; y < targetLocation.Top; y++)
        {
            var row = image.GetRowSpan(y);
            for (var x = targetLocation.Left; x < targetLocation.Right; x++)
            {
                if (row[x].Alpha == 0)
                    continue;
                octree.AddColor(row[x]);
            }
        }

        if (octree.Leaves == 0)
            return image;

        // Already within 256 unique DAC colors after snap.
        if (octree.Leaves <= AdaptiveColors)
            return image;

        var palette = octree.Palettize(Math.Min(AdaptiveColors, octree.Leaves));
        if (palette.Count == 0)
            return image;

        // Palette registers are 6-bit — keep entries on the DAC lattice.
        for (var i = 0; i < palette.Count; i++)
        {
            var c = palette[i];
            palette[i] = new Color(Lut[c.Red], Lut[c.Green], Lut[c.Blue], c.Alpha);
        }

        return ApplyRemap(image, targetLocation, octree, palette);
    }

    private Image ApplyRemap(Image image, Rectangle target, Octree octree, List<Color> palette)
    {
        var method = Dither;
        var amount = Strength;
        var useDither = method != OrderedDitherMethod.None && amount != 0f;
        var biases = useDither
            ? OrderedDither.BuildBiasTable(method, amount, OrderedDither.RetroBiasScale, round: true)
            : [];
        var side = method == OrderedDitherMethod.Bayer8x8 ? 8 : 4;
        var mask = side - 1;

        FilterParallelism.ForRows(
            target,
            y =>
            {
                var row = image.GetRowSpan(y);
                for (var x = target.Left; x < target.Right; ++x)
                {
                    ref var pixel = ref row[x];
                    Color sample;
                    if (useDither)
                    {
                        var bias = biases[(y & mask) * side + (x & mask)];
                        sample = new Color(
                            (byte)Math.Clamp(pixel.Red + bias, 0, 255),
                            (byte)Math.Clamp(pixel.Green + bias, 0, 255),
                            (byte)Math.Clamp(pixel.Blue + bias, 0, 255),
                            pixel.Alpha
                        );
                    }
                    else
                    {
                        sample = pixel;
                    }

                    var mapped = Map(octree, palette, sample);
                    pixel.Red = mapped.Red;
                    pixel.Green = mapped.Green;
                    pixel.Blue = mapped.Blue;
                }
            }
        );

        return image;
    }

    private static Color Map(Octree octree, List<Color> palette, Color sample)
    {
        if (octree.TryGetPaletteIndex(sample, out var idx))
        {
            idx = Math.Clamp(idx, 0, palette.Count - 1);
            var c = palette[idx];
            return new Color(c.Red, c.Green, c.Blue, sample.Alpha);
        }

        var nearest = Palette.NearestRgb(palette, sample.Red, sample.Green, sample.Blue);
        return new Color(nearest.Red, nearest.Green, nearest.Blue, sample.Alpha);
    }

    private static byte[] CreateLut()
    {
        var lut = new byte[256];
        for (var i = 0; i < 256; ++i)
            lut[i] = (byte)(Math.Min(252, (i + 2) / 4 * 4));
        return lut;
    }
}
