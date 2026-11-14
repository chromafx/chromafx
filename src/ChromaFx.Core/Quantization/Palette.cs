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

using ChromaFx.Core.Colors;

namespace ChromaFx.Core.Quantization;

/// <summary>
/// Shared palette helpers (nearest RGB).
/// </summary>
public static class Palette
{
    /// <summary>
    /// Returns the palette entry with the smallest squared RGB distance to (<paramref name="r"/>,<paramref name="g"/>,<paramref name="b"/>).
    /// </summary>
    public static Color NearestRgb(IReadOnlyList<Color> palette, byte r, byte g, byte b) =>
        NearestRgb(palette, (int)r, g, b);

    /// <summary>
    /// Returns the palette entry with the smallest squared RGB distance to (<paramref name="r"/>,<paramref name="g"/>,<paramref name="b"/>).
    /// Channel values may be outside 0–255 (e.g. after dither bias before clamp).
    /// </summary>
    public static Color NearestRgb(IReadOnlyList<Color> palette, int r, int g, int b)
    {
        var best = 0;
        var bestDist = int.MaxValue;
        for (var i = 0; i < palette.Count; i++)
        {
            var c = palette[i];
            var dr = r - c.Red;
            var dg = g - c.Green;
            var db = b - c.Blue;
            var d = dr * dr + dg * dg + db * db;
            if (d < bestDist)
            {
                bestDist = d;
                best = i;
            }
        }

        return palette[best];
    }
}
