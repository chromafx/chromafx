namespace ChromaFx.IO.Formats.Png.Format.Filters;

/// <summary>
/// Scan line filter interface for scan
/// </summary>
internal interface IScanFilter
{
    /// <summary>
    /// Decodes the specified scanline.
    /// </summary>
    /// <param name="scanline">The scanline.</param>
    /// <param name="previousScanline">The previous scanline.</param>
    /// <param name="scanlineStep">The scanline step.</param>
    /// <returns>The resulting byte array</returns>
    byte[] Decode(byte[] scanline, byte[] previousScanline, int scanlineStep);

    /// <summary>
    /// Encodes the specified scanline.
    /// </summary>
    /// <param name="scanline">The scanline.</param>
    /// <param name="previousScanline">The previous scanline.</param>
    /// <param name="scanlineStep">The scanline step.</param>
    /// <returns>The resulting byte array</returns>
    byte[] Encode(byte[] scanline, byte[] previousScanline, int scanlineStep);
}
