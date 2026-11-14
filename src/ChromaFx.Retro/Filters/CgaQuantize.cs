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
using ChromaFx.Core.Colors;
using ChromaFx.Processing.Filters.Interfaces;
using ChromaFx.Processing.Numerics;

namespace ChromaFx.Retro.Filters;

public enum CgaPaletteMode
{
    /// <summary>Standard 16-color RGBI palette.</summary>
    Standard16,
    /// <summary>Palette 1 (Low): Black, Cyan, Magenta, Light Gray.</summary>
    Palette1Low,
    /// <summary>Palette 1 (High): Black, Bright Cyan, Bright Magenta, White.</summary>
    Palette1High,
    /// <summary>Palette 2 (Low): Black, Green, Red, Brown.</summary>
    Palette2Low,
    /// <summary>Palette 2 (High): Black, Bright Green, Bright Red, Yellow.</summary>
    Palette2High,
    /// <summary>2-color monochrome: Black and White.</summary>
    Monochrome2
}

/// <summary>
/// Quantizes pixels to specific historical CGA palette modes using nearest-color matching.
/// </summary>
public sealed class CgaQuantize(CgaPaletteMode mode = CgaPaletteMode.Palette1High) : IFilter
{
    private const int ParallelPixelThreshold = 65536;
    private readonly (byte R, byte G, byte B)[] _palette = mode switch
    {
        CgaPaletteMode.Standard16 =>
        [
            (0, 0, 0), (0, 0, 170), (0, 170, 0), (0, 170, 170),
                (170, 0, 0), (170, 0, 170), (170, 85, 0), (170, 170, 170),
                (85, 85, 85), (85, 85, 255), (85, 255, 85), (85, 255, 255),
                (255, 85, 85), (255, 85, 255), (255, 255, 85), (255, 255, 255)
        ],
        CgaPaletteMode.Palette1Low =>
        [
            (0, 0, 0),     // Black
                (0, 170, 170), // Cyan
                (170, 0, 170), // Magenta
                (170, 170, 170)// Light Gray
        ],
        CgaPaletteMode.Palette1High =>
        [
            (0, 0, 0),     // Black
                (85, 255, 255),// Bright Cyan
                (255, 85, 255),// Bright Magenta
                (255, 255, 255)// White
        ],
        CgaPaletteMode.Palette2Low =>
        [
            (0, 0, 0),     // Black
                (0, 170, 0),   // Green
                (170, 0, 0),   // Red
                (170, 85, 0)   // Brown
        ],
        CgaPaletteMode.Palette2High =>
        [
            (0, 0, 0),     // Black
                (85, 255, 85), // Bright Green
                (255, 85, 85), // Bright Red
                (255, 255, 85) // Yellow
        ],
        CgaPaletteMode.Monochrome2 =>
        [
            (0, 0, 0),     // Black
                (255, 255, 255)// White
        ],
        _ => throw new ArgumentOutOfRangeException(nameof(mode))
    };

    public Image Apply(Image image, Rectangle targetLocation = default)
    {
        targetLocation = targetLocation.Normalize(image);
        int left = targetLocation.Left;
        int right = targetLocation.Right;
        int bottom = targetLocation.Bottom;
        int top = targetLocation.Top;
        int pixelCount = (top - bottom) * (right - left);
        var palette = _palette;

        void ProcessRow(int y)
        {
            var row = image.GetRowSpan(y);
            for (var x = left; x < right; ++x)
            {
                ref var pixel = ref row[x];
                
                int minDistance = int.MaxValue;
                int bestIndex = 0;

                for (int i = 0; i < palette.Length; i++)
                {
                    var (R, G, B) = palette[i];
                    int dr = pixel.Red - R;
                    int dg = pixel.Green - G;
                    int db = pixel.Blue - B;
                    int distance = (dr * dr) + (dg * dg) + (db * db);

                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        bestIndex = i;
                    }
                }

                pixel.Red = palette[bestIndex].R;
                pixel.Green = palette[bestIndex].G;
                pixel.Blue = palette[bestIndex].B;
            }
        }

        if (pixelCount < ParallelPixelThreshold || top - bottom <= 1)
        {
            for (var y = bottom; y < top; ++y)
                ProcessRow(y);
        }
        else
        {
            Parallel.For(bottom, top, ProcessRow);
        }

        return image;
    }
}