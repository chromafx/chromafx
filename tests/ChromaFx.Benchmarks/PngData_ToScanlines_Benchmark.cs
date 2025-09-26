using System;
using BenchmarkDotNet.Attributes;
using ChromaFx.Formats.Png.Format;
using ChromaFx.Colors;
using Microsoft.VSDiagnostics;

namespace ChromaFx.Benchmarks
{
    [CPUUsageDiagnoser]
    public class PngData_ToScanlines_Benchmark
    {
        private Image _image;
        [GlobalSetup]
        public void Setup()
        {
            // Create a 512x512 image with random colors
            var rand = new Random(42);
            var pixels = new Color[512 * 512];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = new Color((byte)rand.Next(256), (byte)rand.Next(256), (byte)rand.Next(256), 255);
            }

            _image = new Image(512, 512, pixels);
        }

        [Benchmark(Description = "ChromaFx PNG ToScanlines (compression)")]
        public void EncodeToScanlines()
        {
            var _ = Data.ToScanlines(_image);
        }
    }
}