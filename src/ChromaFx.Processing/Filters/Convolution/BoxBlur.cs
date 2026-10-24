/*
 * Copyright 2017–2020 JaCraig
 * Modifications Copyright 2023–2026 Ho Tzin Mein
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
using ChromaFx.Processing.Filters.Convolution.BaseClasses;
using ChromaFx.Processing.Numerics;

namespace ChromaFx.Processing.Filters.Convolution;

/// <summary>
/// Box blur (separable two-pass implementation).
/// </summary>
/// <seealso cref="ConvolutionBaseClass" />
public class BoxBlur : ConvolutionBaseClass
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BoxBlur"/> class.
    /// </summary>
    /// <param name="size">The size.</param>
    public BoxBlur(int size)
    {
        size = Math.Max(1, size);
        Height = size;
        Width = size;
        _matrix = new float[size * size];
        for (var x = 0; x < _matrix.Length; ++x)
            _matrix[x] = 1;
    }

    /// <inheritdoc />
    public override bool Absolute => false;

    /// <inheritdoc />
    public override int Height { get; }

    /// <inheritdoc />
    public override float[] Matrix => _matrix;

    /// <inheritdoc />
    public override float Offset => 0;

    /// <inheritdoc />
    public override int Width { get; }

    private readonly float[] _matrix;

    /// <inheritdoc />
    public override Image Apply(Image image, Rectangle targetLocation = default)
    {
        targetLocation = targetLocation.Normalize(image);
        var size = Width;
        if (size <= 1)
            return image;

        var radius = size >> 1;
        var width = image.Width;
        var height = image.Height;
        var stride = image.Stride;
        // Temp uses the same stride layout so vertical indexing matches.
        var temp = FilterBufferPool.RentCopy(image.PixelSpan);
        try
        {
            // Horizontal pass: source → temp (full image so vertical edges stay consistent).
            FilterParallelism.ForRows(
                new Rectangle(0, 0, width, height),
                y =>
                {
                    BlurRowHorizontal(image.GetRowSpan(y), temp.AsSpan(y * stride, width), radius);
                }
            );

            // Vertical pass: temp → source (only target region written).
            FilterParallelism.ForRows(
                targetLocation,
                y =>
                {
                    var dstRow = image.GetRowSpan(y);
                    for (var x = targetLocation.Left; x < targetLocation.Right; ++x)
                    {
                        int sumR = 0,
                            sumG = 0,
                            sumB = 0,
                            count = 0;
                        var y0 = Math.Max(0, y - radius);
                        var y1 = Math.Min(height - 1, y + radius);
                        for (var yi = y0; yi <= y1; ++yi)
                        {
                            var p = temp[yi * stride + x];
                            sumR += p.Red;
                            sumG += p.Green;
                            sumB += p.Blue;
                            ++count;
                        }

                        ref var outPx = ref dstRow[x];
                        outPx.Red = (byte)(sumR / count);
                        outPx.Green = (byte)(sumG / count);
                        outPx.Blue = (byte)(sumB / count);
                    }
                }
            );
        }
        finally
        {
            FilterBufferPool.Return(temp);
        }

        return image;
    }

    /// <summary>
    /// Sliding-window horizontal box blur for one row (alpha preserved).
    /// </summary>
    private static void BlurRowHorizontal(ReadOnlySpan<Color> src, Span<Color> dst, int radius)
    {
        var width = src.Length;
        if (width == 0)
            return;

        int sumR = 0,
            sumG = 0,
            sumB = 0,
            count = 0;

        // Seed window for x = 0: [0, min(radius, width-1)]
        var x1 = Math.Min(width - 1, radius);
        for (var xi = 0; xi <= x1; ++xi)
        {
            var p = src[xi];
            sumR += p.Red;
            sumG += p.Green;
            sumB += p.Blue;
            ++count;
        }

        for (var x = 0; x < width; ++x)
        {
            dst[x].Red = (byte)(sumR / count);
            dst[x].Green = (byte)(sumG / count);
            dst[x].Blue = (byte)(sumB / count);
            dst[x].Alpha = src[x].Alpha;

            // Advance window to center x+1: remove (x - radius), add (x + 1 + radius)
            var remove = x - radius;
            if (remove >= 0)
            {
                var p = src[remove];
                sumR -= p.Red;
                sumG -= p.Green;
                sumB -= p.Blue;
                --count;
            }

            var add = x + 1 + radius;
            if (add < width)
            {
                var p = src[add];
                sumR += p.Red;
                sumG += p.Green;
                sumB += p.Blue;
                ++count;
            }
        }
    }
}
