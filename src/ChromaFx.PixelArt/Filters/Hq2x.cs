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
 * HQ2x adapted from FFmpeg libavfilter/vf_hqx.c (Copyright 2014 Clément Bœsch),
 * ISC license. Algorithm originally by Maxim Stepin.
 */

using ChromaFx.Core;
using ChromaFx.Core.Colors;
using ChromaFx.Processing.Filters.Interfaces;
using ChromaFx.Processing.Numerics;

namespace ChromaFx.PixelArt.Filters;

/// <summary>
/// HQ2x pixel-art upscaler (Maxim Stepin). Doubles width and height; may blend
/// edge colors via YUV-thresholded pattern interpolation.
/// </summary>
/// <seealso cref="IFilter"/>
public class Hq2x : IFilter
{
    /// <summary>
    /// Applies HQ2x to the whole image (partial <paramref name="targetLocation"/> is ignored).
    /// </summary>
    public Image Apply(Image image, Rectangle targetLocation = default)
    {
        if (image.Width <= 0 || image.Height <= 0)
            return image;

        var output = Upscale(image);
        image.ReCreate(image.Width * 2, image.Height * 2, output);
        return image;
    }

    internal static Color[] Upscale(Image image)
    {
        var srcW = image.Width;
        var srcH = image.Height;
        var src = PixelArtSampler.CopyTight(image);
        var dstW = srcW * 2;
        var output = new Color[dstW * srcH * 2];
        Span<Color> n = stackalloc Color[9];
        Span<uint> yuv = stackalloc uint[9];

        for (var y = 0; y < srcH; ++y)
        {
            var outY0 = y * 2 * dstW;
            var outY1 = outY0 + dstW;

            for (var x = 0; x < srcW; ++x)
            {
                PixelArtSampler.Sample3x3(src, srcW, srcH, x, y, n);
                var pattern = HqxHelpers.BuildPattern(n, yuv);
                var ox = x * 2;
                // 00, 01 (vert mirror), 10 (horiz mirror), 11 (center mirror)
                output[outY0 + ox] = Hqx2xInterp.Interp(pattern, n, yuv, 0, 1, 2, 3, 4, 5, 6, 7, 8);
                output[outY0 + ox + 1] = Hqx2xInterp.Interp(pattern, n, yuv, 2, 1, 0, 5, 4, 3, 8, 7, 6);
                output[outY1 + ox] = Hqx2xInterp.Interp(pattern, n, yuv, 6, 7, 8, 3, 4, 5, 0, 1, 2);
                output[outY1 + ox + 1] = Hqx2xInterp.Interp(pattern, n, yuv, 8, 7, 6, 5, 4, 3, 2, 1, 0);
            }
        }

        return output;
    }
}
