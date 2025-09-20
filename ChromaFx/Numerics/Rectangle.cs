using System;
using System.Numerics;

namespace ChromaFx.Numerics;

/// <summary>
/// Rectangle struct, holds 4 points indicating a rectangular region.
/// </summary>
public struct Rectangle : IEquatable<Rectangle>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Rectangle"/> struct.
    /// </summary>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    public Rectangle(int x, int y, int width, int height)
        : this(new Vector4(x, y, x + width, y + height))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Rectangle"/> struct.
    /// </summary>
    /// <param name="value">The value.</param>
    public Rectangle(Vector4 value)
    {
        Data = value;
    }

    /// <summary>
    /// Gets the bottom.
    /// </summary>
    /// <value>The bottom.</value>
    public readonly int Bottom => (int)Data.Y;

    /// <summary>
    /// Gets the center.
    /// </summary>
    /// <value>The center.</value>
    public readonly Vector2 Center => new((Left + Right) >> 1, (Top + Bottom) >> 1);

    /// <summary>
    /// Gets the height.
    /// </summary>
    /// <value>The height.</value>
    public readonly int Height => (int)(Data.W - Data.Y);

    /// <summary>
    /// Gets the left.
    /// </summary>
    /// <value>The left.</value>
    public readonly int Left => (int)Data.X;

    /// <summary>
    /// Gets the right.
    /// </summary>
    /// <value>The right.</value>
    public readonly int Right => (int)Data.Z;

    /// <summary>
    /// Gets the top.
    /// </summary>
    /// <value>The top.</value>
    public readonly int Top => (int)Data.W;

    /// <summary>
    /// Gets the width.
    /// </summary>
    /// <value>The width.</value>
    public readonly int Width => (int)(Data.Z - Data.X);

    /// <summary>
    /// Gets or sets the data.
    /// </summary>
    /// <value>The data.</value>
    private Vector4 Data { get; set; }

    /// <summary>
    /// Implements the operator !=.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <returns>The result of the operator.</returns>
        public static bool operator !=(Rectangle left, Rectangle right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Implements the operator ==.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <returns>The result of the operator.</returns>
        public static bool operator ==(Rectangle left, Rectangle right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Clamps based on the specified image.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <returns>The resulting clamped rectangle</returns>
        public Rectangle Clamp(Image image)
    {
        Data = new Vector4(Data.X < 0 ? 0 : Data.X,
            Data.Y < 0 ? 0 : Data.Y,
            Data.Z > image.Width ? image.Width : Data.Z,
            Data.W > image.Height ? image.Height : Data.W);
        return this;
    }

    /// <summary>
    /// Clamps based on the specified Rectangle.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <returns>The resulting clamped rectangle</returns>
        public Rectangle Clamp(Rectangle image)
    {
        Data = new Vector4(Data.X < image.Left ? image.Left : Data.X,
            Data.Y < image.Bottom ? image.Bottom : Data.Y,
            Data.Z > image.Right ? image.Right : Data.Z,
            Data.W > image.Top ? image.Top : Data.W);
        return this;
    }

    /// <summary>
    /// Determines whether [contains] [the specified x and y coordinate].
    /// </summary>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <returns>True if it does, false otherwise</returns>
        public readonly bool Contains(int x, int y)
    {
        return Left <= x
               && Right > x
               && Top > y
               && Bottom <= y;
    }

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
    /// </returns>
        public readonly bool Equals(Rectangle other)
    {
        return other.Data == Data;
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
        return obj is Rectangle rectangle && Equals(rectangle);
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
        return Data.GetHashCode();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents this instance.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents this instance.</returns>
    public readonly override string ToString()
    {
        return $"{{ {Data.X}, {Data.Y}, {Data.Z}, {Data.W} }}";
    }
}