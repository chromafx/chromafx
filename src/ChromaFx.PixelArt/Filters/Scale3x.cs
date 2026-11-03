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
/// Classic Scale3x (EPX) pixel-art upscaler. Triples width and height using exact neighbor
/// equality; never blends.
/// </summary>
/// <seealso cref="IFilter"/>
public class Scale3x : IFilter
{
    /// <summary>
    /// Applies Scale3x to the whole image (partial <paramref name="targetLocation"/> is ignored).
    /// </summary>
    /// <param name="image">The image.</param>
    /// <param name="targetLocation">Ignored; the full image is scaled.</param>
    /// <returns>The image.</returns>
    public Image Apply(Image image, Rectangle targetLocation = default)
    {
        if (image.Width <= 0 || image.Height <= 0)
            return image;

        var output = Upscale(image);
        image.ReCreate(image.Width * 3, image.Height * 3, output);
        return image;
    }

    /// <summary>
    /// Produces a tightly packed 3× buffer for <paramref name="image"/> using the Scale3x rules.
    /// </summary>
    internal static Color[] Upscale(Image image)
    {
        var srcW = image.Width;
        var srcH = image.Height;
        var dstW = srcW * 3;
        var output = new Color[dstW * srcH * 3];

        for (var y = 0; y < srcH; ++y)
        {
            var row = image.GetRowSpan(y);
            var rowUp = y > 0 ? image.GetRowSpan(y - 1) : default;
            var rowDown = y + 1 < srcH ? image.GetRowSpan(y + 1) : default;
            var outY0 = y * 3 * dstW;
            var outY1 = outY0 + dstW;
            var outY2 = outY1 + dstW;

            for (var x = 0; x < srcW; ++x)
            {
                //   A B C
                //   D E F
                //   G H I
                var e = row[x];
                var a = y > 0 && x > 0 ? rowUp[x - 1] : e;
                var b = y > 0 ? rowUp[x] : e;
                var c = y > 0 && x + 1 < srcW ? rowUp[x + 1] : e;
                var d = x > 0 ? row[x - 1] : e;
                var f = x + 1 < srcW ? row[x + 1] : e;
                var g = y + 1 < srcH && x > 0 ? rowDown[x - 1] : e;
                var h = y + 1 < srcH ? rowDown[x] : e;
                var i = y + 1 < srcH && x + 1 < srcW ? rowDown[x + 1] : e;

                var e0 = d == b && d != h && b != f ? d : e;
                var e1 =
                    (d == b && d != h && b != f && e != c)
                    || (b == f && b != d && f != h && e != a)
                        ? b
                        : e;
                var e2 = b == f && b != d && f != h ? f : e;
                var e3 =
                    (d == b && d != h && b != f && e != g)
                    || (d == h && d != b && h != f && e != a)
                        ? d
                        : e;
                var e4 = e;
                var e5 =
                    (b == f && b != d && f != h && e != i)
                    || (h == f && h != d && f != b && e != c)
                        ? f
                        : e;
                var e6 = d == h && d != b && h != f ? d : e;
                var e7 =
                    (d == h && d != b && h != f && e != i)
                    || (h == f && h != d && f != b && e != g)
                        ? h
                        : e;
                var e8 = h == f && h != d && f != b ? f : e;

                var ox = x * 3;
                output[outY0 + ox] = e0;
                output[outY0 + ox + 1] = e1;
                output[outY0 + ox + 2] = e2;
                output[outY1 + ox] = e3;
                output[outY1 + ox + 1] = e4;
                output[outY1 + ox + 2] = e5;
                output[outY2 + ox] = e6;
                output[outY2 + ox + 1] = e7;
                output[outY2 + ox + 2] = e8;
            }
        }

        return output;
    }
}
