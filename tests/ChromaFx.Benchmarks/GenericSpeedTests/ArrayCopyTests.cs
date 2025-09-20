using BenchmarkDotNet.Attributes;

namespace ChromaFx.Benchmarks.GenericSpeedTests;

public class ArrayCopyTests
{
    [Params(100, 1000, 10000)]
    public int Count { get; set; }

    private byte[] _source, _destination;

    [Benchmark(Baseline = true, Description = "Copy using Array.Copy()")]
    public void CopyArray()
    {
        Array.Copy(_source, _destination, Count);
    }

    [IterationSetup]
    public void SetUp()
    {
        _source = new byte[Count];
        _destination = new byte[Count];
    }
}