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

using System.Runtime.CompilerServices;
using ChromaFx.Core;
using ChromaFx.Core.Colors;

namespace ChromaFx.PixelArt;

/// <summary>Edge-clamped neighborhood reads for pixel-art upscalers.</summary>
internal static class PixelArtSampler
{
    public static Color At(Image image, int x, int y)
    {
        if (x < 0)
            x = 0;
        else if (x >= image.Width)
            x = image.Width - 1;
        if (y < 0)
            y = 0;
        else if (y >= image.Height)
            y = image.Height - 1;
        return image.GetRowSpan(y)[x];
    }

    /// <summary>Edge-clamped read from a tightly packed <paramref name="src"/> buffer.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color At(Color[] src, int width, int height, int x, int y)
    {
        if (x < 0)
            x = 0;
        else if (x >= width)
            x = width - 1;
        if (y < 0)
            y = 0;
        else if (y >= height)
            y = height - 1;
        return src[y * width + x];
    }

    /// <summary>Copies the full image into a tightly packed buffer.</summary>
    public static Color[] CopyTight(Image image)
    {
        var buf = new Color[image.Width * image.Height];
        for (var y = 0; y < image.Height; ++y)
            image.GetRowSpan(y).CopyTo(buf.AsSpan(y * image.Width, image.Width));
        return buf;
    }

    /// <summary>
    /// Fills a 3×3 neighborhood. Interior pixels use direct indexing (no clamps).
    /// </summary>
    public static void Sample3x3(Color[] src, int width, int height, int x, int y, Span<Color> n)
    {
        if ((uint)(x - 1) < (uint)(width - 2) && (uint)(y - 1) < (uint)(height - 2))
        {
            var row0 = (y - 1) * width + (x - 1);
            var row1 = row0 + width;
            var row2 = row1 + width;
            n[0] = src[row0];
            n[1] = src[row0 + 1];
            n[2] = src[row0 + 2];
            n[3] = src[row1];
            n[4] = src[row1 + 1];
            n[5] = src[row1 + 2];
            n[6] = src[row2];
            n[7] = src[row2 + 1];
            n[8] = src[row2 + 2];
            return;
        }

        n[0] = At(src, width, height, x - 1, y - 1);
        n[1] = At(src, width, height, x, y - 1);
        n[2] = At(src, width, height, x + 1, y - 1);
        n[3] = At(src, width, height, x - 1, y);
        n[4] = At(src, width, height, x, y);
        n[5] = At(src, width, height, x + 1, y);
        n[6] = At(src, width, height, x - 1, y + 1);
        n[7] = At(src, width, height, x, y + 1);
        n[8] = At(src, width, height, x + 1, y + 1);
    }
}
