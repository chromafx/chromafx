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
using ChromaFx.Processing.Numerics;

namespace ChromaFx.Processing.Filters.Morphology;

/// <summary>
/// Separable max (dilate) / min (constrict) of R, G and B over the [x-r, x+r) x [y-r, y+r)
/// window clipped to the image; alpha is kept.
/// </summary>
internal static class MorphologyKernel
{
    internal static Image Apply(Image image, Rectangle targetLocation, int radius, bool max)
    {
        targetLocation = targetLocation.Normalize(image);
        var width = image.Width;
        var height = image.Height;
        var source = FilterBufferPool.RentTightCopy(image);
        var horizontal = FilterBufferPool.RentColor(width * height);
        try
        {
            var rowStart = Math.Max(0, targetLocation.Bottom - radius);
            var rowEnd = Math.Min(height, targetLocation.Top + radius - 1);
            FilterParallelism.For(
                rowStart,
                rowEnd,
                (rowEnd - rowStart) * width,
                y =>
                {
                    var src = source.AsSpan(y * width, width);
                    var dst = horizontal.AsSpan(y * width, width);
                    for (var x = targetLocation.Left; x < targetLocation.Right; ++x)
                    {
                        var start = Math.Max(0, x - radius);
                        var end = Math.Min(width, x + radius);
                        var first = src[start];
                        byte r = first.Red,
                            g = first.Green,
                            b = first.Blue;
                        for (var i = start + 1; i < end; ++i)
                        {
                            var p = src[i];
                            r = Pick(r, p.Red, max);
                            g = Pick(g, p.Green, max);
                            b = Pick(b, p.Blue, max);
                        }

                        dst[x] = new Color(r, g, b);
                    }
                }
            );

            FilterParallelism.ForRows(
                targetLocation,
                y =>
                {
                    var row = image.GetRowSpan(y);
                    var start = Math.Max(0, y - radius);
                    var end = Math.Min(height, y + radius);
                    for (var x = targetLocation.Left; x < targetLocation.Right; ++x)
                    {
                        var first = horizontal[start * width + x];
                        byte r = first.Red,
                            g = first.Green,
                            b = first.Blue;
                        for (var i = start + 1; i < end; ++i)
                        {
                            var p = horizontal[i * width + x];
                            r = Pick(r, p.Red, max);
                            g = Pick(g, p.Green, max);
                            b = Pick(b, p.Blue, max);
                        }

                        row[x] = new Color(r, g, b, source[y * width + x].Alpha);
                    }
                }
            );
        }
        finally
        {
            FilterBufferPool.Return(source);
            FilterBufferPool.Return(horizontal);
        }

        return image;
    }

    private static byte Pick(byte current, byte value, bool max) =>
        max ? (value > current ? value : current) : (value < current ? value : current);
}
