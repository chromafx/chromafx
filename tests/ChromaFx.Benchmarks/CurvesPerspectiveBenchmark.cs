using BenchmarkDotNet.Attributes;
using ChromaFx.Core;
using ChromaFx.Core.Colors;
using ChromaFx.Processing.Filters.Normalization;
using ChromaFx.Processing.Filters.Resampling;
using ChromaFx.Processing.Numerics;

namespace ChromaFx.Benchmarks;

public class CurvesPerspectiveBenchmark
{
    private const int Width = 512;
    private const int Height = 512;

    private Image _image = null!;
    private Curves _curves = null!;
    private Perspective _perspective = null!;
    private Rectangle _region;

    [GlobalSetup]
    public void Setup()
    {
        _image = new Image(Width, Height);
        for (var y = 0; y < Height; y++)
        {
            var row = _image.GetRowSpan(y);
            for (var x = 0; x < Width; x++)
                row[x] = new Color((byte)(x & 255), (byte)(y & 255), (byte)((x + y) & 255), 255);
        }

        _curves = new Curves(0.05f, 0.5f, 0.95f);
        _perspective = new Perspective(0.15f, 0.1f);
        _region = new Rectangle(0, 0, Width, Height);
    }

    [Benchmark]
    public Image Curves() => _curves.Apply(new Image(_image), _region);

    [Benchmark]
    public Image Perspective() => _perspective.Apply(new Image(_image), _region);
}
