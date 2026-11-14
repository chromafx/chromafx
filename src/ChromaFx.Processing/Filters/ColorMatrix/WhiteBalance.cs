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
using ChromaFx.Processing.Numerics;

namespace ChromaFx.Processing.Filters.ColorMatrix;

/// <summary>
/// Corrects white balance for a known illuminant Kelvin (inverse Helland scales),
/// optionally with a green↔magenta tint amount (−1…1).
/// </summary>
public class WhiteBalance : MatrixBaseClass
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WhiteBalance"/> class.
    /// </summary>
    /// <param name="kelvin">Assumed illuminant temperature (1000…40000).</param>
    /// <param name="tint">Optional green↔magenta adjustment (−1…1; positive = green).</param>
    public WhiteBalance(double kelvin, float tint = 0f)
    {
        kelvin = Math.Clamp(kelvin, 1000, 40000);
        tint = Math.Clamp(tint, -1f, 1f);

        var (r, g, b) = HellandRgb(kelvin);
        // Invert illuminant scales and normalize so the max channel stays 1.
        var invR = r == 0 ? 1f : 255f / r;
        var invG = g == 0 ? 1f : 255f / g;
        var invB = b == 0 ? 1f : 255f / b;
        var max = Math.Max(invR, Math.Max(invG, invB));
        invR /= max;
        invG /= max;
        invB /= max;

        // Apply tint on the correction (same axis as Tint filter).
        invG *= 1f + tint * 0.5f;
        invR *= 1f - tint * 0.25f;
        invB *= 1f - tint * 0.25f;

        Matrix = new Matrix5X5(
            invR,
            0,
            0,
            0,
            0,
            0,
            invG,
            0,
            0,
            0,
            0,
            0,
            invB,
            0,
            0,
            0,
            0,
            0,
            1,
            0,
            0,
            0,
            0,
            0,
            1
        );
    }

    /// <inheritdoc />
    public override Matrix5X5 Matrix { get; }

    internal static (byte R, byte G, byte B) HellandRgb(double kelvin)
    {
        var value = Math.Clamp(kelvin, 1000, 40000) / 100;
        int red,
            green,
            blue;

        if (value <= 66)
            red = 255;
        else
            red = ((int)(329.698727446 * Math.Pow(value - 60, -0.1332047592))).ToByte();

        if (value <= 66)
            green = (int)(99.4708025861 * Math.Log(value) - 161.1195681661);
        else
            green = (int)(288.1221695283 * Math.Pow(value - 60, -0.0755148492));
        green = green.ToByte();

        if (value >= 66)
            blue = 255;
        else if (value <= 19)
            blue = 0;
        else
            blue = ((int)(138.5177312231 * Math.Log(value - 10) - 305.0447927307)).ToByte();

        return ((byte)red, (byte)green, (byte)blue);
    }
}
