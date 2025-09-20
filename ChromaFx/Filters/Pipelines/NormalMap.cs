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
using ChromaFx.Filters.Convolution.Enums;
using ChromaFx.Filters.Interfaces;
using ChromaFx.Numerics;

namespace ChromaFx.Filters.Pipelines;

/// <summary>
/// Normal map processing pipeline
/// </summary>
/// <seealso cref="IFilter"/>
public class NormalMap : IFilter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NormalMap"/> class.
    /// </summary>
    /// <param name="xDirection">The x direction.</param>
    /// <param name="yDirection">The y direction.</param>
    public NormalMap(XDirection xDirection, YDirection yDirection)
    {
        YDirection = yDirection;
        XDirection = xDirection;
    }

    /// <summary>
    /// Gets or sets the x direction.
    /// </summary>
    /// <value>The x direction.</value>
    public XDirection XDirection { get; set; }

    /// <summary>
    /// Gets or sets the y direction.
    /// </summary>
    /// <value>The y direction.</value>
    public YDirection YDirection { get; set; }

    /// <summary>
    /// Applies the filter to the specified image.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <param name="targetLocation">The target location.</param>
    /// <returns>The image</returns>
    public Image Apply(Image image, Rectangle targetLocation = default)
    {
        targetLocation =
            targetLocation == default
                ? new Rectangle(0, 0, image.Width, image.Height)
                : targetLocation.Clamp(image);
        var tempImageX = new BumpMap(
            XDirection == XDirection.LeftToRight ? Direction.LeftToRight : Direction.RightToLeft
        ).Apply(image.Copy(), targetLocation);
        var tempImageY = new BumpMap(
            YDirection == YDirection.TopToBottom ? Direction.TopToBottom : Direction.BottomToTop
        ).Apply(image.Copy(), targetLocation);
        Parallel.For(
            targetLocation.Bottom,
            targetLocation.Top,
            y =>
            {
                for (var x = targetLocation.Left; x < targetLocation.Right; ++x)
                {
                    var index = y * image.Width + x;

                    var tempVector = new Vector3(
                        tempImageX.Pixels[index].Red / 255f,
                        tempImageY.Pixels[index].Red / 255f,
                        1f
                    );

                    tempVector = Vector3.Normalize(tempVector);
                    tempVector = new Vector3(
                        tempVector.X + 1.0f,
                        tempVector.Y + 1f,
                        tempVector.Z + 1f
                    );
                    tempVector /= 2.0f;

                    image.Pixels[index].Red = (byte)(tempVector.X * 255);
                    image.Pixels[index].Green = (byte)(tempVector.Y * 255);
                    image.Pixels[index].Blue = (byte)(tempVector.Z * 255);
                }
            }
        );
        return image;
    }
}
