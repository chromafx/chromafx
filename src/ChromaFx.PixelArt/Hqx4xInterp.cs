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
 * Adapted from FFmpeg libavfilter/vf_hqx.c hq4x_interp_2x2
 * (Copyright 2014 Clément Bœsch), ISC license.
 */

using ChromaFx.Core.Colors;

namespace ChromaFx.PixelArt;

/// <summary>HQ4x 2×2 block interpolator (one quadrant of the 4×4 output).</summary>
internal static class Hqx4xInterp
{
    public static void Interp2x2(
        out Color dst00,
        out Color dst01,
        out Color dst10,
        out Color dst11,
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

        var cond00 = (P(0xbf, 0x37) || P(0xdb, 0x13)) && diff15;
        var cond01 = (P(0xdb, 0x49) || P(0xef, 0x6d)) && diff73;
        var cond02 =
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
            ) && diff31;
        var cond03 = P(0xdb, 0x49) || P(0xef, 0x6d);
        var cond04 = P(0xbf, 0x37) || P(0xdb, 0x13);
        var cond05 = P(0x1b, 0x03) || P(0x4f, 0x43) || P(0x8b, 0x83) || P(0x6b, 0x43);
        var cond06 = P(0x4b, 0x09) || P(0x8b, 0x89) || P(0x1f, 0x19) || P(0x3b, 0x19);
        var cond07 =
            P(0x0b, 0x08)
            || P(0xf9, 0x68)
            || P(0xf3, 0x62)
            || P(0x6d, 0x6c)
            || P(0x67, 0x66)
            || P(0x3d, 0x3c)
            || P(0x37, 0x36)
            || P(0xf9, 0xf8)
            || P(0xdd, 0xdc)
            || P(0xf3, 0xf2)
            || P(0xd7, 0xd6)
            || P(0xdd, 0x1c)
            || P(0xd7, 0x16)
            || P(0x0b, 0x02);
        var cond08 = (P(0x0f, 0x0b) || P(0x2b, 0x0b) || P(0xfe, 0x4a) || P(0xfe, 0x1a)) && diff31;
        var cond09 = P(0x2f, 0x2f);
        var cond10 = P(0x0a, 0x00);
        var cond11 = P(0x0b, 0x09);
        var cond12 = P(0x7e, 0x2a) || P(0xef, 0xab);
        var cond13 = P(0xbf, 0x8f) || P(0x7e, 0x0e);
        var cond14 =
            P(0x4f, 0x4b)
            || P(0x9f, 0x1b)
            || P(0x2f, 0x0b)
            || P(0xbe, 0x0a)
            || P(0xee, 0x0a)
            || P(0x7e, 0x0a)
            || P(0xeb, 0x4b)
            || P(0x3b, 0x1b);
        var cond15 = P(0x0b, 0x03);

        if (cond00)
            dst00 = HqxHelpers.Interp2(w4, 5, w3, 3, 3);
        else if (cond01)
            dst00 = HqxHelpers.Interp2(w4, 5, w1, 3, 3);
        else if ((P(0x0b, 0x0b) || P(0xfe, 0x4a) || P(0xfe, 0x1a)) && diff31)
            dst00 = w4;
        else if (cond02)
            dst00 = HqxHelpers.Interp2(w4, 5, w0, 3, 3);
        else if (cond03)
            dst00 = HqxHelpers.Interp2(w4, 3, w3, 1, 2);
        else if (cond04)
            dst00 = HqxHelpers.Interp2(w4, 3, w1, 1, 2);
        else if (cond05)
            dst00 = HqxHelpers.Interp2(w4, 5, w3, 3, 3);
        else if (cond06)
            dst00 = HqxHelpers.Interp2(w4, 5, w1, 3, 3);
        else if (
            P(0x0f, 0x0b)
            || P(0x5e, 0x0a)
            || P(0x2b, 0x0b)
            || P(0xbe, 0x0a)
            || P(0x7a, 0x0a)
            || P(0xee, 0x0a)
        )
            dst00 = HqxHelpers.Interp2(w1, 1, w3, 1, 1);
        else if (cond07)
            dst00 = HqxHelpers.Interp2(w4, 5, w0, 3, 3);
        else
            dst00 = HqxHelpers.Interp3(w4, 2, w1, 1, w3, 1, 2);

