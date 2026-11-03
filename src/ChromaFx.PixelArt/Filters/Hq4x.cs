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
 * HQ4x adapted from FFmpeg libavfilter/vf_hqx.c (Copyright 2014 Clément Bœsch),
 * ISC license. Algorithm originally by Maxim Stepin.
 */

using ChromaFx.Core;
using ChromaFx.Core.Colors;
using ChromaFx.Processing.Filters.Interfaces;
using ChromaFx.Processing.Numerics;

namespace ChromaFx.PixelArt.Filters;

/// <summary>
/// HQ4x pixel-art upscaler (Maxim Stepin). Quadruples width and height; may blend
/// edge colors via YUV-thresholded pattern interpolation.
/// </summary>
/// <seealso cref="IFilter"/>
public class Hq4x : IFilter
{
    /// <summary>
    /// Applies HQ4x to the whole image (partial <paramref name="targetLocation"/> is ignored).
    /// </summary>
    public Image Apply(Image image, Rectangle targetLocation = default)
    {
        if (image.Width <= 0 || image.Height <= 0)
            return image;

        var output = Upscale(image);
        image.ReCreate(image.Width * 4, image.Height * 4, output);
        return image;
    }

    internal static Color[] Upscale(Image image)
    {
        var srcW = image.Width;
        var srcH = image.Height;
        var src = PixelArtSampler.CopyTight(image);
        var dstW = srcW * 4;
        var output = new Color[dstW * srcH * 4];
        Span<Color> n = stackalloc Color[9];
        Span<uint> yuv = stackalloc uint[9];

        for (var y = 0; y < srcH; ++y)
        {
            var row0 = y * 4 * dstW;
            var row1 = row0 + dstW;
            var row2 = row1 + dstW;
            var row3 = row2 + dstW;

            for (var x = 0; x < srcW; ++x)
            {
                PixelArtSampler.Sample3x3(src, srcW, srcH, x, y, n);
                var pattern = HqxHelpers.BuildPattern(n, yuv);
                var ox = x * 4;

                WriteQuadrant(output, row0, row1, ox, pattern, n, yuv, 0, 1, 2, 3, 0, 1, 2, 3, 4, 5, 6, 7, 8);
                WriteQuadrant(output, row0, row1, ox + 2, pattern, n, yuv, 1, 0, 3, 2, 2, 1, 0, 5, 4, 3, 8, 7, 6);
                WriteQuadrant(output, row2, row3, ox, pattern, n, yuv, 2, 3, 0, 1, 6, 7, 8, 3, 4, 5, 0, 1, 2);
                WriteQuadrant(output, row2, row3, ox + 2, pattern, n, yuv, 3, 2, 1, 0, 8, 7, 6, 5, 4, 3, 2, 1, 0);
            }
        }

        return output;
    }

    private static void WriteQuadrant(
        Color[] output,
        int rowA,
        int rowB,
        int ox,
        int pattern,
        ReadOnlySpan<Color> n,
        ReadOnlySpan<uint> yuv,
        int pos00,
        int pos01,
        int pos10,
        int pos11,
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
        Hqx4xInterp.Interp2x2(
            out var c00,
            out var c01,
            out var c10,
            out var c11,
            pattern,
            n,
            yuv,
            p0,
            p1,
            p2,
            p3,
            p4,
            p5,
            p6,
            p7,
            p8
        );

        SetPos(output, rowA, rowB, ox, pos00, c00);
        SetPos(output, rowA, rowB, ox, pos01, c01);
        SetPos(output, rowA, rowB, ox, pos10, c10);
        SetPos(output, rowA, rowB, ox, pos11, c11);
    }

    private static void SetPos(Color[] output, int rowA, int rowB, int ox, int pos, Color c)
    {
        var row = (pos & 2) != 0 ? rowB : rowA;
        output[row + ox + (pos & 1)] = c;
    }
}
