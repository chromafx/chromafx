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

using ChromaFx.Filters.Interfaces;
using ChromaFx.Numerics;

namespace ChromaFx.Filters.Arithmetic;

/// <summary>
/// Does a division operation between two images.
/// </summary>
/// <seealso cref="IFilter"/>
/// <remarks>
/// Initializes a new instance of the <see cref="Division"/> class.
/// </remarks>
/// <param name="secondImage">The second image.</param>
public class Division(Image secondImage) : IFilter
{

    /// <summary>
    /// Gets or sets the second image.
    /// </summary>
    /// <value>The second image.</value>
    public Image SecondImage { get; set; } = secondImage;

    /// <summary>
    /// Applies the specified image.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <param name="targetLocation">The target location.</param>
    /// <returns>The image</returns>
    public Image Apply(Image image, Rectangle targetLocation = default)
    {
        targetLocation = targetLocation.Normalize(image);

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
                        .Divide(
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
