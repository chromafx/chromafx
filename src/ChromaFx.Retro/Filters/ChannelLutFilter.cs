/*
 * Copyright 2026 Ho Tzin Mein
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
using ChromaFx.Core.Quantization;
using ChromaFx.Processing;
using ChromaFx.Processing.Numerics;

namespace ChromaFx.Retro.Filters;

/// <summary>
/// Shared Apply loop for per-channel LUT Retro quantizers (web-safe / EGA / VGA).
/// </summary>
internal static class ChannelLutFilter
{
    internal static Image Apply(
        Image image,
        Rectangle targetLocation,
        byte[] lut,
        OrderedDitherMethod dither,
        float strength
    )
    {
        targetLocation = targetLocation.Normalize(image);
        var method = dither;
        var amount = strength;
        var useDither = method != OrderedDitherMethod.None && amount != 0f;
        var biases = useDither
            ? OrderedDither.BuildBiasTable(method, amount, OrderedDither.RetroBiasScale, round: true)
            : [];
        var side = method == OrderedDitherMethod.Bayer8x8 ? 8 : 4;
        var mask = side - 1;

        FilterParallelism.ForRows(
            targetLocation,
            y =>
            {
                var row = image.GetRowSpan(y);
                for (var x = targetLocation.Left; x < targetLocation.Right; ++x)
                {
                    ref var pixel = ref row[x];
                    if (useDither)
                    {
                        var bias = biases[(y & mask) * side + (x & mask)];
                        pixel.Red = lut[(byte)Math.Clamp(pixel.Red + bias, 0, 255)];
                        pixel.Green = lut[(byte)Math.Clamp(pixel.Green + bias, 0, 255)];
                        pixel.Blue = lut[(byte)Math.Clamp(pixel.Blue + bias, 0, 255)];
                    }
                    else
                    {
                        pixel.Red = lut[pixel.Red];
                        pixel.Green = lut[pixel.Green];
                        pixel.Blue = lut[pixel.Blue];
                    }
                }
            }
        );

        return image;
    }
}
