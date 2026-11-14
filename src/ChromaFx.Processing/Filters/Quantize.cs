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
using ChromaFx.Core.Quantization;
using ChromaFx.Processing.Numerics;

namespace ChromaFx.Processing.Filters;

/// <summary>
/// Octree palette quantization (same Core octree as GIF encode) with optional dither.
/// Alpha is preserved. Opaque RGB is snapped to at most <see cref="MaxColors"/> colors.
/// Ordered dither uses <see cref="OrderedDither.QuantizeBiasScale"/> (~±32 at strength 1).
/// </summary>
/// <param name="maxColors">Palette size (2–256). Default 256.</param>
/// <param name="dither">Dither method. Default none.</param>
/// <param name="strength">Ordered-dither strength (0–1); ignored for Floyd–Steinberg. Default 0.5.</param>
public sealed class Quantize(
    int maxColors = 256,
    DitherMethod dither = DitherMethod.None,
    float strength = 0.5f
) : IFilter
{
    /// <summary>Gets the maximum palette size.</summary>
    public int MaxColors { get; } = maxColors;

    /// <summary>Gets the dither method.</summary>
    public DitherMethod Dither { get; } = dither;

    /// <summary>Gets ordered-dither strength.</summary>
    public float Strength { get; } = strength;

    /// <inheritdoc />
    public Image Apply(Image image, Rectangle targetLocation = default)
    {
        if (image.Width <= 0 || image.Height <= 0)
            return image;

        targetLocation = targetLocation.Normalize(image);
        var colors = Math.Clamp(MaxColors, 2, 256);
        var bits = Math.Max(1, Octree.BitsNeeded(colors));

        var octree = new Octree(bits);
        for (var y = targetLocation.Bottom; y < targetLocation.Top; y++)
        {
            var row = image.GetRowSpan(y);
            for (var x = targetLocation.Left; x < targetLocation.Right; x++)
            {
                if (row[x].Alpha == 0)
                    continue;
                octree.AddColor(row[x]);
            }
        }

        if (octree.Leaves == 0)
            return image;

        var palette = octree.Palettize(Math.Min(colors, Math.Max(1, octree.Leaves)));
        if (palette.Count == 0)
            return image;

        return Dither switch
        {
            DitherMethod.FloydSteinberg => ApplyFloyd(image, targetLocation, palette),
            DitherMethod.Bayer4x4 or DitherMethod.Bayer8x8 => ApplyOrdered(
                image,
                targetLocation,
                octree,
                palette
            ),
            _ => ApplyNearest(image, targetLocation, octree, palette),
        };
    }

    private static Image ApplyNearest(
        Image image,
        Rectangle target,
        Octree octree,
        List<Color> palette
    )
    {
        FilterParallelism.ForRows(
            target,
            y =>
            {
                var row = image.GetRowSpan(y);
                for (var x = target.Left; x < target.Right; x++)
                {
                    ref var pixel = ref row[x];
                    if (pixel.Alpha == 0)
                        continue;
                    var mapped = Map(octree, palette, pixel);
                    pixel.Red = mapped.Red;
                    pixel.Green = mapped.Green;
                    pixel.Blue = mapped.Blue;
                }
            }
        );
        return image;
    }

    private Image ApplyOrdered(
        Image image,
        Rectangle target,
        Octree octree,
        List<Color> palette
    )
    {
        var amount = Math.Clamp(Strength, 0f, 1f);
        var ordered =
            Dither == DitherMethod.Bayer8x8
                ? OrderedDitherMethod.Bayer8x8
                : OrderedDitherMethod.Bayer4x4;
        var side = ordered == OrderedDitherMethod.Bayer8x8 ? 8 : 4;
        var mask = side - 1;
        var biases = OrderedDither.BuildBiasTable(
            ordered,
            amount,
            OrderedDither.QuantizeBiasScale
        );
        FilterParallelism.ForRows(
            target,
            y =>
            {
                var row = image.GetRowSpan(y);
                for (var x = target.Left; x < target.Right; x++)
                {
                    ref var pixel = ref row[x];
                    if (pixel.Alpha == 0)
                        continue;

                    var bias = biases[(y & mask) * side + (x & mask)];
                    var sample = new Color(
                        ClampByte(pixel.Red + bias),
                        ClampByte(pixel.Green + bias),
                        ClampByte(pixel.Blue + bias),
                        pixel.Alpha
                    );
                    var mapped = Map(octree, palette, sample);
                    pixel.Red = mapped.Red;
                    pixel.Green = mapped.Green;
                    pixel.Blue = mapped.Blue;
                }
            }
        );
        return image;
    }

    private static Image ApplyFloyd(Image image, Rectangle target, List<Color> palette)
    {
        var w = target.Width;
        var h = target.Height;
        var len = w * h;
        var errR = FilterBufferPool.RentFloat(len);
        var errG = FilterBufferPool.RentFloat(len);
        var errB = FilterBufferPool.RentFloat(len);
        Array.Clear(errR, 0, len);
        Array.Clear(errG, 0, len);
        Array.Clear(errB, 0, len);

        try
        {
            for (var y = 0; y < h; y++)
            {
                var row = image.GetRowSpan(target.Bottom + y);
                for (var x = 0; x < w; x++)
                {
                    var px = target.Left + x;
                    ref var pixel = ref row[px];
                    if (pixel.Alpha == 0)
                        continue;

                    var i = y * w + x;
                    var r = Math.Clamp(pixel.Red + errR[i], 0f, 255f);
                    var g = Math.Clamp(pixel.Green + errG[i], 0f, 255f);
                    var b = Math.Clamp(pixel.Blue + errB[i], 0f, 255f);
                    var chosen = Palette.NearestRgb(
                        palette,
                        (byte)(r + 0.5f),
                        (byte)(g + 0.5f),
                        (byte)(b + 0.5f)
                    );
                    pixel.Red = chosen.Red;
                    pixel.Green = chosen.Green;
                    pixel.Blue = chosen.Blue;

                    Diffuse(
                        errR,
                        errG,
                        errB,
                        w,
                        h,
                        x,
                        y,
                        r - chosen.Red,
                        g - chosen.Green,
                        b - chosen.Blue
                    );
                }
            }
        }
        finally
        {
            FilterBufferPool.Return(errR);
            FilterBufferPool.Return(errG);
            FilterBufferPool.Return(errB);
        }

        return image;
    }

    private static Color Map(Octree octree, List<Color> palette, Color sample)
    {
        if (octree.TryGetPaletteIndex(sample, out var idx))
        {
            idx = Math.Clamp(idx, 0, palette.Count - 1);
            var c = palette[idx];
            return new Color(c.Red, c.Green, c.Blue, sample.Alpha);
        }

        var nearest = Palette.NearestRgb(palette, sample.Red, sample.Green, sample.Blue);
        return new Color(nearest.Red, nearest.Green, nearest.Blue, sample.Alpha);
    }

    private static void Diffuse(
        float[] errR,
        float[] errG,
        float[] errB,
        int w,
        int h,
        int x,
        int y,
        float er,
        float eg,
        float eb
    )
    {
        void Add(int xx, int yy, float factor)
        {
            if ((uint)xx >= (uint)w || (uint)yy >= (uint)h)
                return;
            var i = yy * w + xx;
            errR[i] += er * factor;
            errG[i] += eg * factor;
            errB[i] += eb * factor;
        }

        Add(x + 1, y, 7f / 16f);
        Add(x - 1, y + 1, 3f / 16f);
        Add(x, y + 1, 5f / 16f);
        Add(x + 1, y + 1, 1f / 16f);
    }

    private static byte ClampByte(int v) => (byte)Math.Clamp(v, 0, 255);
}
