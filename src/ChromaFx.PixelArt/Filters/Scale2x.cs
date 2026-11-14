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
/// Classic Scale2x (EPX) pixel-art upscaler. Doubles width and height using exact neighbor equality; never blends.
/// </summary>
/// <seealso cref="IFilter"/>
public class Scale2x : IFilter
{
    /// <summary>
    /// Applies Scale2x to the whole image (partial <paramref name="targetLocation"/> is ignored).
    /// </summary>
    /// <param name="image">The image.</param>
    /// <param name="targetLocation">Ignored; the full image is scaled.</param>
    /// <returns>The image.</returns>
    public Image Apply(Image image, Rectangle targetLocation = default)
    {
        if (image.Width <= 0 || image.Height <= 0)
            return image;

        var output = Upscale(image);
        image.ReCreate(image.Width * 2, image.Height * 2, output);
        return image;
    }

    /// <summary>
    /// Produces a tightly packed 2× buffer for <paramref name="image"/> using the Scale2x rules.
    /// </summary>
    internal static Color[] Upscale(Image image)
    {
        var srcW = image.Width;
        var srcH = image.Height;
        var dstW = srcW * 2;
        var output = new Color[dstW * srcH * 2];

        for (var y = 0; y < srcH; ++y)
        {
            var row = image.GetRowSpan(y);
            var rowUp = y > 0 ? image.GetRowSpan(y - 1) : default;
            var rowDown = y + 1 < srcH ? image.GetRowSpan(y + 1) : default;
            var outY0 = y * 2 * dstW;
            var outY1 = outY0 + dstW;

            for (var x = 0; x < srcW; ++x)
            {
                var p = row[x];
                var a = y > 0 ? rowUp[x] : p;
                var b = x + 1 < srcW ? row[x + 1] : p;
                var c = x > 0 ? row[x - 1] : p;
                var d = y + 1 < srcH ? rowDown[x] : p;

                //   A
                // C P B
                //   D
                var e0 = c == a && c != d && a != b ? a : p;
                var e1 = a == b && a != c && b != d ? b : p;
                var e2 = d == c && d != b && c != a ? c : p;
                var e3 = b == d && b != a && d != c ? d : p;

                var ox = x * 2;
                output[outY0 + ox] = e0;
                output[outY0 + ox + 1] = e1;
                output[outY1 + ox] = e2;
                output[outY1 + ox + 1] = e3;
            }
        }

        return output;
    }
}
