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
using ChromaFx.Core.Colors;
using ChromaFx.Processing.Filters.Interfaces;
using ChromaFx.Processing.Numerics;

namespace ChromaFx.Processing.Filters.Normalization;

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

        FilterParallelism.ForRows(
            targetLocation,
            y =>
            {
                var row = image.GetRowSpan(y);
                for (var x = targetLocation.Left; x < targetLocation.Right; ++x)
                {
                    ref var pixel = ref row[x];
                    pixel.Red = (byte)(maxValue.Red * Math.Log(1f + pixel.Red));
                    pixel.Green = (byte)(maxValue.Green * Math.Log(1f + pixel.Green));
                    pixel.Blue = (byte)(maxValue.Blue * Math.Log(1f + pixel.Blue));
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
            var row = image.GetRowSpan(y);
            for (var x = targetLocation.Left; x < targetLocation.Right; ++x)
            {
                var pixel = row[x];
                if (returnValue.Red < pixel.Red)
                    returnValue.Red = pixel.Red;
                if (returnValue.Green < pixel.Green)
                    returnValue.Green = pixel.Green;
                if (returnValue.Blue < pixel.Blue)
                    returnValue.Blue = pixel.Blue;
                if (returnValue == Color.White)
                    return returnValue;
            }
        }
        return returnValue;
    }
}
