/*
Copyright 2025 Ho Tzin Mein

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using System.Numerics;
using ChromaFx.Core;
using ChromaFx.Core.Colors;
using ChromaFx.Processing.Filters.Interfaces;
using ChromaFx.Processing.Numerics;

namespace ChromaFx.Processing.Filters.Convolution.BaseClasses;

/// <summary>
/// Convolution 2d base class
/// </summary>
/// <seealso cref="IFilter" />
public abstract class Convolution2DBaseClass : IFilter
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
    /// Gets the x matrix.
    /// </summary>
    /// <value>The x matrix.</value>
    public abstract float[] XMatrix { get; }

    /// <summary>
    /// Gets the y matrix.
    /// </summary>
    /// <value>The y matrix.</value>
    public abstract float[] YMatrix { get; }

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
        Parallel.For(
            targetLocation.Bottom,
            targetLocation.Top,
            y =>
            {
                fixed (Color* pointer = &image.Pixels[y * image.Width + targetLocation.Left])
                {
                    var outputPointer = pointer;
                    for (var x = targetLocation.Left; x < targetLocation.Right; ++x)
                    {
                        var xValue = new Vector4(0, 0, 0, 0);
                        var yValue = new Vector4(0, 0, 0, 0);
                        float weightX = 0;
                        float weightY = 0;
                        var xCurrent = -Width >> 1;
                        var yCurrent = -Height >> 1;
                        fixed (float* xMatrixPointer = &XMatrix[0])
                        {
                            fixed (float* yMatrixPointer = &YMatrix[0])
                            {
                                var xMatrixValue = xMatrixPointer;
                                var yMatrixValue = yMatrixPointer;
                                for (
                                    var matrixIndex = 0;
                                    matrixIndex < XMatrix.Length;
                                    ++matrixIndex
                                )
                                {
                                    if (matrixIndex % Width == 0 && matrixIndex != 0)
                                    {
                                        ++yCurrent;
                                        xCurrent = 0;
                                    }
                                    if (
                                        xCurrent + x < image.Width
                                        && xCurrent + x >= 0
                                        && yCurrent + y < image.Height
                                        && yCurrent + y >= 0
                                    )
                                    {
                                        if (*xMatrixValue != 0 || *yMatrixValue != 0)
                                        {
                                            var start = (yCurrent + y) * image.Width + x + xCurrent;
                                            var tempPixel = tempPixels[start];
                                            xValue += new Vector4(
                                                *xMatrixValue * tempPixel.Red,
                                                *xMatrixValue * tempPixel.Green,
                                                *xMatrixValue * tempPixel.Blue,
                                                *xMatrixValue * tempPixel.Alpha
                                            );
                                            yValue += new Vector4(
                                                *yMatrixValue * tempPixel.Red,
                                                *yMatrixValue * tempPixel.Green,
                                                *yMatrixValue * tempPixel.Blue,
                                                *yMatrixValue * tempPixel.Alpha
                                            );
                                            weightX += *xMatrixValue;
                                            weightY += *yMatrixValue;
                                        }
                                        ++xMatrixValue;
                                        ++yMatrixValue;
                                    }
                                    ++xCurrent;
                                }
                            }
                        }
                        if (weightX == 0)
                            weightX = 1;
                        if (weightY == 0)
                            weightY = 1;
                        if (weightX > 0 && weightY > 0)
                        {
                            if (Absolute)
                            {
                                yValue = Vector4.Abs(yValue);
                                xValue = Vector4.Abs(xValue);
                            }
                            xValue /= weightX;
                            yValue /= weightY;
                            var tempResult = Vector4.SquareRoot(xValue * xValue + yValue * yValue);
                            tempResult =
                                Vector4.Clamp(
                                    tempResult,
                                    Vector4.Zero,
                                    new Vector4(255, 255, 255, 255)
                                ) / 255f;
                            *outputPointer = tempResult;
                            ++outputPointer;
                        }
                        else
                        {
                            ++outputPointer;
                        }
                    }
                }
            }
        );
        return image;
    }
}
