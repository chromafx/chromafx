using BenchmarkDotNet.Attributes;
using ChromaFx.Core;
using ChromaFx.Core.Colors;

namespace ChromaFx.Benchmarks;

public class ImageSimilarityBenchmark
{
    private const int Width = 256;
    private const int Height = 256;

    private Image _a = null!;
    private Image _b = null!;

    [GlobalSetup]
    public void Setup()
    {
        _a = Solid(Width, Height, new Color(40, 80, 120, 255));
        _b = Solid(Width, Height, new Color(42, 78, 118, 255));
    }

    [Benchmark]
    public double Mse() => ImageSimilarity.Mse(_a, _b);

    [Benchmark]
    public double Ssim() => ImageSimilarity.Ssim(_a, _b);

    [Benchmark]
    public double MeanCie76() => ColorDifference.MeanCie76(_a, _b);

    private static Image Solid(int w, int h, Color color)
    {
        var data = new Color[w * h];
        Array.Fill(data, color);
        return new Image(w, h, data);
    }
}
