using BenchmarkDotNet.Attributes;
using ChromaFx.Core;

namespace ChromaFx.Benchmarks;

public class GifDecodeBenchmark
{
    private byte[] _gifBytes = null!;

    [GlobalSetup]
    public void Setup()
    {
        _gifBytes = File.ReadAllBytes(
            Path.Combine(AppContext.BaseDirectory, "TestImage", "rings.gif")
        );
    }

    [Benchmark(Description = "ChromaFx GIF Decode")]
    public Animation DecodeGif() => Animation.Load(_gifBytes);
}
