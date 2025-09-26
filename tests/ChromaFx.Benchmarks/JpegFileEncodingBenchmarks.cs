using BenchmarkDotNet.Attributes;
using Microsoft.VSDiagnostics;
using System.IO;
using ChromaFx.Formats.Jpeg.Format;
using ChromaFx;

namespace ChromaFx.Benchmarks
{
    [CPUUsageDiagnoser]
    public class JpegFileEncodingBenchmarks
    {
        private Image _image;
        [GlobalSetup]
        public void Setup()
        {
            // Prepare a simple test image in memory for encoding
            _image = new Image(512, 512);
        }

        [Benchmark(Description = "ChromaFx JPEG Encode")]
        public void EncodeChromaFx()
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);
            var file = new ChromaFx.Formats.Jpeg.Format.File();
            file.Write(writer, _image);
        }
    }
}