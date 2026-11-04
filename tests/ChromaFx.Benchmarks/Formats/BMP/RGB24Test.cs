using BenchmarkDotNet.Attributes;
using ChromaFx.IO.Formats.Bmp.Format;
using ChromaFx.IO.Formats.Bmp.Format.PixelFormats;

namespace ChromaFx.Benchmarks.Formats.BMP;

public class Rgb24Test
{
    [Params(100, 1000, 10000)]
    public int Count { get; set; }

    private Header _header = null!;
    private byte[] _bgr = null!;
    private byte[] _rgba = null!;
    private Palette _palette = null!;
    private Rgb24Bit _format = null!;

    [GlobalSetup]
    public void Setup()
    {
        _format = new Rgb24Bit();
        _header = new Header(Count, Count, 0, Count * Count * 3, 0, 0, 0, 0, Compression.Rgb);
        _bgr = new byte[Count * Count * 3];
        _rgba = new byte[Count * Count * 4];
        _palette = new Palette(0, []);
    }

    [Benchmark(Description = "BMP RGB24 Decode")]
    public byte[] Decode() => _format.Decode(_header, _bgr, _palette);

    [Benchmark(Description = "BMP RGB24 Encode")]
    public byte[] Encode() => _format.Encode(_header, _rgba, _palette);
}
