using ChromaFx.Core;

namespace ChromaFx.Processing.Numerics;

/// <summary>
/// Extension methods for the <see cref="Rectangle"/> struct, providing helpers for image operations.
/// </summary>
public static class RectangleExtensions
{
    /// <summary>
    /// Normalizes the target location rectangle for an image operation.
    /// If the rectangle is default, returns the full image rectangle; otherwise clamps to image bounds.
    /// </summary>
    /// <param name="rect">The rectangle to normalize.</param>
    /// <param name="image">The image to use for bounds.</param>
    /// <returns>A normalized rectangle.</returns>
    public static Rectangle Normalize(this Rectangle rect, Image image)
    {
        return rect == default
            ? new Rectangle(0, 0, image.Width, image.Height)
            : rect.Clamp(image);
    }
}
