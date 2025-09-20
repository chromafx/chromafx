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
/// LCH color space
/// </summary>
public struct Cielch : IEquatable<Cielch>, IColorSpace
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Cielch"/> struct.
    /// </summary>
    /// <param name="l">The l.</param>
    /// <param name="c">The c.</param>
    /// <param name="h">The h.</param>
    public Cielch(double l, double c, double h)
    {
        L = l;
        C = c;
        H = h;
    }

    /// <summary>
    /// Gets or sets the c.
    /// </summary>
    /// <value>The c.</value>
    public double C { get; set; }

    /// <summary>
    /// Gets or sets the h.
    /// </summary>
    /// <value>The h.</value>
    public double H { get; set; }

    /// <summary>
    /// Gets or sets the l.
    /// </summary>
    /// <value>The l.</value>
    public double L { get; set; }

    /// <summary>
    /// The epsilon
    /// </summary>
    private const float Epsilon = 0.001f;

    /// <summary>
    /// Performs an implicit conversion from <see cref="Cielch"/> to <see cref="Xyz"/>.
    /// </summary>
    /// <param name="color">The color.</param>
    /// <returns>The result of the conversion.</returns>
    public static implicit operator CieLab(Cielch color)
    {
        var hRadians = color.H * Math.PI / 180.0;
        return new CieLab(color.L, Math.Cos(hRadians) * color.C, Math.Sin(hRadians) * color.C);
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="Color"/> to <see cref="Cielch"/>.
    /// </summary>
    /// <param name="color">The color.</param>
    /// <returns>The result of the conversion.</returns>
    public static implicit operator Cielch(Color color)
    {
        return (CieLab)color;
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="CieLab"/> to <see cref="Cielch"/>.
    /// </summary>
    /// <param name="color">The color.</param>
    /// <returns>The result of the conversion.</returns>
    public static implicit operator Cielch(CieLab color)
    {
        var h = Math.Atan2(color.B, color.A);
        h = h > 0 ?
            h / Math.PI * 180.0 :
            360 - Math.Abs(h) / Math.PI * 180.0;
        if (h < 0)
        {
            h += 360.0;
        }
        else if (h >= 360)
        {
            h -= 360.0;
        }

        return new Cielch(color.L, Math.Sqrt(color.A * color.A + color.B * color.B), h);
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="Cielch"/> to <see cref="Color"/>.
    /// </summary>
    /// <param name="color">The color.</param>
    /// <returns>The result of the conversion.</returns>
    public static implicit operator Color(Cielch color)
    {
        return (CieLab)color;
    }

    /// <summary>
    /// Implements the operator !=.
    /// </summary>
    /// <param name="color1">The color1.</param>
    /// <param name="color2">The color2.</param>
    /// <returns>The result of the operator.</returns>
        public static bool operator !=(Cielch color1, Cielch color2)
    {
        return !(color1 == color2);
    }

    /// <summary>
    /// Implements the operator ==.
    /// </summary>
    /// <param name="color1">The color1.</param>
    /// <param name="color2">The color2.</param>
    /// <returns>The result of the operator.</returns>
        public static bool operator ==(Cielch color1, Cielch color2)
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
        return obj is Cielch cielch && Equals(cielch);
    }

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
    /// </returns>
        public readonly bool Equals(Cielch other)
    {
        return Math.Abs(other.L - L) < Epsilon
               && Math.Abs(other.C - C) < Epsilon
               && Math.Abs(other.H - H) < Epsilon;
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
        hash = ComputeHash(hash, C);
        return ComputeHash(hash, H);
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents this instance.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents this instance.</returns>
    public readonly override string ToString() => $"({L:#0.##},{C:#0.##},{H:#0.##})";

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