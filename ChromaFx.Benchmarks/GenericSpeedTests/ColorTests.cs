using BenchmarkDotNet.Attributes;

namespace ChromaFx.Benchmarks.GenericSpeedTests;

public class ColorTests
{
    [Benchmark(Description = "New Color struct")]
    public void NewColorStruct()
    {
        var testArray = new TestClasses.ColorStruct[10000];
        for (var x = 0; x < testArray.Length; ++x)
        {
            testArray[x] *= 3;
        }
    }

    [Benchmark(Baseline = true, Description = "Old Color struct")]
    public void OldColorStruct()
    {
        var testArray = new Colors.Color[10000];
        for (var x = 0; x < testArray.Length; ++x)
        {
            testArray[x] *= 3;
        }
    }
}
