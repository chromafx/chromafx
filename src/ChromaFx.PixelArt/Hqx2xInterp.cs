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
 * Adapted from FFmpeg libavfilter/vf_hqx.c hq2x_interp_1x1
 * (Copyright 2014 Clément Bœsch), ISC license.
 */

using ChromaFx.Core.Colors;

namespace ChromaFx.PixelArt;

/// <summary>HQ2x single-output-pixel interpolator (one quadrant of the 2×2 block).</summary>
internal static class Hqx2xInterp
{
    public static Color Interp(
        int pattern,
        ReadOnlySpan<Color> w,
        ReadOnlySpan<uint> yuv,
        int p0,
        int p1,
        int p2,
        int p3,
        int p4,
        int p5,
        int p6,
        int p7,
        int p8
    )
    {
        var k = HqxHelpers.ShufflePattern(pattern, rotate: false, p0, p1, p2, p3, p5, p6, p7, p8);
        var w0 = w[p0];
        var w1 = w[p1];
        var w3 = w[p3];
        var w4 = w[p4];
        var w5 = w[p5];
        var w7 = w[p7];

        bool P(int m, int r) => HqxHelpers.PatternMatch(k, m, r);
        var diff15 = HqxHelpers.DiffYuv(w[p1], yuv[p1], w[p5], yuv[p5]);
        var diff73 = HqxHelpers.DiffYuv(w[p7], yuv[p7], w[p3], yuv[p3]);
        var diff31 = HqxHelpers.DiffYuv(w[p3], yuv[p3], w[p1], yuv[p1]);

        if ((P(0xbf, 0x37) || P(0xdb, 0x13)) && diff15)
            return HqxHelpers.Interp2(w4, 3, w3, 1, 2);
        if ((P(0xdb, 0x49) || P(0xef, 0x6d)) && diff73)
            return HqxHelpers.Interp2(w4, 3, w1, 1, 2);
        if ((P(0x0b, 0x0b) || P(0xfe, 0x4a) || P(0xfe, 0x1a)) && diff31)
            return w4;
        if (
            (
                P(0x6f, 0x2a)
                || P(0x5b, 0x0a)
                || P(0xbf, 0x3a)
                || P(0xdf, 0x5a)
                || P(0x9f, 0x8a)
                || P(0xcf, 0x8a)
                || P(0xef, 0x4e)
                || P(0x3f, 0x0e)
                || P(0xfb, 0x5a)
                || P(0xbb, 0x8a)
                || P(0x7f, 0x5a)
                || P(0xaf, 0x8a)
                || P(0xeb, 0x8a)
            ) && diff31
        )
            return HqxHelpers.Interp2(w4, 3, w0, 1, 2);
        if (P(0x0b, 0x08))
            return HqxHelpers.Interp3(w4, 2, w0, 1, w1, 1, 2);
        if (P(0x0b, 0x02))
            return HqxHelpers.Interp3(w4, 2, w0, 1, w3, 1, 2);
        if (P(0x2f, 0x2f))
            return HqxHelpers.Interp3(w4, 14, w3, 1, w1, 1, 4);
        if (P(0xbf, 0x37) || P(0xdb, 0x13))
            return HqxHelpers.Interp3(w4, 5, w1, 2, w3, 1, 3);
        if (P(0xdb, 0x49) || P(0xef, 0x6d))
            return HqxHelpers.Interp3(w4, 5, w3, 2, w1, 1, 3);
        if (P(0x1b, 0x03) || P(0x4f, 0x43) || P(0x8b, 0x83) || P(0x6b, 0x43))
            return HqxHelpers.Interp2(w4, 3, w3, 1, 2);
        if (P(0x4b, 0x09) || P(0x8b, 0x89) || P(0x1f, 0x19) || P(0x3b, 0x19))
            return HqxHelpers.Interp2(w4, 3, w1, 1, 2);
        if (P(0x7e, 0x2a) || P(0xef, 0xab) || P(0xbf, 0x8f) || P(0x7e, 0x0e))
            return HqxHelpers.Interp3(w4, 2, w3, 3, w1, 3, 3);
        if (
            P(0xfb, 0x6a)
            || P(0x6f, 0x6e)
            || P(0x3f, 0x3e)
            || P(0xfb, 0xfa)
            || P(0xdf, 0xde)
            || P(0xdf, 0x1e)
        )
            return HqxHelpers.Interp2(w4, 3, w0, 1, 2);
        if (
            P(0x0a, 0x00)
            || P(0x4f, 0x4b)
            || P(0x9f, 0x1b)
            || P(0x2f, 0x0b)
            || P(0xbe, 0x0a)
            || P(0xee, 0x0a)
            || P(0x7e, 0x0a)
            || P(0xeb, 0x4b)
            || P(0x3b, 0x1b)
        )
            return HqxHelpers.Interp3(w4, 2, w3, 1, w1, 1, 2);
        return HqxHelpers.Interp3(w4, 6, w3, 1, w1, 1, 3);
    }
}
