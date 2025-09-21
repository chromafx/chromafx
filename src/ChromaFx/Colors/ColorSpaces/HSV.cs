using ChromaFx.Colors.ColorSpaces.Interfaces;
using ChromaFx.ExtensionMethods;

namespace ChromaFx.Colors.ColorSpaces;

/// <summary>
/// HSV color space
/// </summary>
/// <seealso cref="System.IEquatable{HSV}"/>
/// <seealso cref="IColorSpace"/>
/// <remarks>
/// Initializes a new instance of the <see cref="Hsv"/> struct.
/// </remarks>
/// <param name="hue">The hue.</param>
/// <param name="saturation">The saturation.</param>
/// <param name="value">The value.</param>
public struct Hsv(double hue, double saturation, double value) : IEquatable<Hsv>, IColorSpace
{

    /// <summary>
    /// Gets or sets the hue.
    /// </summary>
    /// <value>The hue.</value>
    public double Hue = hue;

    /// <summary>
    /// Gets or sets the saturation.
    /// </summary>
    /// <value>The saturation.</value>
    public double Saturation = saturation;

    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    /// <value>The value.</value>
    public double Value = value;

    /// <summary>
    /// The epsilon value for double comparison
    /// </summary>
    private static readonly double Epsilon = 0.01d;

    /// <summary>
    /// Performs an implicit conversion from <see cref="Hsv"/> to <see cref="Color"/>.
    /// </summary>
    /// <param name="color">The color.</param>
    /// <returns>The result of the conversion.</returns>
    public static implicit operator Color(Hsv color)
    {
        double red;
        double green;
        double blue;
        if (Math.Abs(color.Saturation) < Epsilon)
        {
            red = color.Value;
            green = color.Value;
            blue = color.Value;
        }
        else
        {
            var a = color.Hue / 360d * 6;
            var b = Math.Floor(a);
            var e = color.Value * (1 - color.Saturation);
            var f = color.Value * (1 - color.Saturation * (a - b));
            var g = color.Value * (1 - color.Saturation * (1 - (a - b)));

            switch ((int)b)
            {
                case 6:
                case 0:
                    red = color.Value;
                    green = g;
                    blue = e;
                    break;
                case 1:
                    red = f;
                    green = color.Value;
                    blue = e;
                    break;
                case 2:
                    red = e;
                    green = color.Value;
                    blue = g;
                    break;
                case 3:
                    red = e;
                    green = f;
                    blue = color.Value;
                    break;
                case 4:
                    red = g;
                    green = e;
                    blue = color.Value;
                    break;
                default:
                    red = color.Value;
                    green = e;
                    blue = f;
                    break;
            }
        }

        return new Color(
            (red * 255).ToByte(),
            (green * 255).ToByte(),
            (blue * 255).ToByte()
        );
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="Color"/> to <see cref="Hsv"/>.
    /// </summary>
    /// <param name="color">The color.</param>
    /// <returns>The result of the conversion.</returns>
    public static implicit operator Hsv(Color color)
    {
        double hue = 0;
        double max = Math.Max(color.Red, Math.Max(color.Green, color.Blue));
        double min = Math.Min(color.Red, Math.Min(color.Green, color.Blue));
        double value;
        if (Math.Abs(min - max) < Epsilon)
        {
            value = min;
        }
        else
        {
            double c;
            if (Math.Abs(color.Red - min) < Epsilon)
                c = color.Green - color.Blue;
            else if (Math.Abs(color.Blue - min) < Epsilon)
                c = color.Red - color.Green;
            else
                c = color.Blue - color.Red;
            double d;
            if (Math.Abs(color.Red - min) < Epsilon)
                d = 3;
            else if (Math.Abs(color.Blue - min) < Epsilon)
                d = 1;
            else
                d = 5;
            hue = 60d * (d - c / (max - min));
            value = max;
        }
        var saturation = Math.Abs(max) < Epsilon ? 0 : (max - min) / max;
        return new Hsv(hue, saturation, value / 255d);
    }

    /// <summary>
    /// Implements the operator !=.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <returns>The result of the operator.</returns>
    public static bool operator !=(Hsv left, Hsv right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Implements the operator ==.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <returns>The result of the operator.</returns>
    public static bool operator ==(Hsv left, Hsv right)
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
        return obj is Hsv hsv && Equals(hsv);
    }

    /// <summary>
    /// Determines if the items are equal
    /// </summary>
    /// <param name="other">The other HSV color.</param>
    /// <returns>True if they are, false otherwise</returns>
    public readonly bool Equals(Hsv other)
    {
        return Math.Abs(other.Hue - Hue) < Epsilon
            && Math.Abs(other.Saturation - Saturation) < Epsilon
            && Math.Abs(other.Value - Value) < Epsilon;
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
        var hash = Hue.GetHashCode();
        hash = ComputeHash(hash, Saturation);
        return ComputeHash(hash, Value);
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents this instance.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents this instance.</returns>
    public readonly override string ToString() => $"({Hue:#0.##},{Saturation:#0.##},{Value:#0.##})";

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
