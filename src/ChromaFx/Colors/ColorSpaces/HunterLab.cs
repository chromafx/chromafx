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

namespace ChromaFx.Colors.ColorSpaces;

/// <summary>
/// LAB color space
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="HunterLab"/> struct.
/// </remarks>
/// <param name="l">The l.</param>
/// <param name="a">a.</param>
/// <param name="b">The b.</param>
public struct HunterLab(double l, double a, double b) : IEquatable<HunterLab>, IColorSpace
{

    /// <summary>
    /// Gets or sets a.
    /// </summary>
    /// <value>a.</value>
    public double A { get; set; } = a;

    /// <summary>
    /// Gets or sets the b.
    /// </summary>
    /// <value>The b.</value>
    public double B { get; set; } = b;

    /// <summary>
    /// Gets or sets the l.
    /// </summary>
    /// <value>The l.</value>
    public double L { get; set; } = l;

    /// <summary>
    /// The epsilon
    /// </summary>
    private const float Epsilon = 0.001f;

    /// <summary>
    /// Performs an implicit conversion from <see cref="HunterLab"/> to <see cref="Color"/>.
    /// </summary>
    /// <param name="color">The color.</param>
    /// <returns>The result of the conversion.</returns>
    public static implicit operator Color(HunterLab color)
    {
        return (Xyz)color;
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="Color"/> to <see cref="HunterLab"/>.
    /// </summary>
    /// <param name="color">The color.</param>
    /// <returns>The result of the conversion.</returns>
    public static implicit operator HunterLab(Color color)
    {
        return (Xyz)color;
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="Xyz"/> to <see cref="HunterLab"/>.
    /// </summary>
    /// <param name="color">The color.</param>
    /// <returns>The result of the conversion.</returns>
    public static implicit operator HunterLab(Xyz color)
    {
        var l = 10.0 * Math.Sqrt(color.Y);
        var a = color.Y != 0 ? 17.5 * ((1.02 * color.X - color.Y) / Math.Sqrt(color.Y)) : 0;
        var b = color.Y != 0 ? 7.0 * ((color.Y - .847 * color.Z) / Math.Sqrt(color.Y)) : 0;
        return new HunterLab(l, a, b);
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="HunterLab"/> to <see cref="Xyz"/>.
    /// </summary>
    /// <param name="color">The color.</param>
    /// <returns>The result of the conversion.</returns>
    public static implicit operator Xyz(HunterLab color)
    {
        var x = color.A / 17.5 * (color.L / 10.0);
        var itemL10 = color.L / 10.0;
        var y = itemL10 * itemL10;
        var z = color.B / 7.0 * color.L / 10.0;

        return new Xyz((x + y) / 1.02,
            y,
            -(z - y) / .847
        );
    }

    /// <summary>
    /// Implements the operator !=.
    /// </summary>
    /// <param name="color1">The color1.</param>
    /// <param name="color2">The color2.</param>
    /// <returns>The result of the operator.</returns>
        public static bool operator !=(HunterLab color1, HunterLab color2)
    {
        return !(color1 == color2);
    }

    /// <summary>
    /// Implements the operator ==.
    /// </summary>
    /// <param name="color1">The color1.</param>
    /// <param name="color2">The color2.</param>
    /// <returns>The result of the operator.</returns>
        public static bool operator ==(HunterLab color1, HunterLab color2)
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
        return obj is HunterLab lab && Equals(lab);
    }

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
    /// </returns>
        public readonly bool Equals(HunterLab other)
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