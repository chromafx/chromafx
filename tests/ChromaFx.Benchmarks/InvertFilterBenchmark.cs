using BenchmarkDotNet.Attributes;
using ChromaFx.Core;
using ChromaFx.Processing.Filters.Effects;
using ChromaFx.Processing.Numerics;

namespace ChromaFx.Benchmarks;

public class InvertFilterBenchmark
{
    private const int Width = 512;
    private const int Height = 512;

    private Image _image = null!;
    private Invert _invert = null!;
    private Rectangle _region;

    [GlobalSetup]
    public void Setup()
    {
        _image = new Image(Width, Height);
        _invert = new Invert();
        _region = new Rectangle(0, 0, Width, Height);
    }

    [Benchmark(Description = "ChromaFx Invert Filter")]
    public Image InvertChromaFx() => _invert.Apply(_image, _region);
}
