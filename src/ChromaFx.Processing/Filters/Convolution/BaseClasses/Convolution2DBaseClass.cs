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
    public Image Apply(Image image, Rectangle targetLocation = default)
    {
        targetLocation = targetLocation.Normalize(image);

        var tempPixels = FilterBufferPool.RentCopy(image.PixelSpan);
        try
        {
            var width = image.Width;
            var height = image.Height;
            var stride = image.Stride;
            var xMatrix = XMatrix;
            var yMatrix = YMatrix;
            var kernelWidth = Width;
            var absolute = Absolute;

            FilterParallelism.ForRows(
                targetLocation,
                y =>
                {
                    var outRow = image.GetRowSpan(y);
                    for (var x = targetLocation.Left; x < targetLocation.Right; ++x)
                    {
                        float xR = 0,
                            xG = 0,
                            xB = 0,
                            xA = 0;
                        float yR = 0,
                            yG = 0,
                            yB = 0,
                            yA = 0;
                        float weightX = 0;
                        float weightY = 0;
                        var xCurrent = -kernelWidth >> 1;
                        var yCurrent = -Height >> 1;
                        var matrixOffset = 0;

                        for (var matrixIndex = 0; matrixIndex < xMatrix.Length; ++matrixIndex)
                        {
                            if (matrixIndex % kernelWidth == 0 && matrixIndex != 0)
                            {
                                ++yCurrent;
                                xCurrent = 0;
                            }

                            if (
                                xCurrent + x < width
                                && xCurrent + x >= 0
                                && yCurrent + y < height
                                && yCurrent + y >= 0
                            )
                            {
                                var xMatrixValue = xMatrix[matrixOffset];
                                var yMatrixValue = yMatrix[matrixOffset];
                                if (xMatrixValue != 0 || yMatrixValue != 0)
                                {
                                    var tempPixel = tempPixels[
                                        (yCurrent + y) * stride + x + xCurrent
                                    ];
                                    xR += xMatrixValue * tempPixel.Red;
                                    xG += xMatrixValue * tempPixel.Green;
                                    xB += xMatrixValue * tempPixel.Blue;
                                    xA += xMatrixValue * tempPixel.Alpha;
                                    yR += yMatrixValue * tempPixel.Red;
                                    yG += yMatrixValue * tempPixel.Green;
                                    yB += yMatrixValue * tempPixel.Blue;
                                    yA += yMatrixValue * tempPixel.Alpha;
                                    weightX += xMatrixValue;
                                    weightY += yMatrixValue;
                                }
                                ++matrixOffset;
                            }
                            ++xCurrent;
                        }

                        if (weightX == 0)
                            weightX = 1;
                        if (weightY == 0)
                            weightY = 1;
                        if (weightX > 0 && weightY > 0)
                        {
                            if (absolute)
                            {
                                xR = Math.Abs(xR);
                                xG = Math.Abs(xG);
                                xB = Math.Abs(xB);
                                xA = Math.Abs(xA);
                                yR = Math.Abs(yR);
                                yG = Math.Abs(yG);
                                yB = Math.Abs(yB);
                                yA = Math.Abs(yA);
                            }

                            xR /= weightX;
                            xG /= weightX;
                            xB /= weightX;
                            xA /= weightX;
                            yR /= weightY;
                            yG /= weightY;
                            yB /= weightY;
                            yA /= weightY;

                            var tempResult = new Vector4(
                                MathF.Sqrt(xR * xR + yR * yR),
                                MathF.Sqrt(xG * xG + yG * yG),
                                MathF.Sqrt(xB * xB + yB * yB),
                                MathF.Sqrt(xA * xA + yA * yA)
                            );
                            tempResult =
                                Vector4.Clamp(
                                    tempResult,
                                    Vector4.Zero,
                                    new Vector4(255, 255, 255, 255)
                                ) / 255f;
                            outRow[x] = tempResult;
                        }
                    }
                }
            );
        }
        finally
        {
            FilterBufferPool.Return(tempPixels);
        }

        return image;
    }
}
