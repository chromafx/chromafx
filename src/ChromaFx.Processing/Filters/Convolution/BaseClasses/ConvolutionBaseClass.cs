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

using ChromaFx.Core.Colors;
using System.Numerics;
using ChromaFx.Processing.Filters.Interfaces;
using ChromaFx.Processing.Numerics;
using ChromaFx.Core;

namespace ChromaFx.Processing.Filters.Convolution.BaseClasses;

/// <summary>
/// Convolution base class
/// </summary>
/// <seealso cref="IFilter" />
public abstract class ConvolutionBaseClass : IFilter
{
    /// <summary>
    /// Gets a value indicating whether this <see cref="ConvolutionBaseClass"/> is absolute.
    /// </summary>
    /// <value><c>true</c> if absolute; otherwise, <c>false</c>.</value>
    public abstract bool Absolute { get; }

    /// <summary>
    /// Gets the height.
    /// </summary>
    /// <value>The height.</value>
    public abstract int Height { get; }

    /// <summary>
    /// Gets the matrix.
    /// </summary>
    /// <value>The matrix.</value>
    public abstract float[] Matrix { get; }

    /// <summary>
    /// Gets the offset.
    /// </summary>
    /// <value>The offset.</value>
    public abstract float Offset { get; }

    /// <summary>
    /// Gets the width.
    /// </summary>
    /// <value>The width.</value>
    public abstract int Width { get; }

    /// <summary>
    /// Implements the operator *.
    /// </summary>
    /// <param name="value1">The value1.</param>
    /// <param name="value2">The value2.</param>
    /// <returns>The result of the operator.</returns>
    public static ConvolutionBaseClass operator *(ConvolutionBaseClass value1, ConvolutionBaseClass value2)
    {
        if (value1.Matrix.Length > value2.Matrix.Length)
        {
            (value2, value1) = (value1, value2);
        }
        var height = value1.Height + value2.Height - 1;
        var width = value1.Width + value2.Width - 1;
        var values = new float[width * height];
        var value2YPosition = -value2.Height + 1;
        for (var y = 0; y < height; ++y)
        {
            var value2XPosition = -value2.Width + 1;
            for (var x = 0; x < width; ++x)
            {
                float value = 0;
                for (var i = 0; i < value2.Width; ++i)
                {
                    for (var j = 0; j < value2.Height; ++j)
                    {
                        if (i + value2XPosition >= 0 && i + value2XPosition < value1.Width
                                                     && j + value2YPosition >= 0 && j + value2YPosition < value1.Height)
                        {
                            value += value1.Matrix[i + value2XPosition + (j + value2YPosition) * value1.Width] * value2.Matrix[i + j * value2.Width];
                        }
                    }
                }
                values[x + y * width] = value;
                ++value2XPosition;
            }
            ++value2YPosition;
        }
        return new ConvolutionFilter(values, width, height, value1.Absolute | value2.Absolute, value1.Offset + value2.Offset);
    }

    /// <summary>
    /// Applies the specified image.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <param name="targetLocation">The target location.</param>
    /// <returns>The image</returns>
    public unsafe Image Apply(Image image, Rectangle targetLocation = default)
    {
        targetLocation = targetLocation.Normalize(image);
        var tempPixels = new Color[image.Pixels.Length];
        Array.Copy(image.Pixels, tempPixels, image.Pixels.Length);
        Parallel.For(targetLocation.Bottom, targetLocation.Top, y =>
        {
            fixed (Color* pointer = &image.Pixels[y * image.Width + targetLocation.Left])
            {
                var outputPointer = pointer;
                for (var x = targetLocation.Left; x < targetLocation.Right; ++x)
                {
                    var values = new Vector4(0, 0, 0, 0);
                    float weight = 0;
                    var xCurrent = -Width >> 1;
                    var yCurrent = -Height >> 1;
                    fixed (float* matrixPointer = &Matrix[0])
                    {
                        var matrixValue = matrixPointer;
                        for (var matrixIndex = 0; matrixIndex < Matrix.Length; ++matrixIndex)
                        {
                            if (matrixIndex % Width == 0 && matrixIndex != 0)
                            {
                                ++yCurrent;
                                xCurrent = 0;
                            }
                            if (xCurrent + x < image.Width && xCurrent + x >= 0
                                                           && yCurrent + y < image.Height && yCurrent + y >= 0)
                            {
                                if (*matrixValue != 0)
                                {
                                    var start = (yCurrent + y) * image.Width + x + xCurrent;
                                    var tempPixel = tempPixels[start];
                                    values += new Vector4(*matrixValue * tempPixel.Red,
                                        *matrixValue * tempPixel.Green,
                                        *matrixValue * tempPixel.Blue,
                                        tempPixel.Alpha);
                                    weight += *matrixValue;
                                }
                                ++matrixValue;
                            }
                            ++xCurrent;
                        }
                    }
                    if (weight == 0)
                        weight = 1;
                    if (weight > 0)
                    {
                        if (Absolute)
                        {
                            values = Vector4.Abs(values);
                        }
                        values /= weight;
                        values = new Vector4(values.X + Offset, values.Y + Offset, values.Z + Offset, 1);
                        values = Vector4.Clamp(values, Vector4.Zero, new Vector4(255, 255, 255, 255));
                        (*outputPointer).Red = (byte)values.X;
                        (*outputPointer).Green = (byte)values.Y;
                        (*outputPointer).Blue = (byte)values.Z;
                        ++outputPointer;
                    }
                    else
                    {
                        ++outputPointer;
                    }
                }
            }
        });
        return image;
    }
}