        if (cond00)
            dst01 = HqxHelpers.Interp2(w4, 7, w3, 1, 3);
        else if (cond08)
            dst01 = w4;
        else if (cond02)
            dst01 = HqxHelpers.Interp2(w4, 3, w0, 1, 2);
        else if (cond09)
            dst01 = w4;
        else if (cond10)
            dst01 = HqxHelpers.Interp3(w4, 5, w1, 2, w3, 1, 3);
        else if (P(0x0b, 0x08))
            dst01 = HqxHelpers.Interp3(w4, 5, w1, 2, w0, 1, 3);
        else if (cond11)
            dst01 = HqxHelpers.Interp2(w4, 5, w1, 3, 3);
        else if (cond04)
            dst01 = HqxHelpers.Interp2(w1, 3, w4, 1, 2);
        else if (cond12)
            dst01 = HqxHelpers.Interp3(w1, 2, w4, 1, w3, 1, 2);
        else if (cond13)
            dst01 = HqxHelpers.Interp2(w1, 5, w3, 3, 3);
        else if (cond05)
            dst01 = HqxHelpers.Interp2(w4, 7, w3, 1, 3);
        else if (
            P(0xf3, 0x62)
            || P(0x67, 0x66)
            || P(0x37, 0x36)
            || P(0xf3, 0xf2)
            || P(0xd7, 0xd6)
            || P(0xd7, 0x16)
            || P(0x0b, 0x02)
        )
            dst01 = HqxHelpers.Interp2(w4, 3, w0, 1, 2);
        else if (cond14)
            dst01 = HqxHelpers.Interp2(w1, 1, w4, 1, 1);
        else
            dst01 = HqxHelpers.Interp2(w4, 3, w1, 1, 2);

        if (cond01)
            dst10 = HqxHelpers.Interp2(w4, 7, w1, 1, 3);
        else if (cond08)
            dst10 = w4;
        else if (cond02)
            dst10 = HqxHelpers.Interp2(w4, 3, w0, 1, 2);
        else if (cond09)
            dst10 = w4;
        else if (cond10)
            dst10 = HqxHelpers.Interp3(w4, 5, w3, 2, w1, 1, 3);
        else if (P(0x0b, 0x02))
            dst10 = HqxHelpers.Interp3(w4, 5, w3, 2, w0, 1, 3);
        else if (cond15)
            dst10 = HqxHelpers.Interp2(w4, 5, w3, 3, 3);
        else if (cond03)
            dst10 = HqxHelpers.Interp2(w3, 3, w4, 1, 2);
        else if (cond13)
            dst10 = HqxHelpers.Interp3(w3, 2, w4, 1, w1, 1, 2);
        else if (cond12)
            dst10 = HqxHelpers.Interp2(w3, 5, w1, 3, 3);
        else if (cond06)
            dst10 = HqxHelpers.Interp2(w4, 7, w1, 1, 3);
        else if (
            P(0x0b, 0x08)
            || P(0xf9, 0x68)
            || P(0x6d, 0x6c)
            || P(0x3d, 0x3c)
            || P(0xf9, 0xf8)
            || P(0xdd, 0xdc)
            || P(0xdd, 0x1c)
        )
            dst10 = HqxHelpers.Interp2(w4, 3, w0, 1, 2);
        else if (cond14)
            dst10 = HqxHelpers.Interp2(w3, 1, w4, 1, 1);
        else
            dst10 = HqxHelpers.Interp2(w4, 3, w3, 1, 2);

        if ((P(0x7f, 0x2b) || P(0xef, 0xab) || P(0xbf, 0x8f) || P(0x7f, 0x0f)) && diff31)
            dst11 = w4;
        else if (cond02)
            dst11 = HqxHelpers.Interp2(w4, 7, w0, 1, 3);
        else if (cond15)
            dst11 = HqxHelpers.Interp2(w4, 7, w3, 1, 3);
        else if (cond11)
            dst11 = HqxHelpers.Interp2(w4, 7, w1, 1, 3);
        else if (P(0x0a, 0x00) || P(0x7e, 0x2a) || P(0xef, 0xab) || P(0xbf, 0x8f) || P(0x7e, 0x0e))
            dst11 = HqxHelpers.Interp3(w4, 6, w3, 1, w1, 1, 3);
        else if (cond07)
            dst11 = HqxHelpers.Interp2(w4, 7, w0, 1, 3);
        else
            dst11 = w4;
    }
}
