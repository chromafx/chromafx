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

namespace ChromaFx.Core.Colors.ColorSpaces;

/// <summary>
/// BGRA color space
/// </summary>
/// <seealso cref="IColorSpace"/>
/// <seealso cref="IEquatable{Bgra}"/>
/// <remarks>
/// Initializes a new instance of the <see cref="Bgra"/> struct.
/// </remarks>
/// <param name="blue">The blue.</param>
/// <param name="green">The green.</param>
/// <param name="red">The red.</param>
/// <param name="alpha">The alpha.</param>
public struct Bgra(byte blue, byte green, byte red, byte alpha = 255)
    : IEquatable<Bgra>,
        IColorSpace
{
    /// <summary>
    /// Gets or sets the alpha.
    /// </summary>
    /// <value>The alpha.</value>
    public byte Alpha { get; set; } = Math.Clamp(alpha, (byte)0, (byte)255);

    /// <summary>
    /// Gets or sets the blue.
    /// </summary>
    /// <value>The blue.</value>
    public byte Blue { get; set; } = Math.Clamp(blue, (byte)0, (byte)255);

    /// <summary>
    /// Gets or sets the green.
    /// </summary>
    /// <value>The green.</value>
    public byte Green { get; set; } = Math.Clamp(green, (byte)0, (byte)255);

    /// <summary>
    /// Gets or sets the red.
    /// </summary>
    /// <value>The red.</value>
    public byte Red { get; set; } = Math.Clamp(red, (byte)0, (byte)255);

    /// <summary>
    /// Performs an implicit conversion from <see cref="Color"/> to <see cref="Bgra"/>.
    /// </summary>
    /// <param name="color">The color.</param>
    /// <returns>The result of the conversion.</returns>
    public static implicit operator Bgra(Color color)
    {
        return new Bgra(color.Blue, color.Green, color.Red, color.Alpha);
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="Bgra"/> to <see cref="Color"/>.
    /// </summary>
    /// <param name="color">The color.</param>
    /// <returns>The result of the conversion.</returns>
    public static implicit operator Color(Bgra color)
    {
        return new Color(color.Red, color.Green, color.Blue, color.Alpha);
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="Bgra"/> to <see cref="int"/>.
    /// </summary>
    /// <param name="color">The color.</param>
    /// <returns>The result of the conversion.</returns>
    public static implicit operator int(Bgra color)
    {
        return color.Red << 16 | color.Green << 8 | color.Blue << 0 | color.Alpha << 24;
    }

    /// <summary>
    /// Implements the operator !=.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <returns>The result of the operator.</returns>
    public static bool operator !=(Bgra left, Bgra right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Implements the operator ==.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <returns>The result of the operator.</returns>
    public static bool operator ==(Bgra left, Bgra right)
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
        return obj is Bgra bgra && Equals(bgra);
    }

    /// <summary>
    /// Determines if the items are equal
    /// </summary>
    /// <param name="other">The other Bgra color.</param>
    /// <returns>True if they are, false otherwise</returns>
    public readonly bool Equals(Bgra other)
    {
        return other.Blue == Blue
            && other.Green == Green
            && other.Red == Red
            && other.Alpha == Alpha;
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
        var hash = Blue.GetHashCode();
        hash = ComputeHash(hash, Green);
        hash = ComputeHash(hash, Red);
        return ComputeHash(hash, Alpha);
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents this instance.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents this instance.</returns>
    public readonly override string ToString() =>
        $"({Blue:#0.##},{Green:#0.##},{Red:#0.##},{Alpha:#0.##})";

    /// <summary>
    /// Computes the hash.
    /// </summary>
    /// <param name="hash">The existing hash.</param>
    /// <param name="component">The component.</param>
    /// <returns>The resulting hash</returns>
    private static int ComputeHash(int hash, byte component)
    {
        return ((hash << 5) + hash) ^ component.GetHashCode();
    }
}
