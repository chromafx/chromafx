/*
 * Copyright 2017–2020 JaCraig
 * Modifications Copyright 2023–2025 Ho Tzin Mein
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
using System.Numerics;

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

        Parallel.For(
            targetLocation.Bottom,
            targetLocation.Top,
            y =>
            {
                int rowStart = y * width + targetLocation.Left;
                int rowEnd = y * width + targetLocation.Right;
                int simdWidth = Vector<float>.Count;
                int i = rowStart;
                for (; i <= rowEnd - simdWidth; i += simdWidth)
                {
                    float[] r = new float[simdWidth];
                    float[] g = new float[simdWidth];
                    float[] b = new float[simdWidth];
                    float[] a = new float[simdWidth];
                    for (int j = 0; j < simdWidth; ++j)
                    {
                        r[j] = pixels[i + j].Red / 255f;
                        g[j] = pixels[i + j].Green / 255f;
                        b[j] = pixels[i + j].Blue / 255f;
                        a[j] = pixels[i + j].Alpha / 255f;
                    }
                    var vr = new Vector<float>(r);
                    var vg = new Vector<float>(g);
                    var vb = new Vector<float>(b);
                    var va = new Vector<float>(a);
                    var ones = new Vector<float>(1f);
                    var rOut = vr * matrix.M11 + vg * matrix.M21 + vb * matrix.M31 + va * matrix.M41 + ones * matrix.M51;
                    var gOut = vr * matrix.M12 + vg * matrix.M22 + vb * matrix.M32 + va * matrix.M42 + ones * matrix.M52;
                    var bOut = vr * matrix.M13 + vg * matrix.M23 + vb * matrix.M33 + va * matrix.M43 + ones * matrix.M53;
                    var aOut = vr * matrix.M14 + vg * matrix.M24 + vb * matrix.M34 + va * matrix.M44 + ones * matrix.M54;
                    for (int j = 0; j < simdWidth; ++j)
                    {
                        pixels[i + j] = new Color(
                            (byte)Math.Clamp(rOut[j] * 255f, 0, 255),
                            (byte)Math.Clamp(gOut[j] * 255f, 0, 255),
                            (byte)Math.Clamp(bOut[j] * 255f, 0, 255),
                            (byte)Math.Clamp(aOut[j] * 255f, 0, 255)
                        );
                    }
                }
                // Scalar fallback for remaining pixels
                for (; i < rowEnd; ++i)
                {
                    var c = pixels[i];
                    float rf = c.Red / 255f, gf = c.Green / 255f, bf = c.Blue / 255f, af = c.Alpha / 255f;
                    float rOut = rf * matrix.M11 + gf * matrix.M21 + bf * matrix.M31 + af * matrix.M41 + matrix.M51;
                    float gOut = rf * matrix.M12 + gf * matrix.M22 + bf * matrix.M32 + af * matrix.M42 + matrix.M52;
                    float bOut = rf * matrix.M13 + gf * matrix.M23 + bf * matrix.M33 + af * matrix.M43 + matrix.M53;
                    float aOut = rf * matrix.M14 + gf * matrix.M24 + bf * matrix.M34 + af * matrix.M44 + matrix.M54;
                    pixels[i] = new Color(
                        (byte)Math.Clamp(rOut * 255f, 0, 255),
                        (byte)Math.Clamp(gOut * 255f, 0, 255),
                        (byte)Math.Clamp(bOut * 255f, 0, 255),
                        (byte)Math.Clamp(aOut * 255f, 0, 255)
                    );
                }
            }
        );
        return image;
    }
}
