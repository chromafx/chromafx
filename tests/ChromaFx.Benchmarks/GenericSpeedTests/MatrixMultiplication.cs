using BenchmarkDotNet.Attributes;
using ChromaFx.Core.ExtensionMethods;
using ChromaFx.Processing.Numerics;
using System.Numerics;

namespace ChromaFx.Benchmarks.GenericSpeedTests;

public class MatrixMultiplication
{
    private readonly Matrix5X5 _value = new(
        1, 0, 0, 0, 0,
        0, 1, 0, 0, 0,
        0, 0, 1, 0, 0,
        .5f, .5f, .5f, 0, 0,
        0, 0, 0, 0, 1
    );

    [Benchmark(Description = "Byte matrix multiplication")]
    public void ByteMatrixMultiplication()
    {
        const float r = 128;
        const float g = 127;
        const float b = 126;
        const float a = 255;
#pragma warning disable IDE0059
        var finalValue = new[]
        {
            (byte)(r* _value.M11 + g* _value.M21 + b* _value.M31 + a* _value.M41 + _value.M51*255f).Clamp(0,255),
            (byte)(r* _value.M12 + g* _value.M22 + b* _value.M32 + a* _value.M42 + _value.M52*255f).Clamp(0,255),
            (byte)(r* _value.M13 + g* _value.M23 + b* _value.M33 + a* _value.M43 + _value.M53*255f).Clamp(0,255),
            (byte)(r* _value.M14 + g* _value.M24 + b* _value.M34 + a* _value.M44 + _value.M54*255f).Clamp(0,255)
        };
#pragma warning restore IDE0059
    }

    [Benchmark(Description = "Float matrix multiplication")]
    public void FloatMatrixMultiplication()
    {
        const float r = 128 / 255f;
        const float g = 127 / 255f;
        const float b = 126 / 255f;
        const float a = 255 / 255f;
        var finalValue = new Vector4(r * _value.M11 + g * _value.M21 + b * _value.M31 + a * _value.M41 + _value.M51,
            r * _value.M12 + g * _value.M22 + b * _value.M32 + a * _value.M42 + _value.M52,
            r * _value.M13 + g * _value.M23 + b * _value.M33 + a * _value.M43 + _value.M53,
            r * _value.M14 + g * _value.M24 + b * _value.M34 + a * _value.M44 + _value.M54);
#pragma warning disable IDE0059 // Unnecessary assignment of a value
        finalValue = Vector4.Clamp(finalValue, Vector4.Zero, Vector4.One) * 255f;
#pragma warning restore IDE0059 // Unnecessary assignment of a value
    }
}