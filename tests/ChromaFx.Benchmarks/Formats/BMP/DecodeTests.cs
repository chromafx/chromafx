using BenchmarkDotNet.Attributes;

namespace ChromaFx.Benchmarks.Formats.BMP;

public class DecodeTests
{
    [Benchmark(Baseline = true, Description = "FileStream reading")]
    public void FileStreamReading()
    {
        using var testStream = File.Open("../../../../TestImage/BitmapFilter.bmp", FileMode.Open);
        new IO.Formats.Bmp.BmpFormat().Decode(testStream);
    }

    [Benchmark(Description = "MemoryStream reading")]
    public void MemoryStreamReading()
    {
        using var testStream = File.Open("../../../../TestImage/BitmapFilter.bmp", FileMode.Open);
        var data = new byte[testStream.Length];
        testStream.Read(data, 0, (int)testStream.Length);
        using var memStream = new MemoryStream(data);
        new IO.Formats.Bmp.BmpFormat().Decode(memStream);
    }
}