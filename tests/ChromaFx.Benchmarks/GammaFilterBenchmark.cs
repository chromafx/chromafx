using BenchmarkDotNet.Attributes;
using ChromaFx.Core;
using ChromaFx.Processing.Filters.Normalization;
using ChromaFx.Processing.Numerics;

namespace ChromaFx.Benchmarks;

public class GammaFilterBenchmark
{
    private const int Width = 512;
    private const int Height = 512;

    private Image _image = null!;
    private Gamma _gamma = null!;
    private Rectangle _region;

    [GlobalSetup]
    public void Setup()
    {
        _image = new Image(Width, Height);
        _gamma = new Gamma(2.2f);
        _region = new Rectangle(0, 0, Width, Height);
    }

    [Benchmark(Description = "ChromaFx Gamma Filter")]
    public Image GammaChromaFx() => _gamma.Apply(_image, _region);
}
