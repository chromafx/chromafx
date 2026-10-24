using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.Versioning;
using BenchmarkDotNet.Attributes;
using ChromaFx.Processing.Filters.Resampling.Enums;

namespace ChromaFx.Benchmarks.Filters;

public class Resize
{
    private Core.Image _source = null!;
    private Processing.Filters.Resampling.Resize _filter = null!;

    [GlobalSetup]
    public void Setup()
    {
        _source = new Core.Image(2000, 2000);
        _filter = new Processing.Filters.Resampling.Resize(
            400,
            400,
            ResamplingFiltersAvailable.NearestNeighbor
        );
    }

    [Benchmark(Description = "ChromaFx Resize")]
    public void ResizeChromaFx()
    {
        // Clone so each iteration starts from the full-size source without counting alloc in setup.
        var testImage = _source.Clone();
        _filter.Apply(testImage);
    }

    [SupportedOSPlatform("windows")]
    [OperatingSystemsFilter(true, OS.Windows)]
    [Benchmark(Baseline = true, Description = "System.Drawing Resize")]
    public void ResizeSystemDrawing()
    {
        using Bitmap source = new(2000, 2000);
        using Bitmap destination = new(400, 400);
        using var graphics = Graphics.FromImage(destination);
        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
        graphics.CompositingQuality = CompositingQuality.HighQuality;
        graphics.DrawImage(source, 0, 0, 400, 400);
    }
}
