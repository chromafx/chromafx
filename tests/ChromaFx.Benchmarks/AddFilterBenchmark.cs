using BenchmarkDotNet.Attributes;
using ChromaFx.Core;
using ChromaFx.Processing.Filters.Arithmetic;
using ChromaFx.Processing.Numerics;
using Microsoft.VSDiagnostics;

namespace ChromaFx.Benchmarks
{
    [CPUUsageDiagnoser]
    public class AddFilterBenchmark
    {
        private const int Width = 512;
        private const int Height = 512;
        [Benchmark(Description = "ChromaFx Add Filter")]
        public void AddChromaFx()
        {
            var image = new Image(Width, Height);
            var secondImage = new Image(Width, Height);
            var addFilter = new Add(secondImage);
            addFilter.Apply(image, new Rectangle(0, 0, Width, Height));
        }
    }
}