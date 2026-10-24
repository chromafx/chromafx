using BenchmarkDotNet.Attributes;
using ChromaFx.Core;
using ChromaFx.IO;
using ChromaFx.IO.Encoders;

namespace ChromaFx.Benchmarks;

public class JpegFileEncodingDecodingBenchmarks
{
    private Image _image = null!;
    private byte[] _encoded = null!;

    [GlobalSetup]
    public void Setup()
    {
        _image = new Image(512, 512);
        using var stream = new MemoryStream();
        _image.Save(stream, new JpegEncoder { Quality = 90 });
        _encoded = stream.ToArray();
    }

    [Benchmark(Description = "ChromaFx JPEG Encode")]
    public void EncodeChromaFx()
    {
        using var stream = new MemoryStream();
        _image.Save(stream, new JpegEncoder { Quality = 90 });
    }

    [Benchmark(Description = "ChromaFx JPEG Decode")]
    public void DecodeChromaFx()
    {
        using var stream = new MemoryStream(_encoded);
        _ = Image.Load(stream);
    }
}
