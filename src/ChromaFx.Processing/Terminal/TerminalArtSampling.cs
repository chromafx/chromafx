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

namespace ChromaFx.Processing.Terminal;

internal static class TerminalArtSampling
{
    public static Color SampleColorRow(
        Image image,
        double srcX0,
        double srcX1,
        int colorRow,
        double scaleY,
        Color background
    )
    {
        var srcY0 = colorRow * scaleY;
        var srcY1 = (colorRow + 1) * scaleY;
        return GetBoxAveragedColor(image, srcX0, srcY0, srcX1, srcY1, background);
    }

    public static Color GetBoxAveragedColor(
        Image image,
        double x0,
        double y0,
        double x1,
        double y1,
        Color background
    )
    {
        var minX = Math.Max(0, (int)Math.Floor(x0));
        var minY = Math.Max(0, (int)Math.Floor(y0));
        var maxX = Math.Min(image.Width, (int)Math.Ceiling(x1));
        var maxY = Math.Min(image.Height, (int)Math.Ceiling(y1));

        if (minX >= maxX || minY >= maxY)
            return background;

        double sumR = 0,
            sumG = 0,
            sumB = 0,
            area = 0;

        for (var sy = minY; sy < maxY; sy++)
        {
            var rowOverlap = Overlap1D(sy, sy + 1, y0, y1);
            if (rowOverlap <= 0)
                continue;

            var row = image.GetRowSpan(sy);
            for (var sx = minX; sx < maxX; sx++)
            {
                var colOverlap = Overlap1D(sx, sx + 1, x0, x1);
                if (colOverlap <= 0)
                    continue;

                var weight = rowOverlap * colOverlap;
                var composited = CompositeOverBackground(row[sx], background);
                sumR += composited.Red * weight;
                sumG += composited.Green * weight;
                sumB += composited.Blue * weight;
                area += weight;
            }
        }

        if (area <= 0)
            return background;

        return new Color(
            (byte)Math.Clamp((int)Math.Round(sumR / area), 0, 255),
            (byte)Math.Clamp((int)Math.Round(sumG / area), 0, 255),
            (byte)Math.Clamp((int)Math.Round(sumB / area), 0, 255),
            255
        );
    }

    public static Color CompositeOverBackground(Color pixel, Color background)
    {
        var alpha = pixel.Alpha;
        if (alpha == 255)
            return new Color(pixel.Red, pixel.Green, pixel.Blue, 255);
        if (alpha == 0)
            return new Color(background.Red, background.Green, background.Blue, 255);

        var inv = 255 - alpha;
        return new Color(
            (byte)((pixel.Red * alpha + background.Red * inv) / 255),
            (byte)((pixel.Green * alpha + background.Green * inv) / 255),
            (byte)((pixel.Blue * alpha + background.Blue * inv) / 255),
            255
        );
    }

    private static double Overlap1D(int edge0, int edge1, double range0, double range1)
    {
        var start = Math.Max(edge0, range0);
        var end = Math.Min(edge1, range1);
        return end > start ? end - start : 0;
    }
}
