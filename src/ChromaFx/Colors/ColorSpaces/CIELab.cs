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

using ChromaFx.Colors.ColorSpaces.Interfaces;
using ChromaFx.ExtensionMethods;

namespace ChromaFx.Colors.ColorSpaces;

/// <summary>
/// CIELAB
/// </summary>
/// <seealso cref="IColorSpace"/>
/// <seealso cref="IEquatable{CIELab}"/>
/// <remarks>
/// Initializes a new instance of the <see cref="CieLab"/> struct.
/// </remarks>
/// <param name="lightness">The lightness.</param>
/// <param name="aComponent">The A component.</param>
/// <param name="bComponent">The B component.</param>
public struct CieLab(double lightness, double aComponent, double bComponent) : IEquatable<CieLab>, IColorSpace
{

    /// <summary>
    /// Gets or sets a component.
    /// </summary>
    /// <value>a component.</value>
    public double A = Math.Clamp(aComponent, -100, 100);

    /// <summary>
    /// Gets or sets the b component.
    /// </summary>
    /// <value>The b component.</value>
    public double B = Math.Clamp(bComponent, -100, 100);

    /// <summary>
    /// Gets or sets the lightness.
    /// </summary>
    /// <value>The lightness.</value>
    public double L = Math.Clamp(lightness, 0, 100);

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
    /// Performs an implicit conversion from <see cref="Color"/> to <see cref="CieLab"/>.
    /// </summary>
    /// <param name="color">The color.</param>
    /// <returns>The result of the conversion.</returns>
    public static implicit operator CieLab(Color color)
    {
        return (Xyz)color;
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="Xyz"/> to <see cref="CieLab"/>.
    /// </summary>
    /// <param name="color">The color.</param>
    /// <returns>The result of the conversion.</returns>
    public static implicit operator CieLab(Xyz color)
    {
        var white = Xyz.WhiteReference;
        var x = PivotXyz(color.X / white.X);
        var y = PivotXyz(color.Y / white.Y);
        var z = PivotXyz(color.Z / white.Z);

        return new CieLab(
            (float)Math.Max(0, 116 * y - 16),
            (float)(500 * (x - y)),
            (float)(200 * (y - z))
        );
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="CieLab"/> to <see cref="Color"/>.
    /// </summary>
    /// <param name="color">The color.</param>
    /// <returns>The result of the conversion.</returns>
    public static implicit operator Color(CieLab color)
    {
        return (Xyz)color;
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="CieLab"/> to <see cref="Xyz"/>.
    /// </summary>
    /// <param name="color">The color.</param>
    /// <returns>The result of the conversion.</returns>
    public static implicit operator Xyz(CieLab color)
    {
        var y = (color.L + 16d) / 116d;
        var x = color.A / 500d + y;
        var z = y - color.B / 200d;
        var white = Xyz.WhiteReference;

        var x3 = x * x * x;
        var z3 = z * z * z;

        return new Xyz(
            white.X * (x3 > XyzEpsilon ? x3 : (x - 16.0 / 116.0) / 7.787),
            white.Y
                * (
                    color.L > Kappa * XyzEpsilon
                        ? Math.Pow((color.L + 16.0) / 116.0, 3)
                        : color.L / Kappa
                ),
            white.Z * (z3 > XyzEpsilon ? z3 : (z - 16.0 / 116.0) / 7.787)
        );
    }

    /// <summary>
    /// Implements the operator !=.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <returns>The result of the operator.</returns>
    public static bool operator !=(CieLab left, CieLab right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Implements the operator ==.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <returns>The result of the operator.</returns>
    public static bool operator ==(CieLab left, CieLab right)
    {
        return left.Equals(right);
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
        return obj is CieLab lab && Equals(lab);
    }

    /// <summary>
    /// Determines if the items are equal
    /// </summary>
    /// <param name="other">The other CIELab color.</param>
    /// <returns>True if they are, false otherwise</returns>
    public readonly bool Equals(CieLab other)
    {
        return Math.Abs(other.L - L) < Epsilon
            && Math.Abs(other.A - A) < Epsilon
            && Math.Abs(other.B - B) < Epsilon;
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
        hash = ComputeHash(hash, A);
        return ComputeHash(hash, B);
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents this instance.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents this instance.</returns>
    public readonly override string ToString() => $"({L:#0.##},{A:#0.##},{B:#0.##})";

    /// <summary>
    /// Pivots to xyz.
    /// </summary>
    /// <param name="n">The value.</param>
    /// <returns>The resulting value</returns>
    private static double PivotXyz(double n)
    {
        return n > XyzEpsilon ? n.CubicRoot() : (Kappa * n + 16) / 116;
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
