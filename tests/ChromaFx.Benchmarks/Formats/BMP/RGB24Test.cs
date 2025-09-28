using BenchmarkDotNet.Attributes;
using ChromaFx.IO.Formats.Bmp.Format;
using ChromaFx.IO.Formats.Bmp.Format.PixelFormats;

namespace ChromaFx.Benchmarks.Formats.BMP;

public class Rgb24Test
{
    [Params(100, 1000, 10000)]
    public int Count { get; set; }

    [Benchmark(Baseline = true, Description = "Without pointers")]
    public void Current()
    {
        new Rgb24Bit().Decode(new Header(Count, Count, 0, Count * Count * 3, 0, 0, 0, 0, Compression.Rgb), new byte[Count * Count * 3], new Palette(0, []));
    }
}