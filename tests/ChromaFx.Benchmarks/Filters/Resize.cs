using BenchmarkDotNet.Attributes;
using ChromaFx.Processing.Filters.Resampling.Enums;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.Versioning;

namespace ChromaFx.Benchmarks.Filters;

public class Resize
{
    [Benchmark(Description = "ChromaFx Resize")]
    public void ResizeChromaFx()
    {
        var testImage = new Core.Image(2000, 2000);
        var filter = new Processing.Filters.Resampling.Resize(400, 400, ResamplingFiltersAvailable.NearestNeighbor);
        filter.Apply(testImage);
    }

    [SupportedOSPlatform("windows")]
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