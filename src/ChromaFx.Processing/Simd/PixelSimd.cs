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

using System.Numerics;
using System.Runtime.InteropServices;
using ChromaFx.Core.Colors;
using ChromaFx.Core.Simd;

namespace ChromaFx.Processing.Simd;

/// <summary>
/// SIMD helpers for packed <see cref="Color"/> pixel rows.
/// </summary>
internal static class PixelSimd
{
    /// <summary>Little-endian mask that inverts R,G,B and leaves A unchanged.</summary>
    private const uint InvertRgbMask = 0x00FFFFFFu;

    internal static Span<byte> AsBytes(Span<Color> pixels) => MemoryMarshal.AsBytes(pixels);

    internal static Span<uint> AsUInts(Span<Color> pixels) =>
        MemoryMarshal.Cast<Color, uint>(pixels);

    /// <summary>
    /// Inverts RGB channels in place; alpha is unchanged.
    /// </summary>
    internal static void InvertRgb(Span<Color> pixels)
    {
        Span<uint> data = AsUInts(pixels);
        int i = 0;
        int simdLength = Vector<uint>.Count;

        if (Vector.IsHardwareAccelerated)
        {
            var mask = new Vector<uint>(InvertRgbMask);
            for (; i <= data.Length - simdLength; i += simdLength)
            {
                var v = new Vector<uint>(data.Slice(i));
                (v ^ mask).CopyTo(data.Slice(i));
            }
        }

        for (; i < data.Length; i++)
            data[i] ^= InvertRgbMask;
    }

    /// <summary>
    /// Saturating per-channel add of <paramref name="source"/> into <paramref name="destination"/>.
    /// </summary>
    internal static void AddSaturating(Span<Color> destination, ReadOnlySpan<Color> source)
    {
        ByteSimd.AddSaturating(
            AsBytes(destination),
            MemoryMarshal.AsBytes(source),
            AsBytes(destination)
        );
    }

    /// <summary>
    /// Saturating per-channel subtract of <paramref name="source"/> from <paramref name="destination"/>.
    /// </summary>
    internal static void SubtractSaturating(Span<Color> destination, ReadOnlySpan<Color> source)
    {
        ByteSimd.SubtractSaturating(
            AsBytes(destination),
            MemoryMarshal.AsBytes(source),
            AsBytes(destination)
        );
    }

    internal static void And(Span<Color> destination, ReadOnlySpan<Color> source)
    {
        ByteSimd.AndInPlace(AsBytes(destination), MemoryMarshal.AsBytes(source));
    }

    internal static void Or(Span<Color> destination, ReadOnlySpan<Color> source)
    {
        ByteSimd.OrInPlace(AsBytes(destination), MemoryMarshal.AsBytes(source));
    }

    internal static void Xor(Span<Color> destination, ReadOnlySpan<Color> source)
    {
        ByteSimd.XorInPlace(AsBytes(destination), MemoryMarshal.AsBytes(source));
    }

    /// <summary>
    /// Applies a 256-entry LUT to R, G, and B; alpha is unchanged.
    /// Packed <see cref="uint"/> path with unrolled batches (LUT gathers are not portable SIMD).
    /// </summary>
    internal static void ApplyRgbLut(Span<Color> pixels, ReadOnlySpan<byte> lut)
    {
        if (lut.Length < 256)
            throw new ArgumentException("RGB LUT must contain 256 entries.", nameof(lut));

        Span<uint> data = AsUInts(pixels);
        int i = 0;
        for (; i <= data.Length - 4; i += 4)
        {
            data[i] = ApplyLutPacked(data[i], lut);
            data[i + 1] = ApplyLutPacked(data[i + 1], lut);
            data[i + 2] = ApplyLutPacked(data[i + 2], lut);
            data[i + 3] = ApplyLutPacked(data[i + 3], lut);
        }

        for (; i < data.Length; i++)
            data[i] = ApplyLutPacked(data[i], lut);
    }

    private static uint ApplyLutPacked(uint pixel, ReadOnlySpan<byte> lut)
    {
        var r = lut[(byte)pixel];
        var g = lut[(byte)(pixel >> 8)];
        var b = lut[(byte)(pixel >> 16)];
        return (pixel & 0xFF000000u) | ((uint)b << 16) | ((uint)g << 8) | r;
    }
}
