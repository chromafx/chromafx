﻿/*
Copyright 2016 James Craig
Copyright 2023 Ho Tzin Mein

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

using System.Threading.Tasks;
using Structure.Sketching.Colors;
using Structure.Sketching.Filters.Interfaces;
using Structure.Sketching.Numerics;

namespace Structure.Sketching.Filters.Arithmetic;

/// <summary>
/// Does a subtract operation between two images.
/// </summary>
/// <seealso cref="Structure.Sketching.Filters.Interfaces.IFilter"/>
public class Subtract : IFilter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Subtract"/> class.
    /// </summary>
    /// <param name="secondImage">The second image.</param>
    public Subtract(Image secondImage)
    {
        SecondImage = secondImage;
    }

    /// <summary>
    /// Gets or sets the second image.
    /// </summary>
    /// <value>The second image.</value>
    public Image SecondImage { get; set; }

    /// <summary>
    /// Applies the specified image.
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

        Parallel.For(
            targetLocation.Bottom,
            targetLocation.Top,
            y =>
            {
                if (y >= SecondImage.Height)
                {
                    return;
                }

                for (var x = targetLocation.Left; x < targetLocation.Right; ++x)
                {
                    if (x - targetLocation.Left >= SecondImage.Width)
                    {
                        break;
                    }

                    image
                        .Pixels[y * image.Width + x]
                        .Subtract(
                            SecondImage.Pixels[
                                (y - targetLocation.Bottom) * SecondImage.Width
                                    + (x - targetLocation.Left)
                            ]
                        );
                }
            }
        );

        return image;
    }
}
