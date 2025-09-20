using ChromaFx.Filters.Arithmetic;
using ChromaFx.Filters.ColorMatrix;
using ChromaFx.Filters.Effects;
using ChromaFx.Colors;

namespace ChromaFx;

public partial class Image
{
    /// <summary>
    /// Implements the operator -.
    /// </summary>
    /// <param name="image1">The image1.</param>
    /// <param name="image2">The image2.</param>
    /// <returns>The result of the operator.</returns>
    public static Image operator -(Image image1, Image image2)
    {
        var tempArray = new Color[image1.Pixels.Length];
        Array.Copy(image1.Pixels, tempArray, tempArray.Length);
        var result = new Image(image1.Width, image1.Height, tempArray);
        return new Subtract(image2).Apply(result);
    }

    /// <summary>
    /// Implements the operator !.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <returns>The result of the operator.</returns>
    public static Image operator !(Image image)
    {
        var tempArray = new Color[image.Pixels.Length];
        Array.Copy(image.Pixels, tempArray, tempArray.Length);
        var result = new Image(image.Width, image.Height, tempArray);
        return new Invert().Apply(result);
    }

    /// <summary>
    /// Implements the operator %.
    /// </summary>
    /// <param name="image1">The image1.</param>
    /// <param name="image2">The image2.</param>
    /// <returns>The result of the operator.</returns>
    public static Image operator %(Image image1, Image image2)
    {
        var tempArray = new Color[image1.Pixels.Length];
        Array.Copy(image1.Pixels, tempArray, tempArray.Length);
        var result = new Image(image1.Width, image1.Height, tempArray);
        return new Modulo(image2).Apply(result);
    }

    /// <summary>
    /// Implements the operator &amp;.
    /// </summary>
    /// <param name="image1">The image1.</param>
    /// <param name="image2">The image2.</param>
    /// <returns>The result of the operator.</returns>
    public static Image operator &(Image image1, Image image2)
    {
        var tempArray = new Color[image1.Pixels.Length];
        Array.Copy(image1.Pixels, tempArray, tempArray.Length);
        var result = new Image(image1.Width, image1.Height, tempArray);
        return new And(image2).Apply(result);
    }

    /// <summary>
    /// Implements the operator *.
    /// </summary>
    /// <param name="image1">The image1.</param>
    /// <param name="image2">The image2.</param>
    /// <returns>The result of the operator.</returns>
    public static Image operator *(Image image1, Image image2)
    {
        var tempArray = new Color[image1.Pixels.Length];
        Array.Copy(image1.Pixels, tempArray, tempArray.Length);
        var result = new Image(image1.Width, image1.Height, tempArray);
        return new Multiplication(image2).Apply(result);
    }

    /// <summary>
    /// Implements the operator /.
    /// </summary>
    /// <param name="image1">The image1.</param>
    /// <param name="image2">The image2.</param>
    /// <returns>The result of the operator.</returns>
    public static Image operator /(Image image1, Image image2)
    {
        var tempArray = new Color[image1.Pixels.Length];
        Array.Copy(image1.Pixels, tempArray, tempArray.Length);
        var result = new Image(image1.Width, image1.Height, tempArray);
        return new Division(image2).Apply(result);
    }

    /// <summary>
    /// Implements the operator ^.
    /// </summary>
    /// <param name="image1">The image1.</param>
    /// <param name="image2">The image2.</param>
    /// <returns>The result of the operator.</returns>
    public static Image operator ^(Image image1, Image image2)
    {
        var tempArray = new Color[image1.Pixels.Length];
        Array.Copy(image1.Pixels, tempArray, tempArray.Length);
        var result = new Image(image1.Width, image1.Height, tempArray);
        return new XOr(image2).Apply(result);
    }

    /// <summary>
    /// Implements the operator |.
    /// </summary>
    /// <param name="image1">The image1.</param>
    /// <param name="image2">The image2.</param>
    /// <returns>The result of the operator.</returns>
    public static Image operator |(Image image1, Image image2)
    {
        var tempArray = new Color[image1.Pixels.Length];
        Array.Copy(image1.Pixels, tempArray, tempArray.Length);
        var result = new Image(image1.Width, image1.Height, tempArray);
        return new Or(image2).Apply(result);
    }

    /// <summary>
    /// Implements the operator +.
    /// </summary>
    /// <param name="image1">The image1.</param>
    /// <param name="image2">The image2.</param>
    /// <returns>The result of the operator.</returns>
    public static Image operator +(Image image1, Image image2)
    {
        var tempArray = new Color[image1.Pixels.Length];
        Array.Copy(image1.Pixels, tempArray, tempArray.Length);
        var result = new Image(image1.Width, image1.Height, tempArray);
        return new Add(image2).Apply(result);
    }

    /// <summary>
    /// Implements the operator &lt;&lt;.
    /// </summary>
    /// <param name="image1">The image1.</param>
    /// <param name="value">The value (should be between 0 and 255).</param>
    /// <returns>The result of the operator.</returns>
    public static Image operator <<(Image image1, int value)
    {
        value = Math.Abs(value);
        var tempArray = new Color[image1.Pixels.Length];
        Array.Copy(image1.Pixels, tempArray, tempArray.Length);
        var result = new Image(image1.Width, image1.Height, tempArray);
        return new Brightness(value / 255f).Apply(result);
    }

    /// <summary>
    /// Implements the operator &gt;&gt;.
    /// </summary>
    /// <param name="image1">The image1.</param>
    /// <param name="value">The value.</param>
    /// <returns>The result of the operator.</returns>
    public static Image operator >>(Image image1, int value)
    {
        value = -Math.Abs(value);
        var tempArray = new Color[image1.Pixels.Length];
        Array.Copy(image1.Pixels, tempArray, tempArray.Length);
        var result = new Image(image1.Width, image1.Height, tempArray);
        return new Brightness(value / 255f).Apply(result);
    }
}
