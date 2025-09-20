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

using System.Threading.Tasks;
using ChromaFx.Filters.Interfaces;
using ChromaFx.Numerics;
using ChromaFx.Colors;

namespace ChromaFx.Filters.Resampling;

/// <summary>
/// Crops the image
/// </summary>
/// <seealso cref="IFilter"/>
public class Crop : IFilter
{
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
        var result = new Color[targetLocation.Width * targetLocation.Height];

        Parallel.For(
            targetLocation.Bottom,
            targetLocation.Top,
            y =>
            {
                for (var x = targetLocation.Left; x < targetLocation.Right; ++x)
                {
                    result[
                        (y - targetLocation.Bottom) * targetLocation.Width
                            + (x - targetLocation.Left)
                    ] = image.Pixels[y * image.Width + x];
                }
            }
        );

        return image.ReCreate(targetLocation.Width, targetLocation.Height, result);
    }
}
