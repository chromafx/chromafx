using BenchmarkDotNet.Attributes;
using ChromaFx.Core;
using ChromaFx.Core.Colors;
using ChromaFx.Processing.Filters.Binary;
using ChromaFx.Processing.Numerics;
using Microsoft.VSDiagnostics;

namespace ChromaFx.Benchmarks
{
    [CPUUsageDiagnoser]
    public class AdaptiveThresholdBenchmark
    {
        private const int Width = 512;
        private const int Height = 512;
        [Benchmark(Description = "ChromaFx AdaptiveThreshold Filter")]
        public void AdaptiveThresholdChromaFx()
        {
            var image = new Image(Width, Height, new byte[Width * Height * 4]);
            var filter = new AdaptiveThreshold(3, Color.Black, Color.White, 0.5f);
            filter.Apply(image, new Rectangle(0, 0, Width, Height));
        }
    }
}