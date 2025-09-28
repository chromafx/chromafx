using ChromaFx.Core;
using ChromaFx.Core.Colors;
using ChromaFx.Processing.Filters.Arithmetic;
using ChromaFx.Processing.Filters.ColorMatrix;
using ChromaFx.Processing.Filters.Effects;

namespace ChromaFx.Processing;

public static class ImageOperatorExtensions
{
    public static Image Add(this Image image1, Image image2)
    {
        var tempArray = new Color[image1.Pixels.Length];
        Array.Copy(image1.Pixels, tempArray, tempArray.Length);
        var result = new Image(image1.Width, image1.Height, tempArray);
        return new Add(image2).Apply(result);
    }

    public static Image Subtract(this Image image1, Image image2)
    {
        var tempArray = new Color[image1.Pixels.Length];
        Array.Copy(image1.Pixels, tempArray, tempArray.Length);
        var result = new Image(image1.Width, image1.Height, tempArray);
        return new Subtract(image2).Apply(result);
    }

    public static Image Modulo(this Image image1, Image image2)
    {
        var tempArray = new Color[image1.Pixels.Length];
        Array.Copy(image1.Pixels, tempArray, tempArray.Length);
        var result = new Image(image1.Width, image1.Height, tempArray);
        return new Modulo(image2).Apply(result);
    }

    public static Image And(this Image image1, Image image2)
    {
        var tempArray = new Color[image1.Pixels.Length];
        Array.Copy(image1.Pixels, tempArray, tempArray.Length);
        var result = new Image(image1.Width, image1.Height, tempArray);
        return new And(image2).Apply(result);
    }

    public static Image Multiply(this Image image1, Image image2)
    {
        var tempArray = new Color[image1.Pixels.Length];
        Array.Copy(image1.Pixels, tempArray, tempArray.Length);
        var result = new Image(image1.Width, image1.Height, tempArray);
        return new Multiplication(image2).Apply(result);
    }

    public static Image Divide(this Image image1, Image image2)
    {
        var tempArray = new Color[image1.Pixels.Length];
        Array.Copy(image1.Pixels, tempArray, tempArray.Length);
        var result = new Image(image1.Width, image1.Height, tempArray);
        return new Division(image2).Apply(result);
    }

    public static Image XOr(this Image image1, Image image2)
    {
        var tempArray = new Color[image1.Pixels.Length];
        Array.Copy(image1.Pixels, tempArray, tempArray.Length);
        var result = new Image(image1.Width, image1.Height, tempArray);
        return new XOr(image2).Apply(result);
    }

    public static Image Or(this Image image1, Image image2)
    {
        var tempArray = new Color[image1.Pixels.Length];
        Array.Copy(image1.Pixels, tempArray, tempArray.Length);
        var result = new Image(image1.Width, image1.Height, tempArray);
        return new Or(image2).Apply(result);
    }

    public static Image Invert(this Image image)
    {
        var tempArray = new Color[image.Pixels.Length];
        Array.Copy(image.Pixels, tempArray, tempArray.Length);
        var result = new Image(image.Width, image.Height, tempArray);
        return new Invert().Apply(result);
    }

    public static Image BrightnessUp(this Image image, int value)
    {
        value = Math.Abs(value);
        var tempArray = new Color[image.Pixels.Length];
        Array.Copy(image.Pixels, tempArray, tempArray.Length);
        var result = new Image(image.Width, image.Height, tempArray);
        return new Brightness(value / 255f).Apply(result);
    }

    public static Image BrightnessDown(this Image image, int value)
    {
        value = -Math.Abs(value);
        var tempArray = new Color[image.Pixels.Length];
        Array.Copy(image.Pixels, tempArray, tempArray.Length);
        var result = new Image(image.Width, image.Height, tempArray);
        return new Brightness(value / 255f).Apply(result);
    }
}
