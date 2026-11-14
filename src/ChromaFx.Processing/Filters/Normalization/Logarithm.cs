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
using ChromaFx.Processing.Numerics;
using ChromaFx.Processing.Simd;

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
        // Match prior truncation of scale factors to byte before multiplying Log.
        var scaleR = (byte)(255 / Math.Log(1f + maxValue.Red));
        var scaleG = (byte)(255 / Math.Log(1f + maxValue.Green));
        var scaleB = (byte)(255 / Math.Log(1f + maxValue.Blue));

        var lutR = new byte[256];
        var lutG = new byte[256];
        var lutB = new byte[256];
        for (var i = 0; i < 256; ++i)
        {
            lutR[i] = (byte)(scaleR * Math.Log(1f + i));
            lutG[i] = (byte)(scaleG * Math.Log(1f + i));
            lutB[i] = (byte)(scaleB * Math.Log(1f + i));
        }

        FilterParallelism.ForRows(
            targetLocation,
            y =>
            {
                var row = image.GetRowSpan(y).Slice(targetLocation.Left, targetLocation.Width);
                PixelSimd.ApplyChannelLuts(row, lutR, lutG, lutB);
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
