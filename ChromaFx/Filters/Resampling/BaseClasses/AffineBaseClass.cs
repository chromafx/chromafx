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
using ChromaFx.Filters.Interfaces;
using ChromaFx.Filters.Resampling.Enums;
using ChromaFx.Filters.Resampling.ResamplingFilters.Interfaces;
using ChromaFx.Numerics;
using ChromaFx.Colors;

namespace ChromaFx.Filters.Resampling.BaseClasses;

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
        ResamplingFiltersAvailable filter = ResamplingFiltersAvailable.NearestNeighbor
    )
    {
        Width = width;
        Height = height;
        ResamplingFilter = FilterList.Filters;
        Filter = ResamplingFilter[filter];
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
    /// Gets or sets the resampling filter dictionary.
    /// </summary>
    /// <value>The resampling filter.</value>
    private Dictionary<ResamplingFiltersAvailable, IResamplingFilter> ResamplingFilter { get; }

    /// <summary>
    /// Gets the transformation matrix.
    /// </summary>
    /// <value>The transformation matrix.</value>
    private Matrix3x2 TransformationMatrix { get; set; }

    /// <summary>
    /// Gets or sets the x radius for the sampling filter.
    /// </summary>
    /// <value>The x radius.</value>
    private double XRadius { get; set; }

    /// <summary>
    /// Gets or sets the y radius for the sampling filter.
    /// </summary>
    /// <value>The y radius.</value>
    private double YRadius { get; set; }

    /// <summary>
    /// Applies the filter to the specified image.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <param name="targetLocation">The target location.</param>
    /// <returns>The image</returns>
    public Image Apply(Image image, Rectangle targetLocation = default)
    {
        Filter.Precompute(image.Width, image.Height, Width, Height);
        targetLocation =
            targetLocation == default
                ? new Rectangle(0, 0, image.Width, image.Height)
                : targetLocation.Clamp(image);
        var copy = new Color[image.Pixels.Length];
        Array.Copy(image.Pixels, copy, copy.Length);
        TransformationMatrix = GetMatrix(image, targetLocation);
        double tempWidth = Width < 0 ? image.Width : Width;
        double tempHeight = Height < 0 ? image.Width : Height;
        var xScale = tempWidth / image.Width;
        var yScale = tempHeight / image.Height;
        YRadius = yScale < 1f ? Filter.FilterRadius / yScale : Filter.FilterRadius;
        XRadius = xScale < 1f ? Filter.FilterRadius / xScale : Filter.FilterRadius;

        Parallel.For(
            targetLocation.Bottom,
            targetLocation.Top,
            y =>
            {
                for (int x = targetLocation.Left; x < targetLocation.Right; x++)
                {
                    var values = new Vector4(0, 0, 0, 0);
                    float weight = 0;

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
                        image.Pixels[y * image.Width + x] = new Color(0, 0, 0, 255);
                        continue;
                    }
                    var left = (int)(rotatedX - XRadius);
                    var right = (int)(rotatedX + XRadius);
                    var top = (int)(rotatedY - YRadius);
                    var bottom = (int)(rotatedY + YRadius);
                    if (top < 0)
                        top = 0;
                    if (bottom >= image.Height)
                        bottom = image.Height - 1;
                    if (left < 0)
                        left = 0;
                    if (right >= image.Width)
                        right = image.Width - 1;
                    for (int i = top, yCount = 0; i <= bottom; i++, yCount++)
                    {
                        for (int j = left, xCount = 0; j <= right; j++, xCount++)
                        {
                            var tempYWeight = Filter.YWeights[rotatedY].Values[yCount];
                            var tempXWeight = Filter.XWeights[rotatedX].Values[xCount];
                            var tempWeight = tempYWeight * tempXWeight;

                            if (YRadius == 0 && XRadius == 0)
                                tempWeight = 1;

                            if (tempWeight == 0)
                                continue;

                            var pixel = copy[i * image.Width + j];
                            values.X += pixel.Red * (float)tempWeight;
                            values.Y += pixel.Green * (float)tempWeight;
                            values.Z += pixel.Blue * (float)tempWeight;
                            values.W += pixel.Alpha * (float)tempWeight;
                            weight += (float)tempWeight;
                        }
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
                        image.Pixels[y * image.Width + x] = new Color(
                            (byte)values.X,
                            (byte)values.Y,
                            (byte)values.Z,
                            (byte)values.W
                        );
                    }
                }
            }
        );

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
