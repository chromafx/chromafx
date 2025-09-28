using ChromaFx.IO.Formats.Png.Format.Enums;
using ChromaFx.IO.Formats.Png.Format.Filters.BaseClasses;

namespace ChromaFx.IO.Formats.Png.Format.Filters;

/// <summary>
/// Adds the pixel above
/// </summary>
/// <seealso cref="FilterBaseClass"/>
public class UpFilter : FilterBaseClass
{
    /// <summary>
    /// Gets the filter value.
    /// </summary>
    /// <value>The filter value.</value>
    public override byte FilterValue => (byte)FilterType.Up;

    /// <summary>
    /// Calculates the value to add based on the left, up, and upper left bytes.
    /// </summary>
    /// <param name="left">The left byte.</param>
    /// <param name="above">The above byte.</param>
    /// <param name="upperLeft">The upper left byte.</param>
    /// <returns>The resulting byte.</returns>
    protected override byte Calculate(byte left, byte above, byte upperLeft)
    {
        return above;
    }
}