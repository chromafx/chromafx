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
/// YCbCr color space
/// </summary>
/// <seealso cref="IColorSpace"/>
/// <seealso cref="IEquatable{YCbCr}"/>
/// <remarks>
/// Initializes a new instance of the <see cref="YCbCr"/> struct.
/// </remarks>
/// <param name="yLuminance">The y luminance.</param>
/// <param name="cbChroma">The cb chroma.</param>
/// <param name="crChroma">The cr chroma.</param>
public struct YCbCr(double yLuminance, double cbChroma, double crChroma) : IEquatable<YCbCr>, IColorSpace
{

    /// <summary>
    /// Gets or sets the cb chroma.
    /// </summary>
    /// <value>The cb chroma.</value>
    public double CbChroma = cbChroma;

    /// <summary>
    /// Gets or sets the cr chroma.
    /// </summary>
    /// <value>The cr chroma.</value>
    public double CrChroma = crChroma;

    /// <summary>
    /// Gets or sets the y luminance.
    /// </summary>
    /// <value>The y luminance.</value>
    public double YLuminance = yLuminance;

    /// <summary>
    /// The epsilon
    /// </summary>
    private const float Epsilon = 0.001f;

    /// <summary>
    /// Performs an implicit conversion from <see cref="YCbCr"/> to <see cref="Color"/>.
    /// </summary>
    /// <param name="color">The color.</param>
    /// <returns>The result of the conversion.</returns>
    public static implicit operator Color(YCbCr color)
    {
        var y = color.YLuminance;
        var cb = color.CbChroma - 128;
        var cr = color.CrChroma - 128;

        return new Color(Math.Round(y + 1.402 * cr, MidpointRounding.AwayFromZero).ToByte(),
            Math.Round(y - 0.344136 * cb - 0.714136 * cr, MidpointRounding.AwayFromZero).ToByte(),
            Math.Round(y + 1.772 * cb, MidpointRounding.AwayFromZero).ToByte());
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="Color"/> to <see cref="YCbCr"/>.
    /// </summary>
    /// <param name="color">The color.</param>
    /// <returns>The result of the conversion.</returns>
    public static implicit operator YCbCr(Color color)
    {
        color = color.Clamp();
        var r = color.Red;
        var g = color.Green;
        var b = color.Blue;

        return new YCbCr(0.299 * r + 0.587 * g + 0.114 * b,
            128 - 0.168736 * r - 0.331264 * g + 0.5 * b,
            128 + 0.5 * r - 0.418688 * g - 0.081312 * b);
    }

    /// <summary>
    /// Implements the operator !=.
    /// </summary>
    /// <param name="color1">The color1.</param>
    /// <param name="color2">The color2.</param>
    /// <returns>The result of the operator.</returns>
        public static bool operator !=(YCbCr color1, YCbCr color2)
    {
        return !(color1 == color2);
    }

    /// <summary>
    /// Implements the operator ==.
    /// </summary>
    /// <param name="color1">The color1.</param>
    /// <param name="color2">The color2.</param>
    /// <returns>The result of the operator.</returns>
        public static bool operator ==(YCbCr color1, YCbCr color2)
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
        return obj is YCbCr cr && Equals(cr);
    }

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
    /// </returns>
        public readonly bool Equals(YCbCr other)
    {
        return Math.Abs(other.YLuminance - YLuminance) < Epsilon
               && Math.Abs(other.CbChroma - CbChroma) < Epsilon
               && Math.Abs(other.CrChroma - CrChroma) < Epsilon;
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
        var hash = YLuminance.GetHashCode();
        hash = ComputeHash(hash, CbChroma);
        return ComputeHash(hash, CrChroma);
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents this instance.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents this instance.</returns>
    public readonly override string ToString() => $"({YLuminance:#0.##},{CbChroma:#0.##},{CrChroma:#0.##})";

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