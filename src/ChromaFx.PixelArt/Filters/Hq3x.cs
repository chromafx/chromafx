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
 * HQ3x adapted from FFmpeg libavfilter/vf_hqx.c (Copyright 2014 Clément Bœsch),
 * ISC license. Algorithm originally by Maxim Stepin.
 */

using ChromaFx.Core;
using ChromaFx.Core.Colors;
using ChromaFx.Processing.Filters;
using ChromaFx.Processing.Numerics;

namespace ChromaFx.PixelArt.Filters;

/// <summary>
/// HQ3x pixel-art upscaler (Maxim Stepin). Triples width and height; may blend
/// edge colors via YUV-thresholded pattern interpolation.
/// </summary>
/// <seealso cref="IFilter"/>
public class Hq3x : IFilter
{
    /// <summary>
    /// Applies HQ3x to the whole image (partial <paramref name="targetLocation"/> is ignored).
    /// </summary>
    public Image Apply(Image image, Rectangle targetLocation = default)
    {
        if (image.Width <= 0 || image.Height <= 0)
            return image;

        var output = Upscale(image);
        image.ReCreate(image.Width * 3, image.Height * 3, output);
        return image;
    }

    internal static Color[] Upscale(Image image)
    {
        var srcW = image.Width;
        var srcH = image.Height;
        var src = PixelArtSampler.CopyTight(image);
        var dstW = srcW * 3;
        var output = new Color[dstW * srcH * 3];
        Span<Color> n = stackalloc Color[9];
        Span<uint> yuv = stackalloc uint[9];

        for (var y = 0; y < srcH; ++y)
        {
            var row0 = y * 3 * dstW;
            var row1 = row0 + dstW;
            var row2 = row1 + dstW;

            for (var x = 0; x < srcW; ++x)
            {
                PixelArtSampler.Sample3x3(src, srcW, srcH, x, y, n);
                var pattern = HqxHelpers.BuildPattern(n, yuv);
                var ox = x * 3;

                Hqx3xInterp.Interp2x1(
                    out output[row0 + ox],
                    out output[row0 + ox + 1],
                    pattern,
                    n,
                    yuv,
                    0,
                    1,
                    2,
                    3,
                    4,
                    5,
                    6,
                    7,
                    8,
                    rotate: false
                );
                Hqx3xInterp.Interp2x1(
                    out output[row0 + ox + 2],
                    out output[row1 + ox + 2],
                    pattern,
                    n,
                    yuv,
                    2,
                    5,
                    8,
                    1,
                    4,
                    7,
                    0,
                    3,
                    6,
                    rotate: true
                );
                Hqx3xInterp.Interp2x1(
                    out output[row2 + ox],
                    out output[row1 + ox],
                    pattern,
                    n,
                    yuv,
                    6,
                    3,
                    0,
                    7,
                    4,
                    1,
                    8,
                    5,
                    2,
                    rotate: true
                );
                Hqx3xInterp.Interp2x1(
                    out output[row2 + ox + 2],
                    out output[row2 + ox + 1],
                    pattern,
                    n,
                    yuv,
                    8,
                    7,
                    6,
                    5,
                    4,
                    3,
                    2,
                    1,
                    0,
                    rotate: false
                );
                output[row1 + ox + 1] = n[4];
            }
        }

        return output;
    }
}
