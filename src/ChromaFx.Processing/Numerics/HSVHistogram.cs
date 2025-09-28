using ChromaFx.Core.Colors;
using ChromaFx.Processing.Numerics.Interfaces;
using ChromaFx.Core.Colors.ColorSpaces;
using ChromaFx.Core;

namespace ChromaFx.Processing.Numerics;

/// <summary>
/// Class used to create an RGB Histogram
/// </summary>
public class HsvHistogram : IHistogram
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HsvHistogram"/> class.
    /// </summary>
    /// <param name="image">The image to load.</param>
    public HsvHistogram(Image image = null)
    {
        V = new double[101];
        if (image != null)
            LoadImage(image);
    }

    /// <summary>
    /// Red values
    /// </summary>
    public double[] V { get; set; }

    private int _height;

    private int _width;

    /// <summary>
    /// Equalizes the histogram
    /// </summary>
    /// <returns>this</returns>
    public IHistogram Equalize()
    {
        double totalPixels = _width * _height;
        var vMax = int.MinValue;
        var vMin = int.MaxValue;
        for (var x = 0; x < 101; ++x)
        {
            if (V[x] > 0f)
            {
                if (vMax < x)
                    vMax = x;
                if (vMin > x)
                    vMin = x;
            }
        }
        var previousV = V[0];
        V[0] = V[0] / totalPixels;
        for (var x = 1; x < 101; ++x)
        {
            previousV += V[x];
            V[x] = (previousV - V[vMin]) / (totalPixels - V[vMin]);
        }
        _width = 256;
        _height = 1;
        return this;
    }

    /// <summary>
    /// Equalizes the color.
    /// </summary>
    /// <param name="color">The color.</param>
    /// <returns>The resulting color</returns>
    public Color EqualizeColor(Color color)
    {
        var tempHsv = (Hsv)color;
        return new Hsv(
            tempHsv.Hue,
            tempHsv.Saturation,
            V[(int)Math.Round(tempHsv.Value * 100, MidpointRounding.AwayFromZero)]
        );
    }

    /// <summary>
    /// Loads the specified colors.
    /// </summary>
    /// <param name="colors">The colors.</param>
    /// <returns>This</returns>
    public IHistogram Load(params Color[] colors)
    {
        _width = colors.Length;
        _height = 1;
        Array.Clear(V, 0, V.Length);
        foreach (var c in colors)
        {
            var tempHsv = (Hsv)c;
            ++V[(int)(tempHsv.Value * 100)];
        }
        return this;
    }

    /// <summary>
    /// Loads an image
    /// </summary>
    /// <param name="image">Image to load</param>
    /// <returns>this</returns>
    public IHistogram LoadImage(Image image)
    {
        _width = image.Width;
        _height = image.Height;
        Array.Clear(V, 0, V.Length);

        for (var x = 0; x < image.Width; ++x)
        {
            for (var y = 0; y < image.Height; ++y)
            {
                var pixel = image.Pixels[y * image.Width + x];
                var tempHsv = (Hsv)new Color(pixel.Red, pixel.Green, pixel.Blue);
                ++V[(int)(tempHsv.Value * 100)];
            }
        }

        return this;
    }

    /// <summary>
    /// Normalizes the histogram
    /// </summary>
    /// <returns>this</returns>
    public IHistogram Normalize()
    {
        double totalPixels = _width * _height;
        if (totalPixels <= 0)
            return this;
        for (var x = 0; x < 101; ++x)
        {
            V[x] /= totalPixels;
        }
        return this;
    }
}
