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
using ChromaFx.Processing.Filters.Interfaces;
using ChromaFx.Processing.Numerics;

namespace ChromaFx.PixelArt.Filters;

/// <summary>
/// Classic 2×SaI upscaler. May introduce blended colors along edges.
/// Clean-room implementation of the publicly described algorithm.
/// </summary>
/// <seealso cref="IFilter"/>
public class Scale2xSaI : IFilter
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
                var i = PixelArtSampler.At(image, x - 1, y - 1);
                var j = PixelArtSampler.At(image, x + 2, y - 1);
                var k = PixelArtSampler.At(image, x + 2, y);
                var l = PixelArtSampler.At(image, x + 2, y + 1);
                var m = PixelArtSampler.At(image, x - 1, y + 2);
                var n = PixelArtSampler.At(image, x, y + 2);
                var o = PixelArtSampler.At(image, x + 1, y + 2);

                // Top-left is always A; compute top-right, bottom-left, bottom-right.
                Color tr;
                Color bl;
                Color br;

                if (a == d && b != c)
                {
                    tr =
                        (a == e && b == l) || (a == c && a == f && b != e && b == j)
                            ? a
                            : PixelArtBlend.Interpolate(a, b);
                    bl =
                        (a == g && c == o) || (a == b && a == h && g != c && c == m)
                            ? a
                            : PixelArtBlend.Interpolate(a, c);
                    br = a;
                }
                else if (b == c && a != d)
                {
                    tr =
                        (b == f && a == h) || (b == e && b == d && a != f && a == i)
                            ? b
                            : PixelArtBlend.Interpolate(a, b);
                    bl =
                        (c == h && a == f) || (c == g && c == d && a != h && a == i)
                            ? c
                            : PixelArtBlend.Interpolate(a, c);
                    br = b;
                }
                else if (a == d && b == c)
                {
                    if (a == b)
                    {
                        tr = bl = br = a;
                    }
                    else
                    {
                        bl = PixelArtBlend.Interpolate(a, c);
                        tr = PixelArtBlend.Interpolate(a, b);
                        var r = 0;
                        r += SaIDecision.Score(a, b, g, e);
                        r += SaIDecision.ScoreOpposite(b, a, k, f);
                        r += SaIDecision.ScoreOpposite(b, a, h, n);
                        r += SaIDecision.Score(a, b, l, o);
                        br =
                            r > 0 ? a
                            : r < 0 ? b
                            : PixelArtBlend.QInterpolate(a, b, c, d);
                    }
                }
                else
                {
                    br = PixelArtBlend.QInterpolate(a, b, c, d);

                    if (a == c && a == f && b != e && b == j)
                        tr = a;
                    else if (b == e && b == d && a != f && a == i)
                        tr = b;
                    else
                        tr = PixelArtBlend.Interpolate(a, b);

                    if (a == b && a == h && g != c && c == m)
                        bl = a;
                    else if (c == g && c == d && a != h && a == i)
                        bl = c;
                    else
                        bl = PixelArtBlend.Interpolate(a, c);
                }

                var ox = x * 2;
                output[outY0 + ox] = a;
                output[outY0 + ox + 1] = tr;
                output[outY1 + ox] = bl;
                output[outY1 + ox + 1] = br;
            }
        }

        return output;
    }
}
