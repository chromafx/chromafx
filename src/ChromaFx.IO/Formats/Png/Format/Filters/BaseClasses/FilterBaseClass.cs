using ChromaFx.IO.Formats.Png.Format.Filters.Interfaces;

namespace ChromaFx.IO.Formats.Png.Format.Filters.BaseClasses;

/// <summary>
/// Scanline filter base class
/// </summary>
/// <seealso cref="IScanFilter"/>
public abstract class FilterBaseClass : IScanFilter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FilterBaseClass"/> class.
    /// </summary>
    protected FilterBaseClass()
    {
    }

    /// <summary>
    /// Gets the filter value.
    /// </summary>
    /// <value>The filter value.</value>
    public abstract byte FilterValue { get; }

    /// <summary>
    /// Decodes the specified scanline.
    /// </summary>
    /// <param name="scanline">The scanline.</param>
    /// <param name="previousScanline">The previous scanline.</param>
    /// <param name="scanlineStep">The scanline step.</param>
    /// <returns>The resulting byte array</returns>
    public byte[] Decode(byte[] scanline, byte[] previousScanline, int scanlineStep)
    {
        var result = new byte[scanline.Length];
        for (var x = 0; x < result.Length; ++x)
        {
            var left = (x >= scanlineStep) ? result[x - scanlineStep] : (byte)0;
            var above = previousScanline[x];
            var upperLeft = (x >= scanlineStep) ? previousScanline[x - scanlineStep] : (byte)0;

            result[x] = (byte)(scanline[x] + Calculate(left, above, upperLeft));
        }
        return result;
    }

    /// <summary>
    /// Encodes the specified scanline.
    /// </summary>
    /// <param name="scanline">The scanline.</param>
    /// <param name="previousScanline">The previous scanline.</param>
    /// <param name="scanlineStep">The scanline step.</param>
    /// <returns>The resulting byte array</returns>
    public byte[] Encode(byte[] scanline, byte[] previousScanline, int scanlineStep)
    {
        var result = new byte[scanline.Length + 1];
        result[0] = FilterValue;
        for (var x = 0; x < scanline.Length; ++x)
        {
            var left = (x >= scanlineStep) ? result[x - scanlineStep] : (byte)0;
            var above = previousScanline[x];
            var upperLeft = (x >= scanlineStep) ? previousScanline[x - scanlineStep] : (byte)0;

            result[x + 1] = (byte)(scanline[x] - Calculate(left, above, upperLeft));
        }

        return result;
    }

    /// <summary>
    /// Calculates the value to add based on the left, up, and upper left bytes.
    /// </summary>
    /// <param name="left">The left byte.</param>
    /// <param name="above">The above byte.</param>
    /// <param name="upperLeft">The upper left byte.</param>
    /// <returns>The resulting byte.</returns>
    protected abstract byte Calculate(byte left, byte above, byte upperLeft);
}