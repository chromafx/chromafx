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
using ChromaFx.Processing.Filters;
using ChromaFx.Processing.Numerics;

namespace ChromaFx.PixelArt.Filters;

/// <summary>
/// SuperEagle 2× upscaler (SaI family). May introduce blended colors along edges.
/// Clean-room implementation of the publicly described algorithm.
/// </summary>
/// <seealso cref="IFilter"/>
public class SuperEagle : IFilter
{
    /// <inheritdoc cref="Eagle.Apply"/>
    public Image Apply(Image image, Rectangle targetLocation = default)
    {
        if (image.Width <= 0 || image.Height <= 0)
            return image;

        var output = Upscale(image);
        image.ReCreate(image.Width * 2, image.Height * 2, output);
        return image;
    }

    /// <summary>
    /// 4×4 neighborhood (Wikipedia SaI matrix); writes top-left, top-right, bottom-left, bottom-right.
    /// </summary>
    internal static Color[] Upscale(Image image)
    {
        var srcW = image.Width;
        var srcH = image.Height;
        var dstW = srcW * 2;
        var output = new Color[dstW * srcH * 2];

        for (var y = 0; y < srcH; ++y)
        {
            var outY0 = y * 2 * dstW;
            var outY1 = outY0 + dstW;

            for (var x = 0; x < srcW; ++x)
            {
                // I E F J
                // G A B K
                // H C D L
                // M N O P
                var a = PixelArtSampler.At(image, x, y);
                var b = PixelArtSampler.At(image, x + 1, y);
                var c = PixelArtSampler.At(image, x, y + 1);
                var d = PixelArtSampler.At(image, x + 1, y + 1);
                var e = PixelArtSampler.At(image, x, y - 1);
                var f = PixelArtSampler.At(image, x + 1, y - 1);
                var g = PixelArtSampler.At(image, x - 1, y);
                var h = PixelArtSampler.At(image, x - 1, y + 1);
                var k = PixelArtSampler.At(image, x + 2, y);
                var l = PixelArtSampler.At(image, x + 2, y + 1);
                var n = PixelArtSampler.At(image, x, y + 2);
                var o = PixelArtSampler.At(image, x + 1, y + 2);

                Color tl;
                Color tr;
                Color bl;
                Color br;

                if (b == c && a != d)
                {
                    tr = bl = b;
                    tl =
                        h == b || b == f
                            ? PixelArtBlend.ThreeQuarter(b, a)
                            : PixelArtBlend.Interpolate(a, b);
                    br =
                        b == k || c == n
                            ? PixelArtBlend.ThreeQuarter(b, d)
                            : PixelArtBlend.Interpolate(c, d);
                }
                else if (a == d && b != c)
                {
                    br = tl = a;
                    tr =
                        e == a || d == l
                            ? PixelArtBlend.ThreeQuarter(a, b)
                            : PixelArtBlend.Interpolate(a, b);
                    bl =
                        d == o || g == a
                            ? PixelArtBlend.ThreeQuarter(a, c)
                            : PixelArtBlend.Interpolate(c, d);
                }
                else if (a == d && b == c)
                {
                    var r = 0;
                    r += SaIDecision.Score(b, a, h, n);
                    r += SaIDecision.Score(b, a, g, e);
                    r += SaIDecision.Score(b, a, o, l);
                    r += SaIDecision.Score(b, a, f, k);

                    if (r > 0)
                    {
                        tr = bl = b;
                        tl = br = PixelArtBlend.Interpolate(a, b);
                    }
                    else if (r < 0)
                    {
                        br = tl = a;
                        tr = bl = PixelArtBlend.Interpolate(a, b);
                    }
                    else
                    {
                        br = tl = a;
                        tr = bl = b;
                    }
                }
                else
                {
                    var bc = PixelArtBlend.Interpolate(c, b);
                    br = PixelArtBlend.QInterpolate(d, d, d, bc);
                    tl = PixelArtBlend.QInterpolate(a, a, a, bc);
                    var ad = PixelArtBlend.Interpolate(a, d);
                    bl = PixelArtBlend.QInterpolate(c, c, c, ad);
                    tr = PixelArtBlend.QInterpolate(b, b, b, ad);
                }

                var ox = x * 2;
                output[outY0 + ox] = tl;
                output[outY0 + ox + 1] = tr;
                output[outY1 + ox] = bl;
                output[outY1 + ox + 1] = br;
            }
        }

        return output;
    }
}
