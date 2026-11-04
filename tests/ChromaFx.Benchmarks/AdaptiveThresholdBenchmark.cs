using BenchmarkDotNet.Attributes;
using ChromaFx.Core;
using ChromaFx.Core.Colors;
using ChromaFx.Processing.Filters.Binary;
using ChromaFx.Processing.Numerics;

namespace ChromaFx.Benchmarks;

public class AdaptiveThresholdBenchmark
{
    private const int Width = 512;
    private const int Height = 512;

    private Image _image = null!;
    private AdaptiveThreshold _filter = null!;
    private Rectangle _region;

    [GlobalSetup]
    public void Setup()
    {
        _image = new Image(Width, Height);
        _filter = new AdaptiveThreshold(3, Color.Black, Color.White, 0.5f);
        _region = new Rectangle(0, 0, Width, Height);
    }

    [Benchmark(Description = "ChromaFx AdaptiveThreshold Filter")]
    public Image AdaptiveThresholdChromaFx() => _filter.Apply(_image, _region);
}
