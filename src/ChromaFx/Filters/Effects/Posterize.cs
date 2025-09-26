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

namespace ChromaFx.Filters.Effects;

/// <summary>
/// Posterizes an image
/// </summary>
/// <seealso cref="IFilter"/>
public class Posterize : IFilter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Posterize"/> class.
    /// </summary>
    /// <param name="divisions">The number of divisions.</param>
    public Posterize(int divisions)
    {
        if (divisions < 1)
            divisions = 1;
        var step = 255 / divisions;
        Divisions = new byte[divisions + 1];
        Divisions[0] = 0;
        for (var x = 1; x < divisions; ++x)
        {
            Divisions[x] = (byte)(Divisions[x - 1] + step);
        }
        Divisions[divisions] = 255;
    }

    /// <summary>
    /// Gets or sets the divisions.
    /// </summary>
    /// <value>
    /// The divisions.
    /// </value>
    public byte[] Divisions { get; set; }

    /// <summary>
    /// Applies the filter to the specified image.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <param name="targetLocation">The target location.</param>
    /// <returns>The filtered image.</returns>
    public Image Apply(Image image, Rectangle targetLocation = default)
    {
        targetLocation = targetLocation.Normalize(image);
        Parallel.For(targetLocation.Bottom, targetLocation.Top, (y) =>
        {
            var rowOffset = y * image.Width;
            for (var x = targetLocation.Left; x < targetLocation.Right; ++x)
            {
                var pixelOffset = rowOffset + x;
                image.Pixels[pixelOffset].Red = FindValue(image.Pixels[pixelOffset].Red);
                image.Pixels[pixelOffset].Green = FindValue(image.Pixels[pixelOffset].Green);
                image.Pixels[pixelOffset].Blue = FindValue(image.Pixels[pixelOffset].Blue);
            }
        });
        return image;
    }

    /// <summary>
    /// Finds the value.
    /// </summary>
    /// <param name="x">The input value.</param>
    /// <returns>The posterized value.</returns>
    private byte FindValue(byte x)
    {
        if (Divisions.Length == 1)
            return Divisions[0];
        for (var z = 1; z < Divisions.Length; ++z)
        {
            if (Divisions[z] > x)
                return Divisions[z - 1];
        }
        return Divisions[^1];
    }
}
