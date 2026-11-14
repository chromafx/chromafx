/*
 * Copyright 2023–2026 Ho Tzin Mein
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

using ChromaFx.Processing.Filters.ColorMatrix.BaseClasses;
using ChromaFx.Processing.Numerics;

namespace ChromaFx.Processing.Filters.ColorMatrix;

/// <summary>
/// Tints the image toward a blackbody illuminant at the given Kelvin temperature
/// (Helland approximation). This colorizes toward the light's color; it is not
/// photographic white-balance correction.
/// </summary>
/// <seealso cref="MatrixBaseClass" />
public class KelvinTint : MatrixBaseClass
{
    /// <summary>
    /// Initializes a new instance of the <see cref="KelvinTint"/> class.
    /// Original implementation https://tannerhelland.com/2012/09/18/convert-temperature-rgb-algorithm-code.html
    /// C# port https://gist.github.com/ibober/6b5a6e1dea888c01c0af175e71b15fa4
    /// </summary>
    /// <param name="kelvin">Illuminant temperature in Kelvin (1000 to 40000).</param>
    public KelvinTint(double kelvin)
    {
        var (red, green, blue) = WhiteBalance.HellandRgb(kelvin);
        Matrix = new Matrix5X5(
            red / 255f,
            0,
            0,
            0,
            0,
            0,
            green / 255f,
            0,
            0,
            0,
            0,
            0,
            blue / 255f,
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

    /// <summary>
    /// Common illuminant Kelvin presets.
    /// https://expertphotography.com/color-temperature/
    /// </summary>
    public static class Presets
    {
        public const double CandleLight = 1800;
        public const double Twilight = 2500;
        public const double Moonlight = 4000;
        public const double SunAtNoon = 5500;
        public const double CloudySky = 6500;
        public const double OutdoorShade = 7000;
        public const double ClearBlueSky = 10000;
    }
}
