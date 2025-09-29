/*
 * Copyright 2023 Ho Tzin Mein
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

using ChromaFx.Core.ExtensionMethods;
using ChromaFx.Processing.Filters.ColorMatrix.BaseClasses;
using ChromaFx.Processing.Numerics;

namespace ChromaFx.Processing.Filters.ColorMatrix;

/// <summary>
/// Hue matrix
/// </summary>
/// <seealso cref="MatrixBaseClass" />
public class Temperature : MatrixBaseClass
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Temperature"/> class.
    /// Original implementation https://tannerhelland.com/2012/09/18/convert-temperature-rgb-algorithm-code.html
    /// C# port https://gist.github.com/ibober/6b5a6e1dea888c01c0af175e71b15fa4
    /// </summary>
    /// <param name="value">The temperature value (1000 to 40000).</param>
    public Temperature(double value)
    {
        // Temperature must fit between 1000 and 40000 degrees.
        value = Math.Clamp(value, 1000, 40000);

        // All calculations require temperature / 100, so only do the conversion once.
        value /= 100;

        // Compute each color in turn.
        int red, green, blue;

        // First: red.
        if (value <= 66)
        {
            red = 255;
        }
        else
        {
            // Note: the R-squared value for this approximation is 0.988.
            red = (int)(329.698727446 * Math.Pow(value - 60, -0.1332047592));
            red = red.ToByte();
        }

        // Second: green.
        if (value <= 66)
        {
            // Note: the R-squared value for this approximation is 0.996.
            green = (int)(99.4708025861 * Math.Log(value) - 161.1195681661);
        }
        else
        {
            // Note: the R-squared value for this approximation is 0.987.
            green = (int)(288.1221695283 * Math.Pow(value - 60, -0.0755148492));
        }

        green = green.ToByte();

        switch (value)
        {
            // Third: blue.
            case >= 66:
                blue = 255;
                break;
            case <= 19:
                blue = 0;
                break;
            default:
                // Note: the R-squared value for this approximation is 0.998.
                blue = (int)(138.5177312231 * Math.Log(value - 10) - 305.0447927307);
                blue = blue.ToByte();
                break;
        }


        Matrix = new Matrix5X5(
            red / 255f, 0, 0, 0, 0, // M11, M12, M13, M14, M15
            0, green / 255f, 0, 0, 0, // M21, M22, M23, M24, M25
            0, 0, blue / 255f, 0, 0, // M31, M32, M33, M34, M35
            0, 0, 0, 1, 0, // M41, M42, M43, M44, M45
            0, 0, 0, 0, 1 // M51, M52, M53, M54, M55
        );
    }

    /// <summary>
    /// Gets the matrix.
    /// </summary>
    /// <value>The matrix.</value>
    public override Matrix5X5 Matrix { get; }

    /// <summary>
    /// Temperature presets
    /// https://expertphotography.com/color-temperature/
    /// </summary>
    public struct Temperatures
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