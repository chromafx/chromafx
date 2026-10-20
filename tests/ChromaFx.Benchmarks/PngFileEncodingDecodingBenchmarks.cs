using BenchmarkDotNet.Attributes;
using ChromaFx.Core;

namespace ChromaFx.Benchmarks;

public class PngFileEncodingDecodingBenchmarks
{
    private byte[] _pngData = [];
    private Image _image = null!;

    [GlobalSetup]
    public void Setup()
    {
        _pngData = File.ReadAllBytes(
            Path.Combine(AppContext.BaseDirectory, "TestImage", "splash.png")
        );
        var file = new IO.Formats.Png.Format.File();
        using var decodeStream = new MemoryStream(_pngData);
        file.Decode(decodeStream);
        _image = file.GetImage();
    }

    [Benchmark(Description = "ChromaFx PNG Decode")]
    public Image DecodeChromaFx()
    {
        using var stream = new MemoryStream(_pngData);
        var file = new IO.Formats.Png.Format.File();
        file.Decode(stream);
        return file.GetImage();
    }

    [Benchmark(Description = "ChromaFx PNG Encode")]
    public void EncodeChromaFx()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        var file = new IO.Formats.Png.Format.File();
        file.Write(writer, _image);
    }
}
