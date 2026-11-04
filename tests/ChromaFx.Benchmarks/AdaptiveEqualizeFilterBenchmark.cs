using BenchmarkDotNet.Attributes;
using ChromaFx.Core;
using ChromaFx.Processing.Filters.Normalization;
using ChromaFx.Processing.Numerics;

namespace ChromaFx.Benchmarks;

public class AdaptiveEqualizeFilterBenchmark
{
    private const int Width = 512;
    private const int Height = 512;

    private Image _image = null!;
    private AdaptiveEqualize _filter = null!;
    private Rectangle _region;

    [GlobalSetup]
    public void Setup()
    {
        _image = new Image(Width, Height);
        _filter = new AdaptiveEqualize(3);
        _region = new Rectangle(0, 0, Width, Height);
    }

    [Benchmark(Description = "ChromaFx AdaptiveEqualize Filter")]
    public Image AdaptiveEqualizeChromaFx() => _filter.Apply(_image, _region);
}
