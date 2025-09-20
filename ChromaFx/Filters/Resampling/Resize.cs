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
using System.Threading.Tasks;
using ChromaFx.Filters.Interfaces;
using ChromaFx.Filters.Resampling.Enums;
using ChromaFx.Filters.Resampling.ResamplingFilters.Interfaces;
using ChromaFx.Numerics;
using ChromaFx.Colors;

namespace ChromaFx.Filters.Resampling;

/// <summary>
/// Resizes an image
/// </summary>
/// <seealso cref="IFilter"/>
public class Resize : IFilter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Resize"/> class.
    /// </summary>
    /// <param name="height">The new height.</param>
    /// <param name="width">The new width.</param>
    /// <param name="filter">The filter.</param>
    public Resize(int width, int height, ResamplingFiltersAvailable filter)
    {
        Height = height;
        Width = width;
        Filter = FilterList.Filters[filter];
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
    public int Height { get; set; }

    /// <summary>
    /// Gets or sets the width.
    /// </summary>
    /// <value>The width.</value>
    public int Width { get; set; }

    /// <summary>
    /// Applies the resizing filter to the specified image.
    /// </summary>
    /// <param name="image">The image to resize.</param>
    /// <param name="targetLocation">The target location.</param>
    /// <returns>The image</returns>
    public Image Apply(Image image, Rectangle targetLocation = default)
    {
        var output = Sample(image);
        image.ReCreate(Width, Height, output);
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

    private Color[] Sample(Image image)
    {
        Filter.Precompute(image.Width, image.Height, Width, Height);
        var output = new Color[Width * Height];
        var transformationMatrix = GetMatrix(image);
        double tempWidth = Width < 0 ? image.Width : Width;
        double tempHeight = Height < 0 ? image.Width : Height;
        var xScale = tempWidth / image.Width;
        var yScale = tempHeight / image.Height;
        var yRadius = yScale < 1f ? Filter.FilterRadius / yScale : Filter.FilterRadius;
        var xRadius = xScale < 1f ? Filter.FilterRadius / xScale : Filter.FilterRadius;

        Parallel.For(
            0,
            Height,
            y =>
            {
                for (var x = 0; x < Width; ++x)
                {
                    var values = new Vector4(0, 0, 0, 0);
                    float weight = 0;

                    var rotated = Vector2.Transform(new Vector2(x, y), transformationMatrix);
                    var rotatedY = (int)rotated.Y;
                    var rotatedX = (int)rotated.X;
                    var left = (int)(rotatedX - xRadius);
                    var right = (int)(rotatedX + xRadius);
                    var top = (int)(rotatedY - yRadius);
                    var bottom = (int)(rotatedY + yRadius);
                    if (top < 0)
                        top = 0;
                    if (bottom >= image.Height)
                        bottom = image.Height - 1;
                    if (left < 0)
                        left = 0;
                    if (right >= image.Width)
                        right = image.Width - 1;

                    for (int i = top, yCount = 0; i <= bottom; ++i, ++yCount)
                    {
                        for (int j = left, xCount = 0; j <= right; ++j, ++xCount)
                        {
                            var tempYWeight = Filter.YWeights[rotatedY].Values[yCount];
                            var tempXWeight = Filter.XWeights[rotatedX].Values[xCount];
                            var tempWeight = tempYWeight * tempXWeight;

                            if (yRadius == 0 && xRadius == 0)
                                tempWeight = 1;

                            if (tempWeight == 0)
                                continue;

                            var pixel = image.Pixels[i * image.Width + j];
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
                        output[y * Width + x] = new Color
                        {
                            Red = (byte)values.X,
                            Green = (byte)values.Y,
                            Blue = (byte)values.Z,
                            Alpha = (byte)values.W
                        };
                    }
                }
            }
        );

        return output;
    }
}
