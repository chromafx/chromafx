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

    private Core.Image _image = null!;
    private Processing.Filters.Resampling.Crop _cropFilter = null!;

    [GlobalSetup]
    public void Setup()
    {
        _cropFilter = new Processing.Filters.Resampling.Crop();
    }

    [IterationSetup]
    public void CreateImage()
    {
        _image = new Core.Image(Width, Height);
    }

    [Benchmark(Description = "ChromaFx Crop")]
    public void CropChromaFx()
    {
        _cropFilter.Apply(_image, new Processing.Numerics.Rectangle(0, 0, Count, Count));
    }

    [SupportedOSPlatform("windows")]
    [OperatingSystemsFilter(true, OS.Windows)]
    [Benchmark(Baseline = true, Description = "System.Drawing Crop")]
    public void CropSystemDrawing()
    {
        using Bitmap source = new(Width, Height);
        using var destination = source.Clone(
            new System.Drawing.Rectangle(0, 0, Count, Count),
            source.PixelFormat
        );
    }
}
