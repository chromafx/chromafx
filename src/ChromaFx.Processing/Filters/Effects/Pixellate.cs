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

using ChromaFx.Core;
using ChromaFx.Processing.Numerics;

namespace ChromaFx.Processing.Filters.Effects;

/// <summary>
/// Pixellates an image
/// </summary>
/// <seealso cref="IFilter"/>
/// <remarks>
/// Initializes a new instance of the <see cref="Pixellate"/> class.
/// </remarks>
/// <param name="pixelSize">Size of the pixel.</param>
public class Pixellate(int pixelSize) : IFilter
{
    /// <summary>
    /// Gets or sets the size of the pixel.
    /// </summary>
    /// <value>The size of the pixel.</value>
    public int PixelSize { get; set; } = pixelSize;

    /// <summary>
    /// Applies the filter to the specified image.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <param name="targetLocation">The target location.</param>
    /// <returns>The image</returns>
    public Image Apply(Image image, Rectangle targetLocation = default)
    {
        if (PixelSize <= 0)
            return image;

        targetLocation = targetLocation.Normalize(image);
        var half = PixelSize / 2;
        var source = FilterBufferPool.RentTightCopy(image);
        try
        {
            for (var y = targetLocation.Bottom; y < targetLocation.Top; y += PixelSize)
            {
                var minY = Math.Clamp(y - half, targetLocation.Bottom, targetLocation.Top - 1);
                var maxY = Math.Clamp(y + half, targetLocation.Bottom, targetLocation.Top);
                if (maxY <= minY)
                    maxY = minY + 1;

                for (var x = targetLocation.Left; x < targetLocation.Right; x += PixelSize)
                {
                    var minX = Math.Clamp(x - half, targetLocation.Left, targetLocation.Right - 1);
                    var maxX = Math.Clamp(x + half, targetLocation.Left, targetLocation.Right);
                    if (maxX <= minX)
                        maxX = minX + 1;

                    uint rValue = 0;
                    uint gValue = 0;
                    uint bValue = 0;
                    var numberPixels = 0;

                    for (var y2 = minY; y2 < maxY; ++y2)
                    {
                        var rowStart = y2 * image.Width;
                        for (var x2 = minX; x2 < maxX; ++x2)
                        {
                            var tempPixel = source[rowStart + x2];
                            rValue += tempPixel.Red;
                            gValue += tempPixel.Green;
                            bValue += tempPixel.Blue;
                            ++numberPixels;
                        }
                    }

                    rValue /= (uint)numberPixels;
                    gValue /= (uint)numberPixels;
                    bValue /= (uint)numberPixels;

                    for (var y2 = minY; y2 < maxY; ++y2)
                    {
                        var row = image.GetRowSpan(y2);
                        for (var x2 = minX; x2 < maxX; ++x2)
                        {
                            row[x2].Red = (byte)rValue;
                            row[x2].Green = (byte)gValue;
                            row[x2].Blue = (byte)bValue;
                        }
                    }
                }
            }
        }
        finally
        {
            FilterBufferPool.Return(source);
        }

        return image;
    }
}
