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
using System;

namespace ChromaFx.Colors.ColorSpaces;

/// <summary>
/// YXY color space
/// </summary>
public struct Yxy : IEquatable<Yxy>, IColorSpace
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Yxy"/> class.
    /// </summary>
    /// <param name="y1">The y1.</param>
    /// <param name="x">The x.</param>
    /// <param name="y2">The y2.</param>
    public Yxy(double y1, double x, double y2)
    {
        Y2 = y2;
        X = x;
        Y1 = y1;
    }

    /// <summary>
    /// Gets or sets the x.
    /// </summary>
    /// <value>The x.</value>
    public double X { get; set; }

    /// <summary>
    /// Gets or sets the y1.
    /// </summary>
    /// <value>The y1.</value>
    public double Y1 { get; set; }

    /// <summary>
    /// Gets or sets the y2.
    /// </summary>
    /// <value>The y2.</value>
    public double Y2 { get; set; }

    /// <summary>
    /// The epsilon
    /// </summary>
    private const float Epsilon = 0.001f;

    /// <summary>
    /// Performs an implicit conversion from <see cref="Yxy"/> to <see cref="Color"/>.
    /// </summary>
    /// <param name="color">The color.</param>
    /// <returns>The result of the conversion.</returns>
    public static implicit operator Color(Yxy color)
    {
        return (Xyz)color;
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="Yxy"/> to <see cref="Xyz"/>.
    /// </summary>
    /// <param name="color">The color.</param>
    /// <returns>The result of the conversion.</returns>
    public static implicit operator Xyz(Yxy color)
    {
        return new Xyz(color.X * (color.Y1 / color.Y2),
            color.Y1,
            (1.0 - color.X - color.Y2) * (color.Y1 / color.Y2));
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="Xyz"/> to <see cref="Yxy"/>.
    /// </summary>
    /// <param name="color">The color.</param>
    /// <returns>The result of the conversion.</returns>
    public static implicit operator Yxy(Xyz color)
    {
        var xyz = color;
        var xDividend = xyz.X + xyz.Y + xyz.Z;
        var y2Dividend = xyz.X + xyz.Y + xyz.Z;
        return new Yxy(xyz.Y,
            Math.Abs(xDividend) < Epsilon ? 0.0 : xyz.X / xDividend,
            Math.Abs(y2Dividend) < Epsilon ? 0.0 : xyz.Y / (xyz.X + xyz.Y + xyz.Z));
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="Color"/> to <see cref="Yxy"/>.
    /// </summary>
    /// <param name="color">The color.</param>
    /// <returns>The result of the conversion.</returns>
    public static implicit operator Yxy(Color color)
    {
        return (Xyz)color;
    }

    /// <summary>
    /// Implements the operator !=.
    /// </summary>
    /// <param name="color1">The color1.</param>
    /// <param name="color2">The color2.</param>
    /// <returns>The result of the operator.</returns>
        public static bool operator !=(Yxy color1, Yxy color2)
    {
        return !(color1 == color2);
    }

    /// <summary>
    /// Implements the operator ==.
    /// </summary>
    /// <param name="color1">The color1.</param>
    /// <param name="color2">The color2.</param>
    /// <returns>The result of the operator.</returns>
        public static bool operator ==(Yxy color1, Yxy color2)
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
        return obj is Yxy yxy && Equals(yxy);
    }

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
    /// </returns>
        public readonly bool Equals(Yxy other)
    {
        return Math.Abs(other.X - X) < Epsilon
               && Math.Abs(other.Y1 - Y1) < Epsilon
               && Math.Abs(other.Y2 - Y2) < Epsilon;
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
        var hash = X.GetHashCode();
        hash = ComputeHash(hash, Y1);
        return ComputeHash(hash, Y2);
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents this instance.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents this instance.</returns>
    public readonly override string ToString() => $"({Y1:#0.##},{X:#0.##},{Y2:#0.##})";

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