using System.Drawing;
using System.Runtime.Versioning;
using BenchmarkDotNet.Attributes;
using ChromaFx.Core;
using ChromaFx.Processing.Numerics;

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
        var testImage = new Core.Image(Width, Height, new byte[Width * Height * 4]);
        var cropFilter = new Processing.Filters.Resampling.Crop();
        cropFilter.Apply(testImage, new Processing.Numerics.Rectangle(0, 0, Count, Count));
    }

    [Benchmark(Description = "ChromaFx Test Crop")]
    public void CropChromaFxTest()
    {
        var testImage = new Core.Image(Width, Height, new byte[Width * Height * 4]);
        var testCropFilter = new Processing.Filters.Resampling.Crop();
        testCropFilter.Apply(testImage, new Processing.Numerics.Rectangle(0, 0, Count, Count));
    }

    [SupportedOSPlatform("windows")]
    [Benchmark(Baseline = true, Description = "System.Drawing Crop")]
    public void CropSystemDrawing()
    {
        using Bitmap source = new(Width, Height);
        using var destination = source.Clone(new System.Drawing.Rectangle(0, 0, Count, Count), source.PixelFormat);
    }
}
