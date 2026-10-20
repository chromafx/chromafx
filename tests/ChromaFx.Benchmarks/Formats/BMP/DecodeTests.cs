using BenchmarkDotNet.Attributes;

namespace ChromaFx.Benchmarks.Formats.BMP;

public class DecodeTests
{
    private static string BmpPath =>
        Path.Combine(AppContext.BaseDirectory, "TestImage", "BitmapFilter.bmp");

    [Benchmark(Baseline = true, Description = "FileStream reading")]
    public void FileStreamReading()
    {
        using var testStream = File.Open(BmpPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        new IO.Formats.Bmp.BmpFormat().Decode(testStream);
    }

    [Benchmark(Description = "MemoryStream reading")]
    public void MemoryStreamReading()
    {
        using var testStream = File.Open(BmpPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var data = new byte[testStream.Length];
        testStream.ReadExactly(data);
        using var memStream = new MemoryStream(data);
        new IO.Formats.Bmp.BmpFormat().Decode(memStream);
    }
}
