using BenchmarkDotNet.Attributes;
using ChromaFx.Core;
using ChromaFx.Processing.Filters.Smoothing;
using ChromaFx.Processing.Numerics;

namespace ChromaFx.Benchmarks;

public class MedianFilterBenchmark
{
    private const int Width = 512;
    private const int Height = 512;

    private Image _image = null!;
    private Median _median = null!;
    private Rectangle _region;

    [GlobalSetup]
    public void Setup()
    {
        _image = new Image(Width, Height);
        _median = new Median(3);
        _region = new Rectangle(0, 0, Width, Height);
    }

    [Benchmark(Description = "ChromaFx Median Filter")]
    public Image MedianChromaFx() => _median.Apply(_image, _region);
}
