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

using System.Threading;
using ChromaFx.Core;
using ChromaFx.Core.Quantization;
using ChromaFx.Processing;
using ChromaFx.Processing.Filters;
using ChromaFx.Processing.Numerics;

namespace ChromaFx.Retro.Filters;

/// <summary>
/// Snaps each RGB channel to the 4-level EGA palette
/// (<c>00</c>, <c>55</c>, <c>AA</c>, <c>FF</c> — 64 colors total). Alpha is preserved.
/// <see cref="EgaPaletteMode.Adaptive16"/> keeps the 16 most frequent cube colors and remaps the rest.
/// Optional ordered dither biases channels before the LUT snap.
/// </summary>
/// <param name="mode">EGA palette mode; default is the full 64-color cube.</param>
/// <param name="dither">Ordered dither method; default is none.</param>
/// <param name="strength">Dither strength (0 = none; ~0.3–0.6 typical).</param>
public sealed class EgaQuantize(
    EgaPaletteMode mode = EgaPaletteMode.Full64,
    OrderedDitherMethod dither = OrderedDitherMethod.None,
    float strength = 0.5f
) : IFilter
{
    private const int CubeSize = 64;
    private const int AdaptiveColors = 16;

    private static readonly byte[] Lut = CreateLut();

    /// <summary>Gets the palette mode.</summary>
    public EgaPaletteMode Mode { get; } = mode;

    /// <summary>Gets the dither method.</summary>
    public OrderedDitherMethod Dither { get; } = dither;

    /// <summary>Gets the dither strength.</summary>
    public float Strength { get; } = strength;

    /// <inheritdoc />
    public Image Apply(Image image, Rectangle targetLocation = default)
    {
        if (Mode == EgaPaletteMode.Full64)
            return ChannelLutFilter.Apply(image, targetLocation, Lut, Dither, Strength);

        return ApplyAdaptive16(image, targetLocation);
    }

    /// <summary>Maps a single channel to the nearest EGA level.</summary>
    internal static byte SnapChannel(byte value) => Lut[value];

    /// <summary>Packs snapped EGA RGB into a 0..63 cube index.</summary>
    internal static int PackIndex(byte r, byte g, byte b) =>
        ((r / 85) << 4) | ((g / 85) << 2) | (b / 85);

    /// <summary>Unpacks a 0..63 cube index to RGB levels.</summary>
    internal static void UnpackIndex(int index, out byte r, out byte g, out byte b)
    {
        r = (byte)(((index >> 4) & 3) * 85);
        g = (byte)(((index >> 2) & 3) * 85);
        b = (byte)((index & 3) * 85);
    }

    private Image ApplyAdaptive16(Image image, Rectangle targetLocation)
    {
        targetLocation = targetLocation.Normalize(image);
        var counts = new int[CubeSize];

        FilterParallelism.ForRows(
            targetLocation,
            y =>
            {
                var row = image.GetRowSpan(y);
                for (var x = targetLocation.Left; x < targetLocation.Right; ++x)
                {
                    ref readonly var pixel = ref row[x];
                    var idx = PackIndex(Lut[pixel.Red], Lut[pixel.Green], Lut[pixel.Blue]);
                    Interlocked.Increment(ref counts[idx]);
                }
            }
        );

        Span<int> used = stackalloc int[CubeSize];
        var usedCount = 0;
        for (var i = 0; i < CubeSize; ++i)
        {
            if (counts[i] > 0)
                used[usedCount++] = i;
        }

        // Already within 16 unique cube colors: Full64 snap is enough.
        if (usedCount <= AdaptiveColors)
            return ChannelLutFilter.Apply(image, targetLocation, Lut, Dither, Strength);

        // Sort used indices by count desc, then index asc (deterministic ties).
        for (var i = 1; i < usedCount; ++i)
        {
            var key = used[i];
            var keyCount = counts[key];
            var j = i - 1;
            while (
                j >= 0
                && (
                    counts[used[j]] < keyCount
                    || (counts[used[j]] == keyCount && used[j] > key)
                )
            )
            {
                used[j + 1] = used[j];
                --j;
            }

            used[j + 1] = key;
        }

        Span<int> palette = used[..AdaptiveColors];
        var remapR = new byte[CubeSize];
        var remapG = new byte[CubeSize];
        var remapB = new byte[CubeSize];
        BuildRemap(palette, remapR, remapG, remapB);

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
                    byte sr;
                    byte sg;
                    byte sb;
                    if (useDither)
                    {
                        var bias = biases[(y & mask) * side + (x & mask)];
                        sr = Lut[(byte)Math.Clamp(pixel.Red + bias, 0, 255)];
                        sg = Lut[(byte)Math.Clamp(pixel.Green + bias, 0, 255)];
                        sb = Lut[(byte)Math.Clamp(pixel.Blue + bias, 0, 255)];
                    }
                    else
                    {
                        sr = Lut[pixel.Red];
                        sg = Lut[pixel.Green];
                        sb = Lut[pixel.Blue];
                    }

                    var idx = PackIndex(sr, sg, sb);
                    pixel.Red = remapR[idx];
                    pixel.Green = remapG[idx];
                    pixel.Blue = remapB[idx];
                }
            }
        );

        return image;
    }

    private static void BuildRemap(
        ReadOnlySpan<int> palette,
        byte[] remapR,
        byte[] remapG,
        byte[] remapB
    )
    {
        Span<(byte R, byte G, byte B)> colors = stackalloc (byte, byte, byte)[palette.Length];
        for (var i = 0; i < palette.Length; ++i)
        {
            UnpackIndex(palette[i], out var r, out var g, out var b);
            colors[i] = (r, g, b);
        }

        for (var idx = 0; idx < CubeSize; ++idx)
        {
            UnpackIndex(idx, out var r, out var g, out var b);
            var best = 0;
            var bestDist = int.MaxValue;
            for (var i = 0; i < colors.Length; ++i)
            {
                var c = colors[i];
                var dr = r - c.R;
                var dg = g - c.G;
                var db = b - c.B;
                var d = dr * dr + dg * dg + db * db;
                if (d < bestDist)
                {
                    bestDist = d;
                    best = i;
                }
            }

            var chosen = colors[best];
            remapR[idx] = chosen.R;
            remapG[idx] = chosen.G;
            remapB[idx] = chosen.B;
        }
    }

    private static byte[] CreateLut()
    {
        var lut = new byte[256];
        for (var i = 0; i < 256; ++i)
            lut[i] = (byte)(((i + 42) / 85) * 85);
        return lut;
    }
}
