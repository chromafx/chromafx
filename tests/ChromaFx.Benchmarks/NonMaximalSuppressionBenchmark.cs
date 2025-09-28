using BenchmarkDotNet.Attributes;
using ChromaFx.Core;
using ChromaFx.Core.Colors;
using ChromaFx.Processing.Filters.Binary;
using ChromaFx.Processing.Numerics;
using Microsoft.VSDiagnostics;

namespace ChromaFx.Benchmarks
{
    [CPUUsageDiagnoser]
    public class NonMaximalSuppressionBenchmark
    {
        private const int Width = 512;
        private const int Height = 512;
        private Image image;
        private NonMaximalSuppression filter;
        private Rectangle rect;

        [GlobalSetup]
        public void Setup()
        {
            image = new Image(Width, Height, new Color[Width * Height]);
            filter = new NonMaximalSuppression(Color.Black, Color.White, 0.5f, 0.3f);
            rect = new Rectangle(0, 0, Width, Height);
        }

        [Benchmark(Description = "ChromaFx NonMaximalSuppression Filter")]
        public void NonMaximalSuppressionChromaFx()
        {
            filter.Apply(image, rect);
        }
    }
}
