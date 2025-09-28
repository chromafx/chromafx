/*
Copyright 2025 Ho Tzin Mein

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using ChromaFx.Core.Colors.ColorSpaces.Interfaces;
using ChromaFx.Core.ExtensionMethods;

namespace ChromaFx.Core.Colors.ColorSpaces;

/// <summary>
/// LUV color space
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="Cieluv"/> class.
/// </remarks>
/// <param name="l">The l.</param>
/// <param name="u">The u.</param>
/// <param name="v">The v.</param>
public struct Cieluv(double l, double u, double v) : IEquatable<Cieluv>, IColorSpace
{

    /// <summary>
    /// Gets or sets the l.
    /// </summary>
    /// <value>The l.</value>
    public double L { get; set; } = l;

    /// <summary>
    /// Gets or sets the u.
    /// </summary>
    /// <value>The u.</value>
    public double U { get; set; } = u;

    /// <summary>
    /// Gets or sets the v.
    /// </summary>
    /// <value>The v.</value>
    public double V { get; set; } = v;

    /// <summary>
    /// The epsilon
    /// </summary>
    private const float Epsilon = 0.001f;

    /// <summary>
    /// Intent is 24389/27
    /// </summary>
    private const double Kappa = 903.3;

    /// <summary>
    /// Intent is 216/24389
    /// </summary>
    private const double XyzEpsilon = 0.008856;

    /// <summary>
    /// Performs an implicit conversion from <see cref="Color"/> to <see cref="Cieluv"/>.
    /// </summary>
    /// <param name="color">The color.</param>
    /// <returns>The result of the conversion.</returns>
    public static implicit operator Cieluv(Color color)
    {
        return (Xyz)color;
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="Xyz"/> to <see cref="Cieluv"/>.
    /// </summary>
    /// <param name="color">The color.</param>
    /// <returns>The result of the conversion.</returns>
    public static implicit operator Cieluv(Xyz color)
    {
        var white = Xyz.WhiteReference;

        var y = color.Y / white.Y;
        var l = y > XyzEpsilon ? 116.0 * y.CubicRoot() - 16.0 : Kappa * y;

        var targetDenominator = GetDenominator(color);
        var referenceDenominator = GetDenominator(white);
        var xTarget = targetDenominator == 0 ? 0 : 4.0 * color.X / targetDenominator - 4.0 * white.X / referenceDenominator;
        var yTarget = targetDenominator == 0 ? 0 : 9.0 * color.Y / targetDenominator - 9.0 * white.Y / referenceDenominator;
        var u = 13.0 * l * xTarget;
        var v = 13.0 * l * yTarget;
        return new Cieluv(l, u, v);
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="Cieluv"/> to <see cref="Color"/>.
    /// </summary>
    /// <param name="color">The color.</param>
    /// <returns>The result of the conversion.</returns>
    public static implicit operator Color(Cieluv color)
    {
        return (Xyz)color;
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="Cieluv"/> to <see cref="Xyz"/>.
    /// </summary>
    /// <param name="color">The color.</param>
    /// <returns>The result of the conversion.</returns>
    public static implicit operator Xyz(Cieluv color)
    {
        var white = Xyz.WhiteReference;
        var c = -1.0 / 3.0;
        var uPrime = 4.0 * white.X / GetDenominator(white);
        var vPrime = 9.0 * white.Y / GetDenominator(white);
        var a = 1.0 / 3.0 * (52.0 * color.L / (color.U + 13 * color.L * uPrime) - 1.0);
        var imteL16116 = (color.L + 16.0) / 116.0;
        var y = color.L > Kappa * XyzEpsilon
            ? imteL16116 * imteL16116 * imteL16116
            : color.L / Kappa;
        var b = -5.0 * y;
        var d = y * (39.0 * color.L / (color.V + 13.0 * color.L * vPrime) - 5.0);
        var x = (d - b) / (a - c);
        var z = x * a + b;
        return new Xyz(100 * x, 100 * y, 100 * z);
    }

    /// <summary>
    /// Implements the operator !=.
    /// </summary>
    /// <param name="color1">The color1.</param>
    /// <param name="color2">The color2.</param>
    /// <returns>The result of the operator.</returns>
        public static bool operator !=(Cieluv color1, Cieluv color2)
    {
        return !(color1 == color2);
    }

    /// <summary>
    /// Implements the operator ==.
    /// </summary>
    /// <param name="color1">The color1.</param>
    /// <param name="color2">The color2.</param>
    /// <returns>The result of the operator.</returns>
        public static bool operator ==(Cieluv color1, Cieluv color2)
    {
        return color1.Equals(color2);
    }

    /// <summary>
    /// Determines whether the specified <see cref="object"/>, is equal to this instance.
    /// </summary>
    /// <param name="obj">The <see cref="object"/> to compare with this instance.</param>
    /// <returns>
    /// <c>true</c> if the specified <see cref="object"/> is equal to this instance; otherwise, <c>false</c>.
    /// </returns>
        public readonly override bool Equals(object obj)
    {
        return obj is Cieluv cieluv && Equals(cieluv);
    }

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
    /// </returns>
        public readonly bool Equals(Cieluv other)
    {
        return Math.Abs(other.L - L) < Epsilon
               && Math.Abs(other.U - U) < Epsilon
               && Math.Abs(other.V - V) < Epsilon;
    }

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>
    /// A hash code for this instance, suitable for use in hashing algorithms and data structures
    /// like a hash table.
    /// </returns>
    public readonly override int GetHashCode()
    {
        var hash = L.GetHashCode();
        hash = ComputeHash(hash, U);
        return ComputeHash(hash, V);
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents this instance.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents this instance.</returns>
    public readonly override string ToString() => $"({L:#0.##},{U:#0.##},{V:#0.##})";

    /// <summary>
    /// Gets the denominator.
    /// </summary>
    /// <param name="xyz">The xyz.</param>
    /// <returns>The denominator</returns>
    private static double GetDenominator(Xyz xyz)
    {
        return xyz.X + 15.0 * xyz.Y + 3.0 * xyz.Z;
    }

    /// <summary>
    /// Computes the hash.
    /// </summary>
    /// <param name="hash">The existing hash.</param>
    /// <param name="component">The component.</param>
    /// <returns>The resulting hash</returns>
    private static int ComputeHash(int hash, double component)
    {
        return ((hash << 5) + hash) ^ component.GetHashCode();
    }
}