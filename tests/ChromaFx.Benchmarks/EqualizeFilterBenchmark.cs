using BenchmarkDotNet.Attributes;
using ChromaFx.Core;
using ChromaFx.Processing.Filters.Normalization;
using ChromaFx.Processing.Numerics;

namespace ChromaFx.Benchmarks;

public class EqualizeFilterBenchmark
{
    private const int Width = 512;
    private const int Height = 512;

    private Image _image = null!;
    private Equalize _equalize = null!;
    private Rectangle _region;

    [GlobalSetup]
    public void Setup()
    {
        _image = new Image(Width, Height);
        _equalize = new Equalize();
        _region = new Rectangle(0, 0, Width, Height);
    }

    [Benchmark(Description = "ChromaFx Equalize Filter")]
    public Image EqualizeChromaFx() => _equalize.Apply(_image, _region);
}
