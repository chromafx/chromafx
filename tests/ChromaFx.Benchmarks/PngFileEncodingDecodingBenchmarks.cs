using BenchmarkDotNet.Attributes;
using Microsoft.VSDiagnostics;

namespace ChromaFx.Benchmarks;

[CPUUsageDiagnoser]
public class PngFileEncodingDecodingBenchmarks
{
    private byte[] _pngData;
    private Image _image;

    [GlobalSetup]
    public void Setup()
    {
        // Prepare a small PNG file in memory for decoding
        _pngData = File.ReadAllBytes("TestImage/splash.png");
        var file = new ChromaFx.Formats.Png.Format.File();
        using var decodeStream = new MemoryStream(_pngData);
        file.Decode(decodeStream);
        _image = file.GetImage();
    }

    [Benchmark(Description = "ChromaFx PNG Decode")]
    public void DecodeChromaFx()
    {
        var stream = new MemoryStream(_pngData);
        var file = new ChromaFx.Formats.Png.Format.File();
        file.Decode(stream);
    }

    [Benchmark(Description = "ChromaFx PNG Encode")]
    public void EncodeChromaFx()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        var file = new ChromaFx.Formats.Png.Format.File();
        file.Write(writer, _image);
    }
}
