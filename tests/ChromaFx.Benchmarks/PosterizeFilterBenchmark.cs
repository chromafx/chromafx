using BenchmarkDotNet.Attributes;
using ChromaFx.Core;
using ChromaFx.Processing.Filters.Effects;
using ChromaFx.Processing.Numerics;

namespace ChromaFx.Benchmarks;

public class PosterizeFilterBenchmark
{
    private const int Width = 512;
    private const int Height = 512;

    private Image _image = null!;
    private Posterize _posterize = null!;
    private Rectangle _region;

    [GlobalSetup]
    public void Setup()
    {
        _image = new Image(Width, Height);
        _posterize = new Posterize(4);
        _region = new Rectangle(0, 0, Width, Height);
    }

    [Benchmark(Description = "ChromaFx Posterize Filter")]
    public Image PosterizeChromaFx() => _posterize.Apply(_image, _region);
}
