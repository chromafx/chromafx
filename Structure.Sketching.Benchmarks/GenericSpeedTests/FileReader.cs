﻿using BenchmarkDotNet.Attributes;
using System.IO;

namespace Structure.Sketching.Benchmarks.GenericSpeedTests;

public class FileReader
{
    [Benchmark(Description = "File.Read")]
    public static void FileRead()
    {
        using var testStream = File.Open("../../../../TestImage/BitmapFilter.bmp", FileMode.Open);
        var data = new byte[testStream.Length];
        testStream.Read(data, 0, (int)testStream.Length);
    }

    [Benchmark(Description = "File.Read in loop, 1024")]
    public static void FileReadLoop1024()
    {
        var data = new byte[1024];
        using var testStream = File.Open("../../../../TestImage/BitmapFilter.bmp", FileMode.Open);
        while (testStream.Read(data, 0, 1024) == 1024) { }
    }

    [Benchmark(Description = "File.Read in loop, 2048")]
    public static void FileReadLoop2048()
    {
        var data = new byte[2048];
        using var testStream = File.Open("../../../../TestImage/BitmapFilter.bmp", FileMode.Open);
        while (testStream.Read(data, 0, 2048) == 2048) { }
    }

    [Benchmark(Baseline = true, Description = "File.Read in loop, 4096")]
    public static void FileReadLoop4096()
    {
        var data = new byte[4096];
        using var testStream = File.Open("../../../../TestImage/BitmapFilter.bmp", FileMode.Open);
        while (testStream.Read(data, 0, 4096) == 4096) { }
    }

    [Benchmark(Description = "File.ReadAllBytes")]
    public static void ReadAllBytes()
    {
#pragma warning disable IDE0059 // Unnecessary assignment of a value
        var data = File.ReadAllBytes("../../../../TestImage/BitmapFilter.bmp");
#pragma warning restore IDE0059 // Unnecessary assignment of a value
    }
}