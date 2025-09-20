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

using ChromaFx.Filters.Arithmetic;
using ChromaFx.Filters.Convolution;
using ChromaFx.Filters.Interfaces;
using ChromaFx.Numerics;

namespace ChromaFx.Filters.Sharpening;

/// <summary>
/// Does an unsharp filter on an image
/// </summary>
/// <seealso cref="IFilter"/>
public class Unsharp : IFilter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Unsharp"/> class.
    /// </summary>
    /// <param name="size">The size.</param>
    /// <param name="constant">The constant to scale by (usually 0.2 to 0.7).</param>
    public Unsharp(int size, float constant)
    {
        Constant = constant;
        Size = size;
    }

    /// <summary>
    /// Gets or sets the constant.
    /// </summary>
    /// <value>The constant.</value>
    public float Constant { get; set; }

    /// <summary>
    /// Gets or sets the size.
    /// </summary>
    /// <value>The size.</value>
    public int Size { get; set; }

    /// <summary>
    /// Applies the filter to the specified image.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <param name="targetLocation">The target location.</param>
    /// <returns>The image</returns>
    public Image Apply(Image image, Rectangle targetLocation = default)
    {
        targetLocation = targetLocation == default ? new Rectangle(0, 0, image.Width, image.Height) : targetLocation.Clamp(image);
        var blurredImage = new Image(image);
        new BoxBlur(Size).Apply(blurredImage, targetLocation);
        var difference = new Image(image);
        new Subtract(blurredImage).Apply(difference);
        new ColorMatrix.ColorMatrix(new Matrix5X5(
            Constant, 0, 0, 0, 0,
            0, Constant, 0, 0, 0,
            0, 0, Constant, 0, 0,
            0, 0, 0, 1, 0,
            0, 0, 0, 0, 1)).Apply(difference, targetLocation);
        new Add(difference).Apply(image);
        return image;
    }
}