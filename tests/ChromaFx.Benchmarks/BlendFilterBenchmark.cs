using BenchmarkDotNet.Attributes;
using ChromaFx.Core;
using ChromaFx.Processing.Filters.Overlays;
using ChromaFx.Processing.Numerics;

namespace ChromaFx.Benchmarks;

public class BlendFilterBenchmark
{
    private const int Width = 512;
    private const int Height = 512;

    private Image _image = null!;
    private Blend _blend = null!;
    private Rectangle _region;

    [GlobalSetup]
    public void Setup()
    {
        _image = new Image(Width, Height);
        var overlay = new Image(Width, Height);
        _blend = new Blend(overlay, 0.5f);
        _region = new Rectangle(0, 0, Width, Height);
    }

    [Benchmark(Description = "ChromaFx Blend Filter")]
    public Image BlendChromaFx() => _blend.Apply(_image, _region);
}
