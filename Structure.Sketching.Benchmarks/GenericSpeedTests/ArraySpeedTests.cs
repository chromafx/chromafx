﻿using BenchmarkDotNet.Attributes;
using Structure.Sketching.Benchmarks.GenericSpeedTests.TestClasses;
using System.Numerics;

namespace Structure.Sketching.Benchmarks.ArraySpeedTests;

public class ArraySpeedTests
{
    [Benchmark(Baseline = true, Description = "Byte array manipulation")]
    public static void ByteArray()
    {
        var testArray = new byte[40000];
        for (var x = 0; x < testArray.Length; ++x)
        {
            testArray[x] *= 3;
        }
    }

    [Benchmark(Description = "ColorStruct array manipulation")]
    public static void ColorStructArray()
    {
        var testArray = new ColorStruct[10000];
        for (var x = 0; x < testArray.Length; ++x)
        {
            testArray[x].Red *= 3;
            testArray[x].Green *= 3;
            testArray[x].Blue *= 3;
            testArray[x].Alpha *= 3;
        }
    }

    [Benchmark(Description = "ColorStruct array manipulation 2")]
    public static void ColorStructArray2()
    {
        var testArray = new ColorStruct[10000];
        for (var x = 0; x < testArray.Length; ++x)
        {
            testArray[x] *= 3;
        }
    }

    [Benchmark(Description = "Float array manipulation")]
    public static void FloatArray()
    {
        var testArray = new float[40000];
        for (var x = 0; x < testArray.Length; ++x)
        {
            testArray[x] *= 3;
        }
    }

    [Benchmark(Description = "Unsigned int array manipulation")]
    public static void UIntArray()
    {
        var testArray = new uint[10000];
        for (var x = 0; x < testArray.Length; ++x)
        {
            testArray[x] = (uint)((byte)((testArray[x] & 0x00FF0000) * 3) << 16
                                  | ((byte)((testArray[x] & 0x0000FF00) * 3) << 8)
                                  | (byte)((testArray[x] & 0x000000FF) * 3)
                                  | ((byte)((testArray[x] & 0xFF000000) * 3) << 24));
        }
    }

    [Benchmark(Description = "Vector4 array manipulation")]
    public static void Vector4Array()
    {
        var testArray = new Vector4[10000];
        for (var x = 0; x < testArray.Length; ++x)
        {
            testArray[x] = testArray[x] * 3;
        }
    }
}