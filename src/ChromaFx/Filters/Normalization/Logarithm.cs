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
using ChromaFx.Colors;

namespace ChromaFx.Filters.Normalization;

/// <summary>
/// Does a Logarithm function to the image.
/// </summary>
/// <seealso cref="IFilter"/>
public class Logarithm : IFilter
{
    /// <summary>
    /// Applies the filter to the specified image.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <param name="targetLocation">The target location.</param>
    /// <returns>The image</returns>
    public Image Apply(Image image, Rectangle targetLocation = default)
    {
        targetLocation = targetLocation.Normalize(image);
        var maxValue = GetMaxValue(image, targetLocation);
        maxValue = new Color(
            (byte)(255 / Math.Log(1f + maxValue.Red)),
            (byte)(255 / Math.Log(1f + maxValue.Green)),
            (byte)(255 / Math.Log(1f + maxValue.Blue)),
            maxValue.Alpha
        );

        Parallel.For(
            targetLocation.Bottom,
            targetLocation.Top,
            y =>
            {
                for (var x = targetLocation.Left; x < targetLocation.Right; ++x)
                {
                    var pixelIndex = y * image.Width + x;
                    image.Pixels[pixelIndex].Red = (byte)(
                        maxValue.Red * Math.Log(1f + image.Pixels[pixelIndex].Red)
                    );
                    image.Pixels[pixelIndex].Green = (byte)(
                        maxValue.Green * Math.Log(1f + image.Pixels[pixelIndex].Green)
                    );
                    image.Pixels[pixelIndex].Blue = (byte)(
                        maxValue.Blue * Math.Log(1f + image.Pixels[pixelIndex].Blue)
                    );
                }
            }
        );

        return image;
    }

    private static Color GetMaxValue(Image image, Rectangle targetLocation)
    {
        var returnValue = new Color(0);
        for (var y = targetLocation.Bottom; y < targetLocation.Top; ++y)
        {
            for (var x = targetLocation.Left; x < targetLocation.Right; ++x)
            {
                var red = image.Pixels[y * image.Width + x].Red;
                if (returnValue.Red < red)
                    returnValue.Red = red;
                var green = image.Pixels[y * image.Width + x].Green;
                if (returnValue.Green < green)
                    returnValue.Green = green;
                var blue = image.Pixels[y * image.Width + x].Blue;
                if (returnValue.Blue < blue)
                    returnValue.Blue = blue;
                if (returnValue == Color.White)
                    return returnValue;
            }
        }
        return returnValue;
    }
}
