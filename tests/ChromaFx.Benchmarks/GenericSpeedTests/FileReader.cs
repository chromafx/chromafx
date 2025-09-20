using BenchmarkDotNet.Attributes;

namespace ChromaFx.Benchmarks.GenericSpeedTests;

public class FileReader
{
    [Benchmark(Description = "File.Read")]
    public void FileRead()
    {
        using var testStream = File.Open("../../../../TestImage/BitmapFilter.bmp", FileMode.Open);
        var data = new byte[testStream.Length];
        testStream.Read(data, 0, (int)testStream.Length);
    }

    [Benchmark(Description = "File.Read in loop, 1024")]
    public void FileReadLoop1024()
    {
        var data = new byte[1024];
        using var testStream = File.Open("../../../../TestImage/BitmapFilter.bmp", FileMode.Open);
        while (testStream.Read(data, 0, 1024) == 1024) { }
    }

    [Benchmark(Description = "File.Read in loop, 2048")]
    public void FileReadLoop2048()
    {
        var data = new byte[2048];
        using var testStream = File.Open("../../../../TestImage/BitmapFilter.bmp", FileMode.Open);
        while (testStream.Read(data, 0, 2048) == 2048) { }
    }

    [Benchmark(Baseline = true, Description = "File.Read in loop, 4096")]
    public void FileReadLoop4096()
    {
        var data = new byte[4096];
        using var testStream = File.Open("../../../../TestImage/BitmapFilter.bmp", FileMode.Open);
        while (testStream.Read(data, 0, 4096) == 4096) { }
    }

    [Benchmark(Description = "File.ReadAllBytes")]
    public void ReadAllBytes()
    {
#pragma warning disable IDE0059 // Unnecessary assignment of a value
        var data = File.ReadAllBytes("../../../../TestImage/BitmapFilter.bmp");
#pragma warning restore IDE0059 // Unnecessary assignment of a value
    }
}