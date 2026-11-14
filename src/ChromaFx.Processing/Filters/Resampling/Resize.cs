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
using ChromaFx.Processing.Numerics;

namespace ChromaFx.Processing.Filters.Resampling;

/// <summary>
/// Resizes an image
/// </summary>
/// <seealso cref="IFilter"/>
/// <remarks>
/// Initializes a new instance of the <see cref="Resize"/> class.
/// </remarks>
/// <param name="height">The new height.</param>
/// <param name="width">The new width.</param>
/// <param name="filter">The filter.</param>
public class Resize(int width, int height, ResamplingFilter filter) : IFilter
{
    /// <summary>
    /// Gets or sets the filter.
    /// </summary>
    /// <value>The filter.</value>
    public IResamplingFilter Filter { get; set; } = ResamplingKernels.Create(filter);

    /// <summary>
    /// Gets or sets the height.
    /// </summary>
    /// <value>The height.</value>
    public int Height { get; set; } = height;

    /// <summary>
    /// Gets or sets the width.
    /// </summary>
    /// <value>The width.</value>
    public int Width { get; set; } = width;

    /// <summary>
    /// Applies the resizing filter to the specified image.
    /// </summary>
    /// <param name="image">The image to resize.</param>
    /// <param name="targetLocation">The target location.</param>
    /// <returns>The image</returns>
    public Image Apply(Image image, Rectangle targetLocation = default)
    {
        var pixelCount = Width * Height;
        var output = FilterBufferPool.RentColor(pixelCount);
        try
        {
            output.AsSpan(0, pixelCount).Clear();
            Sample(image, output);
            image.ReCreate(Width, Height, output.AsSpan(0, pixelCount));
        }
        finally
        {
            FilterBufferPool.Return(output);
        }

        return image;
    }

    /// <summary>
    /// Gets the matrix.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <returns>The transformation matrix</returns>
    protected Matrix3x2 GetMatrix(Image image)
    {
        var xScale = (float)image.Width / Width;
        var yScale = (float)image.Height / Height;
        return Matrix3x2.CreateScale(xScale, yScale);
    }

    private void Sample(Image image, Color[] output)
    {
        Filter.Precompute(image.Width, image.Height, Width, Height);
        double tempWidth = Width < 0 ? image.Width : Width;
        double tempHeight = Height < 0 ? image.Height : Height;
        var xScale = tempWidth / image.Width;
        var yScale = tempHeight / image.Height;

        if (Filter.FilterRadius > 0 && xScale >= 1.0 && yScale >= 1.0 && CanUseSeparablePath())
        {
            SampleSeparable(image, output);
            return;
        }

        var transformationMatrix = GetMatrix(image);

        FilterParallelism.For(
            0,
            Height,
            Width * Height,
            y =>
            {
                for (var x = 0; x < Width; ++x)
                {
                    var rotated = Vector2.Transform(new Vector2(x, y), transformationMatrix);
                    var rotatedY = (int)rotated.Y;
                    var rotatedX = (int)rotated.X;
                    if (
                        rotatedY < 0
                        || rotatedY >= image.Height
                        || rotatedX < 0
                        || rotatedX >= image.Width
                    )
                        continue;

                    var xWeights = Filter.XWeights[rotatedX];
                    var yWeights = Filter.YWeights[rotatedY];
                    var values = new Vector4(0, 0, 0, 0);

                    for (var yCount = 0; yCount < yWeights.Values.Length; ++yCount)
                    {
                        var tempYWeight = yWeights.Values[yCount];
                        if (tempYWeight == 0)
                            continue;

                        var srcY = yWeights.Left + yCount;
                        var sourceRow = image.GetRowSpan(srcY);
                        for (var xCount = 0; xCount < xWeights.Values.Length; ++xCount)
                        {
                            var tempWeight = (float)(tempYWeight * xWeights.Values[xCount]);
                            if (tempWeight == 0)
                                continue;

                            var pixel = sourceRow[xWeights.Left + xCount];
                            values.X += pixel.Red * tempWeight;
                            values.Y += pixel.Green * tempWeight;
                            values.Z += pixel.Blue * tempWeight;
                            values.W += pixel.Alpha * tempWeight;
                        }
                    }

                    values = Vector4.Clamp(values, Vector4.Zero, new Vector4(255, 255, 255, 255));
                    output[y * Width + x] = new Color
                    {
                        Red = (byte)values.X,
                        Green = (byte)values.Y,
                        Blue = (byte)values.Z,
                        Alpha = (byte)values.W,
                    };
                }
            }
        );
    }

    private bool CanUseSeparablePath() =>
        Filter
            is not (
                RobidouxFilter
                or RobidouxSharpFilter
                or RobidouxSoftFilter
                or BellFilter
                or CubicBSplineFilter
                or CubicConvolutionFilter
                or MitchellFilter
                or QuadraticBSplineFilter
            );

    private void SampleSeparable(Image image, Color[] output)
    {
        var transformationMatrix = GetMatrix(image);
        var intermediate = FilterBufferPool.RentVector4(image.Height * Width);

        try
        {
            FilterParallelism.For(
                0,
                image.Height,
                image.Height * Width,
                row =>
                {
                    var sourceRowSpan = image.GetRowSpan(row);
                    for (var x = 0; x < Width; ++x)
                    {
                        var values = new Vector4(0, 0, 0, 0);
                        var rotatedX = (int)
                            Vector2.Transform(new Vector2(x, 0), transformationMatrix).X;
                        var xWeights = Filter.XWeights[rotatedX];

                        for (var xCount = 0; xCount < xWeights.Values.Length; ++xCount)
                        {
                            var tempWeight = xWeights.Values[xCount];
                            if (tempWeight == 0)
                                continue;

                            var pixel = sourceRowSpan[xWeights.Left + xCount];
                            values.X += pixel.Red * (float)tempWeight;
                            values.Y += pixel.Green * (float)tempWeight;
                            values.Z += pixel.Blue * (float)tempWeight;
                            values.W += pixel.Alpha * (float)tempWeight;
                        }

                        intermediate[row * Width + x] = values;
                    }
                }
            );

            FilterParallelism.For(
                0,
                Height,
                Height * Width,
                y =>
                {
                    for (var x = 0; x < Width; ++x)
                    {
                        var values = new Vector4(0, 0, 0, 0);
                        float weight = 0;
                        var rotatedY = (int)
                            Vector2.Transform(new Vector2(0, y), transformationMatrix).Y;
                        var yWeights = Filter.YWeights[rotatedY];

                        for (var yCount = 0; yCount < yWeights.Values.Length; ++yCount)
                        {
                            var tempWeight = yWeights.Values[yCount];
                            if (tempWeight == 0)
                                continue;

                            var pixel = intermediate[(yWeights.Left + yCount) * Width + x];
                            values.X += pixel.X * (float)tempWeight;
                            values.Y += pixel.Y * (float)tempWeight;
                            values.Z += pixel.Z * (float)tempWeight;
                            values.W += pixel.W * (float)tempWeight;
                            weight += (float)tempWeight;
                        }

                        if (weight == 0)
                            weight = 1;

                        if (weight > 0)
                        {
                            values = Vector4.Clamp(
                                values,
                                Vector4.Zero,
                                new Vector4(255, 255, 255, 255)
                            );
                            output[y * Width + x] = new Color
                            {
                                Red = (byte)values.X,
                                Green = (byte)values.Y,
                                Blue = (byte)values.Z,
                                Alpha = (byte)values.W,
                            };
                        }
                    }
                }
            );
        }
        finally
        {
            FilterBufferPool.Return(intermediate);
        }
    }
}
