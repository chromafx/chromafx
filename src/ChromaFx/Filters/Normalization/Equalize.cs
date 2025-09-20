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
using ChromaFx.Numerics.Interfaces;

namespace ChromaFx.Filters.Normalization;

/// <summary>
/// Equalizes of an image
/// </summary>
/// <seealso cref="IFilter"/>
public class Equalize : IFilter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Equalize"/> class.
    /// </summary>
    /// <param name="histogram">The histogram.</param>
    public Equalize(IHistogram histogram = null)
    {
        Histogram = histogram ?? new RgbHistogram();
    }

    /// <summary>
    /// Gets or sets the histogram.
    /// </summary>
    /// <value>The histogram.</value>
    private IHistogram Histogram { get; }

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
        Histogram.LoadImage(image).Equalize();

        Parallel.For(
            targetLocation.Bottom,
            targetLocation.Top,
            y =>
            {
                for (var x = targetLocation.Left; x < targetLocation.Right; ++x)
                {
                    var pixelIndex = y * image.Width + x;
                    var resultColor = Histogram.EqualizeColor(image.Pixels[pixelIndex]);

                    image.Pixels[pixelIndex].Red = resultColor.Red;
                    image.Pixels[pixelIndex].Green = resultColor.Green;
                    image.Pixels[pixelIndex].Blue = resultColor.Blue;
                }
            }
        );

        return image;
    }
}
