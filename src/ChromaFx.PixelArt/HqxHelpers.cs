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
 *
 * HQX pattern / YUV / blend helpers are adapted from FFmpeg libavfilter/vf_hqx.c
 * (Copyright 2014 Clément Bœsch), licensed under the ISC license.
 */

using System.Runtime.CompilerServices;
using ChromaFx.Core.Colors;

namespace ChromaFx.PixelArt;

/// <summary>
/// Shared HQX (hq2x / hq3x / hq4x) helpers: YUV difference, weighted blends, pattern shuffle.
/// </summary>
internal static class HqxHelpers
{
    /// <summary>Packed YUV (Y≪16 | U≪8 | V) matching FFmpeg hq× thresholds.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ToYuv(Color c)
    {
        int r = c.Red;
        int g = c.Green;
        int b = c.Blue;
        var rg = r - g;
        var bg = b - g;
        var y = (uint)((299 * rg + 1000 * g + 114 * bg) / 1000);
        var u = (uint)((-169 * rg + 500 * bg) / 1000 + 128);
        var v = (uint)((500 * rg - 81 * bg) / 1000 + 128);
        return (y << 16) | (u << 8) | v;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool YuvDiff(uint yuv1, uint yuv2) =>
        AbsDiff(yuv1 & 0xff0000, yuv2 & 0xff0000) > (48u << 16)
        || AbsDiff(yuv1 & 0x00ff00, yuv2 & 0x00ff00) > (7u << 8)
        || AbsDiff(yuv1 & 0x0000ff, yuv2 & 0x0000ff) > 6u;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Diff(Color a, Color b) =>
        a != b && YuvDiff(ToYuv(a), ToYuv(b));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool DiffYuv(Color a, uint yuvA, Color b, uint yuvB) =>
        a != b && YuvDiff(yuvA, yuvB);

    /// <summary>(c1·w1 + c2·w2) ≫ s</summary>
    public static Color Interp2(Color c1, int w1, Color c2, int w2, int s) =>
        new(
            (byte)((c1.Red * w1 + c2.Red * w2) >> s),
            (byte)((c1.Green * w1 + c2.Green * w2) >> s),
            (byte)((c1.Blue * w1 + c2.Blue * w2) >> s),
            (byte)((c1.Alpha * w1 + c2.Alpha * w2) >> s)
        );

    /// <summary>(c1·w1 + c2·w2 + c3·w3) ≫ s</summary>
    public static Color Interp3(Color c1, int w1, Color c2, int w2, Color c3, int w3, int s) =>
        new(
            (byte)((c1.Red * w1 + c2.Red * w2 + c3.Red * w3) >> s),
            (byte)((c1.Green * w1 + c2.Green * w2 + c3.Green * w3) >> s),
            (byte)((c1.Blue * w1 + c2.Blue * w2 + c3.Blue * w3) >> s),
            (byte)((c1.Alpha * w1 + c2.Alpha * w2 + c3.Alpha * w3) >> s)
        );

    /// <summary>
    /// Builds the 8-bit neighbor-diff pattern for center <paramref name="w"/>[4]
    /// (bits 0..7 → neighbors 0,1,2,3,5,6,7,8) and fills <paramref name="yuv"/>[0..8]
    /// (skipped equals still write the center YUV for reuse in <see cref="DiffYuv"/>).
    /// </summary>
    public static int BuildPattern(ReadOnlySpan<Color> w, Span<uint> yuv)
    {
        var c = w[4];
        var yuv1 = ToYuv(c);
        yuv[4] = yuv1;
        var pattern = 0;
        pattern |= PatternBit(c, yuv1, w[0], ref yuv[0], 0);
        pattern |= PatternBit(c, yuv1, w[1], ref yuv[1], 1);
        pattern |= PatternBit(c, yuv1, w[2], ref yuv[2], 2);
        pattern |= PatternBit(c, yuv1, w[3], ref yuv[3], 3);
        pattern |= PatternBit(c, yuv1, w[5], ref yuv[5], 4);
        pattern |= PatternBit(c, yuv1, w[6], ref yuv[6], 5);
        pattern |= PatternBit(c, yuv1, w[7], ref yuv[7], 6);
        pattern |= PatternBit(c, yuv1, w[8], ref yuv[8], 7);
        return pattern;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int PatternBit(Color center, uint yuvCenter, Color n, ref uint yuvN, int bit)
    {
        if (center == n)
        {
            yuvN = yuvCenter;
            return 0;
        }

        yuvN = ToYuv(n);
        return YuvDiff(yuvCenter, yuvN) ? 1 << bit : 0;
    }

    /// <summary>
    /// Remaps pattern bits for a p0..p8 neighborhood permutation (FFmpeg SHF / DROP4).
    /// </summary>
    public static int ShufflePattern(
        int k,
        bool rotate,
        int p0,
        int p1,
        int p2,
        int p3,
        int p5,
        int p6,
        int p7,
        int p8
    ) =>
        ShuffleBit(k, rotate, 0, p0)
        | ShuffleBit(k, rotate, 1, p1)
        | ShuffleBit(k, rotate, 2, p2)
        | ShuffleBit(k, rotate, 3, p3)
        | ShuffleBit(k, rotate, 5, p5)
        | ShuffleBit(k, rotate, 6, p6)
        | ShuffleBit(k, rotate, 7, p7)
        | ShuffleBit(k, rotate, 8, p8);

    public static bool PatternMatch(int kShuffled, int mask, int expected) =>
        (kShuffled & mask) == expected;

    private static int Drop4(int z) => z > 4 ? z - 1 : z;

    private static int ShuffleBit(int k, bool rotate, int n, int pn)
    {
        var from = rotate ? 7 - Drop4(n) : Drop4(n);
        return ((k >> from) & 1) << Drop4(pn);
    }

    private static uint AbsDiff(uint a, uint b) => a > b ? a - b : b - a;
}
