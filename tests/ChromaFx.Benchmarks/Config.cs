using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;

namespace ChromaFx.Benchmarks;

/// <summary>
/// Cross-platform benchmark config (memory diagnoser; no Windows-only ETW).
/// </summary>
public class Config : ManualConfig
{
    public Config()
    {
        Add(DefaultConfig.Instance);
        AddDiagnoser(MemoryDiagnoser.Default);
    }
}
