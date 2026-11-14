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
/// Classic Eagle 2× upscaler. Exact neighbor equality; never blends.
/// Isolated pixels against a uniform background can disappear (known Eagle quirk).
/// </summary>
/// <seealso cref="IFilter"/>
public class Eagle : IFilter
{
    /// <summary>
    /// Applies Eagle to the whole image (partial <paramref name="targetLocation"/> is ignored).
    /// </summary>
    public Image Apply(Image image, Rectangle targetLocation = default)
    {
        if (image.Width <= 0 || image.Height <= 0)
            return image;

        var output = Upscale(image);
        image.ReCreate(image.Width * 2, image.Height * 2, output);
        return image;
    }

    /// <summary>
    /// Produces a tightly packed 2× buffer using classic Eagle rules.
    /// Neighborhood (S T U / V C W / X Y Z); outputs 1 2 / 3 4.
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
                var c = PixelArtSampler.At(image, x, y);
                var s = PixelArtSampler.At(image, x - 1, y - 1);
                var t = PixelArtSampler.At(image, x, y - 1);
                var u = PixelArtSampler.At(image, x + 1, y - 1);
                var v = PixelArtSampler.At(image, x - 1, y);
                var w = PixelArtSampler.At(image, x + 1, y);
                var xx = PixelArtSampler.At(image, x - 1, y + 1);
                var yy = PixelArtSampler.At(image, x, y + 1);
                var z = PixelArtSampler.At(image, x + 1, y + 1);

                var e0 = v == s && s == t ? s : c;
                var e1 = t == u && u == w ? u : c;
                var e2 = v == xx && xx == yy ? xx : c;
                var e3 = w == z && z == yy ? z : c;

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
