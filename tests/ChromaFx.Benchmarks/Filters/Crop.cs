using System.Drawing;
using System.Runtime.Versioning;
using BenchmarkDotNet.Attributes;

namespace ChromaFx.Benchmarks.Filters;

public class Crop
{
    [Params(10, 100, 1000, 5000)]
    public int Count { get; set; }

    private static int Height => 8000;
    private static int Width => 8000;

    [Benchmark(Description = "ChromaFx Crop")]
    public void CropChromaFx()
    {
        var testImage = new Image(Width, Height, new byte[Width * Height * 4]);
        var cropFilter = new ChromaFx.Filters.Resampling.Crop();
        cropFilter.Apply(testImage, new Numerics.Rectangle(0, 0, Count, Count));
    }

    [Benchmark(Description = "ChromaFx Test Crop")]
    public void CropChromaFxTest()
    {
        var testImage = new Image(Width, Height, new byte[Width * Height * 4]);
        var testCropFilter = new ChromaFx.Filters.Resampling.Crop();
        testCropFilter.Apply(testImage, new Numerics.Rectangle(0, 0, Count, Count));
    }

    [SupportedOSPlatform("windows")]
    [Benchmark(Baseline = true, Description = "System.Drawing Crop")]
    public void CropSystemDrawing()
    {
        using Bitmap source = new(Width, Height);
        using var destination = source.Clone(new Rectangle(0, 0, Count, Count), source.PixelFormat);
    }
}
