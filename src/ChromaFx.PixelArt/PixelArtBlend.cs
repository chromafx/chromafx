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

namespace ChromaFx.PixelArt;

/// <summary>
/// Midpoint / quarter blends used by SaI-family upscalers (RGBA channel averages).
/// Uses integer channel means — not <see cref="Color.Average"/>, which clamps on add
/// and corrupts alpha when blending two opaque colors.
/// </summary>
internal static class PixelArtBlend
{
    /// <summary>Linear midpoint of two colors (SaI INTERPOLATE).</summary>
    public static Color Interpolate(Color a, Color b) =>
        a == b
            ? a
            : new Color(
                (byte)((a.Red + b.Red) / 2),
                (byte)((a.Green + b.Green) / 2),
                (byte)((a.Blue + b.Blue) / 2),
                (byte)((a.Alpha + b.Alpha) / 2)
            );

    /// <summary>Average of four colors (SaI Q_INTERPOLATE).</summary>
    public static Color QInterpolate(Color a, Color b, Color c, Color d) =>
        new(
            (byte)((a.Red + b.Red + c.Red + d.Red) / 4),
            (byte)((a.Green + b.Green + c.Green + d.Green) / 4),
            (byte)((a.Blue + b.Blue + c.Blue + d.Blue) / 4),
            (byte)((a.Alpha + b.Alpha + c.Alpha + d.Alpha) / 4)
        );

    /// <summary>¾ of <paramref name="heavy"/> + ¼ of <paramref name="light"/>.</summary>
    public static Color ThreeQuarter(Color heavy, Color light) =>
        heavy == light ? heavy : heavy.Lerp(light, 0.25f);
}
