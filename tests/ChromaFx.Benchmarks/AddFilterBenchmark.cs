using BenchmarkDotNet.Attributes;
using ChromaFx.Core;
using ChromaFx.Processing.Filters.Arithmetic;
using ChromaFx.Processing.Numerics;

namespace ChromaFx.Benchmarks;

public class AddFilterBenchmark
{
    private const int Width = 512;
    private const int Height = 512;

    private Image _image = null!;
    private Add _addFilter = null!;
    private Rectangle _region;

    [GlobalSetup]
    public void Setup()
    {
        _image = new Image(Width, Height);
        var secondImage = new Image(Width, Height);
        _addFilter = new Add(secondImage);
        _region = new Rectangle(0, 0, Width, Height);
    }

    [Benchmark(Description = "ChromaFx Add Filter")]
    public Image AddChromaFx() => _addFilter.Apply(_image, _region);
}
