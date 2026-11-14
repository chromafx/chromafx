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
/// Affine transformation base class
/// </summary>
/// <seealso cref="IFilter"/>
public abstract class AffineBaseClass : IFilter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AffineBaseClass"/> class.
    /// </summary>
    /// <param name="width">The new width.</param>
    /// <param name="height">The new height.</param>
    /// <param name="filter">The filter to use (defaults to nearest neighbor).</param>
    protected AffineBaseClass(
        int width = -1,
        int height = -1,
        ResamplingFilter filter = ResamplingFilter.NearestNeighbor
    )
    {
        Width = width;
        Height = height;
        Filter = ResamplingKernels.Create(filter);
    }

    /// <summary>
    /// Gets or sets the filter.
    /// </summary>
    /// <value>The filter.</value>
    public IResamplingFilter Filter { get; set; }

    /// <summary>
    /// Gets or sets the height.
    /// </summary>
    /// <value>The height.</value>
    protected int Height { get; set; }

    /// <summary>
    /// Gets or sets the width.
    /// </summary>
    /// <value>The width.</value>
    protected int Width { get; set; }

    /// <summary>
    /// Gets the transformation matrix.
    /// </summary>
    /// <value>The transformation matrix.</value>
    private Matrix3x2 TransformationMatrix { get; set; }

    /// <summary>
    /// Applies the filter to the specified image.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <param name="targetLocation">The target location.</param>
    /// <returns>The image</returns>
    public Image Apply(Image image, Rectangle targetLocation = default)
    {
        Filter.Precompute(image.Width, image.Height, Width, Height);
        targetLocation = targetLocation.Normalize(image);
        var copy = FilterBufferPool.RentCopy(image.PixelSpan);
        try
        {
            TransformationMatrix = GetMatrix(image, targetLocation);

            var clampMax = new Vector4(255, 255, 255, 255);
            FilterParallelism.For(
                targetLocation.Bottom,
                targetLocation.Top,
                (targetLocation.Top - targetLocation.Bottom)
                    * (targetLocation.Right - targetLocation.Left),
                y =>
                {
                    var destRow = image.GetRowSpan(y);
                    for (int x = targetLocation.Left; x < targetLocation.Right; x++)
                    {
                        var values = Vector4.Zero;

                        var rotated = Vector2.Transform(new Vector2(x, y), TransformationMatrix);
                        var rotatedY = (int)rotated.Y;
                        var rotatedX = (int)rotated.X;
                        if (
                            rotatedY >= image.Height
                            || rotatedY < 0
                            || rotatedX >= image.Width
                            || rotatedX < 0
                        )
                        {
                            destRow[x] = default;
                            continue;
                        }

                        var xWeights = Filter.XWeights[rotatedX];
                        var yWeights = Filter.YWeights[rotatedY];
                        for (var yCount = 0; yCount < yWeights.Values.Length; ++yCount)
                        {
                            var tempYWeight = yWeights.Values[yCount];
                            if (tempYWeight == 0)
                                continue;

                            var srcY = yWeights.Left + yCount;
                            var rowOffset = srcY * image.Stride;
                            for (var xCount = 0; xCount < xWeights.Values.Length; ++xCount)
                            {
                                var tempWeight = (float)(tempYWeight * xWeights.Values[xCount]);
                                if (tempWeight == 0)
                                    continue;

                                var pixel = copy[rowOffset + xWeights.Left + xCount];
                                values.X += pixel.Red * tempWeight;
                                values.Y += pixel.Green * tempWeight;
                                values.Z += pixel.Blue * tempWeight;
                                values.W += pixel.Alpha * tempWeight;
                            }
                        }

                        values = Vector4.Clamp(values, Vector4.Zero, clampMax);
                        destRow[x] = new Color(
                            (byte)values.X,
                            (byte)values.Y,
                            (byte)values.Z,
                            (byte)values.W
                        );
                    }
                }
            );
        }
        finally
        {
            FilterBufferPool.Return(copy);
        }

        return image;
    }

    /// <summary>
    /// Gets the matrix.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <param name="targetLocation">The target location.</param>
    /// <returns>The matrix used for the transformation</returns>
    protected abstract Matrix3x2 GetMatrix(Image image, Rectangle targetLocation);
}
