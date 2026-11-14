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
using ChromaFx.Processing.Filters.Interfaces;
using ChromaFx.Processing.Numerics;

namespace ChromaFx.Retro.Filters;

/// <summary>
/// Snaps each RGB channel to the 4-level EGA palette 
/// (<c>00</c>, <c>55</c>, <c>AA</c>, <c>FF</c> — 64 colors total). Alpha is preserved.
/// </summary>
public sealed class EgaQuantize : IFilter
{
    private const int ParallelPixelThreshold = 65536;
    private static readonly byte[] Lut = CreateLut();

    public Image Apply(Image image, Rectangle targetLocation = default)
    {
        targetLocation = targetLocation.Normalize(image);
        int left = targetLocation.Left;
        int right = targetLocation.Right;
        int bottom = targetLocation.Bottom;
        int top = targetLocation.Top;
        int pixelCount = (top - bottom) * (right - left);
        var lut = Lut;

        void ProcessRow(int y)
        {
            var row = image.GetRowSpan(y);
            for (var x = left; x < right; ++x)
            {
                ref var pixel = ref row[x];
                pixel.Red = lut[pixel.Red];
                pixel.Green = lut[pixel.Green];
                pixel.Blue = lut[pixel.Blue];
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

    /// <summary>
    /// Maps a single channel to the nearest EGA level.
    /// </summary>
    internal static byte SnapChannel(byte value) => Lut[value];

    private static byte[] CreateLut()
    {
        var lut = new byte[256];
        for (var i = 0; i < 256; ++i)
        {
            // Nearest of 0, 85, 170, 255
            lut[i] = (byte)(((i + 42) / 85) * 85);
        }
        return lut;
    }
}