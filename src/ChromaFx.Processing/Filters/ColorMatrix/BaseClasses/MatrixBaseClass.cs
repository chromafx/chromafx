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

using System.Numerics;
using ChromaFx.Core;
using ChromaFx.Core.Colors;
using ChromaFx.Processing.Filters.Interfaces;
using ChromaFx.Processing.Numerics;

namespace ChromaFx.Processing.Filters.ColorMatrix.BaseClasses;

/// <summary>
/// Color matrix base class
/// </summary>
public abstract class MatrixBaseClass : IFilter
{
    /// <summary>
    /// Gets the matrix.
    /// </summary>
    /// <value>The matrix.</value>
    public abstract Matrix5X5 Matrix { get; }

    /// <summary>
    /// Implements the operator *.
    /// </summary>
    /// <param name="value1">The value1.</param>
    /// <param name="value2">The value2.</param>
    /// <returns>The result of the operator.</returns>
    public static MatrixBaseClass operator *(MatrixBaseClass value1, MatrixBaseClass value2)
    {
        return new ColorMatrix(value1.Matrix * value2.Matrix);
    }

    /// <summary>
    /// Applies the specified image.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <param name="targetLocation">The target location.</param>
    /// <returns>The image</returns>
    public Image Apply(Image image, Rectangle targetLocation = default)
    {
        targetLocation = targetLocation.Normalize(image);

        int width = image.Width;
        Color[] pixels = image.Pixels;
        Matrix5X5 matrix = Matrix;
        FilterParallelism.ForRows(
            targetLocation,
            y => ApplyRow(pixels, width, matrix, y, targetLocation.Left, targetLocation.Right)
        );

        return image;
    }

    private static void ApplyRow(
        Color[] pixels,
        int width,
        Matrix5X5 matrix,
        int y,
        int left,
        int right
    )
    {
        int rowStart = y * width + left;
        int rowWidth = right - left;
        if (rowWidth <= 0)
            return;

        // Planar RGBA floats: [R…][G…][B…][A…]
        int planarLength = rowWidth * 4;
        if (planarLength <= 1024)
        {
            Span<float> planar = stackalloc float[planarLength];
            ApplyPlanar(pixels, rowStart, rowWidth, matrix, planar);
        }
        else
        {
            var rented = FilterBufferPool.RentFloat(planarLength);
            try
            {
                ApplyPlanar(pixels, rowStart, rowWidth, matrix, rented.AsSpan(0, planarLength));
            }
            finally
            {
                FilterBufferPool.Return(rented);
            }
        }
    }

    private static void ApplyPlanar(
        Color[] pixels,
        int rowStart,
        int rowWidth,
        Matrix5X5 matrix,
        Span<float> planar
    )
    {
        var r = planar.Slice(0, rowWidth);
        var g = planar.Slice(rowWidth, rowWidth);
        var b = planar.Slice(rowWidth * 2, rowWidth);
        var a = planar.Slice(rowWidth * 3, rowWidth);

        for (int j = 0; j < rowWidth; ++j)
        {
            var c = pixels[rowStart + j];
            r[j] = c.Red / 255f;
            g[j] = c.Green / 255f;
            b[j] = c.Blue / 255f;
            a[j] = c.Alpha / 255f;
        }

        int simdWidth = Vector<float>.Count;
        var ones = Vector<float>.One;
        int i = 0;

        for (; i <= rowWidth - simdWidth; i += simdWidth)
        {
            var vr = new Vector<float>(r.Slice(i));
            var vg = new Vector<float>(g.Slice(i));
            var vb = new Vector<float>(b.Slice(i));
            var va = new Vector<float>(a.Slice(i));
            var rOut =
                vr * matrix.M11
                + vg * matrix.M21
                + vb * matrix.M31
                + va * matrix.M41
                + ones * matrix.M51;
            var gOut =
                vr * matrix.M12
                + vg * matrix.M22
                + vb * matrix.M32
                + va * matrix.M42
                + ones * matrix.M52;
            var bOut =
                vr * matrix.M13
                + vg * matrix.M23
                + vb * matrix.M33
                + va * matrix.M43
                + ones * matrix.M53;
            var aOut =
                vr * matrix.M14
                + vg * matrix.M24
                + vb * matrix.M34
                + va * matrix.M44
                + ones * matrix.M54;

            for (int j = 0; j < simdWidth; ++j)
            {
                pixels[rowStart + i + j] = new Color(
                    (byte)Math.Clamp(rOut[j] * 255f, 0, 255),
                    (byte)Math.Clamp(gOut[j] * 255f, 0, 255),
                    (byte)Math.Clamp(bOut[j] * 255f, 0, 255),
                    (byte)Math.Clamp(aOut[j] * 255f, 0, 255)
                );
            }
        }

        for (; i < rowWidth; ++i)
        {
            float rf = r[i],
                gf = g[i],
                bf = b[i],
                af = a[i];
            float rOut =
                rf * matrix.M11 + gf * matrix.M21 + bf * matrix.M31 + af * matrix.M41 + matrix.M51;
            float gOut =
                rf * matrix.M12 + gf * matrix.M22 + bf * matrix.M32 + af * matrix.M42 + matrix.M52;
            float bOut =
                rf * matrix.M13 + gf * matrix.M23 + bf * matrix.M33 + af * matrix.M43 + matrix.M53;
            float aOut =
                rf * matrix.M14 + gf * matrix.M24 + bf * matrix.M34 + af * matrix.M44 + matrix.M54;
            pixels[rowStart + i] = new Color(
                (byte)Math.Clamp(rOut * 255f, 0, 255),
                (byte)Math.Clamp(gOut * 255f, 0, 255),
                (byte)Math.Clamp(bOut * 255f, 0, 255),
                (byte)Math.Clamp(aOut * 255f, 0, 255)
            );
        }
    }
}
