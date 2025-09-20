using ChromaFx.Numerics.Interfaces;
using ChromaFx.Colors;

namespace ChromaFx.Numerics;

/// <summary>
/// Class used to create an RGB Histogram
/// </summary>
public class RgbHistogram : IHistogram
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RgbHistogram"/> class.
    /// </summary>
    /// <param name="image">The image to load.</param>
    public RgbHistogram(Image image = null)
    {
        R = new float[256];
        G = new float[256];
        B = new float[256];
        if (image != null)
            LoadImage(image);
    }

    /// <summary>
    /// Blue values
    /// </summary>
    public float[] B { get; set; }

    /// <summary>
    /// Green values
    /// </summary>
    public float[] G { get; set; }

    /// <summary>
    /// Red values
    /// </summary>
    public float[] R { get; set; }

    private int _height;

    private int _width;

    /// <summary>
    /// Equalizes the histogram
    /// </summary>
    /// <returns>this</returns>
    public IHistogram Equalize()
    {
        float totalPixels = _width * _height;
        var rMax = int.MinValue;
        var rMin = int.MaxValue;
        var gMax = int.MinValue;
        var gMin = int.MaxValue;
        var bMax = int.MinValue;
        var bMin = int.MaxValue;
        for (var x = 0; x < 256; ++x)
        {
            if (R[x] > 0f)
            {
                if (rMax < x)
                    rMax = x;
                if (rMin > x)
                    rMin = x;
            }
            if (G[x] > 0f)
            {
                if (gMax < x)
                    gMax = x;
                if (gMin > x)
                    gMin = x;
            }
            if (B[x] > 0f)
            {
                if (bMax < x)
                    bMax = x;
                if (bMin > x)
                    bMin = x;
            }
        }

        var previousR = R[0];
        R[0] = R[0] * 256 / totalPixels;
        var previousG = G[0];
        G[0] = G[0] * 256 / totalPixels;
        var previousB = B[0];
        B[0] = B[0] * 256 / totalPixels;
        for (var x = 1; x < 256; ++x)
        {
            previousR += R[x];
            previousG += G[x];
            previousB += B[x];
            R[x] = (previousR - R[rMin]) / (totalPixels - R[rMin]) * 255;
            G[x] = (previousG - G[gMin]) / (totalPixels - G[gMin]) * 255;
            B[x] = (previousB - B[bMin]) / (totalPixels - B[bMin]) * 255;
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
        return new Color((byte)R[color.Red], (byte)G[color.Green], (byte)B[color.Blue]);
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
        Array.Clear(R, 0, R.Length);
        Array.Clear(G, 0, G.Length);
        Array.Clear(B, 0, B.Length);
        for (var x = 0; x < colors.Length; ++x)
        {
            ++R[colors[x].Red];
            ++G[colors[x].Green];
            ++B[colors[x].Blue];
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
        Array.Clear(R, 0, R.Length);
        Array.Clear(G, 0, G.Length);
        Array.Clear(B, 0, B.Length);

        for (var x = 0; x < image.Width; ++x)
        {
            for (var y = 0; y < image.Height; ++y)
            {
                var pixel = image.Pixels[y * image.Width + x];
                ++R[pixel.Red];
                ++G[pixel.Green];
                ++B[pixel.Blue];
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
        float totalPixels = _width * _height;
        if (totalPixels <= 0)
            return this;
        for (var x = 0; x < 256; ++x)
        {
            R[x] /= totalPixels;
            G[x] /= totalPixels;
            B[x] /= totalPixels;
        }
        return this;
    }
}
