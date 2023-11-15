﻿using Structure.Sketching.Tests.Formats.BaseClasses;
using System.IO;
using Xunit;

namespace Structure.Sketching.Tests.Formats.Bmp;

public class Encoder : FormatTestBase
{
    public override string ExpectedDirectory => "./ExpectedResults/Formats/Bmp/";

    public override string InputDirectory => "./TestImages/Formats/Bmp/";

    public override string OutputDirectory => "./TestOutput/Formats/Bmp/Encoder/";

    public static readonly TheoryData<string> InputFileNames = new()
    {
        "Car.bmp",
        "Test24.bmp",
        "EncodingTest.bmp",
        "Test8.bmp",
        "Test4.bmp",
        "Test16.bmp",
        "Test32.bmp",
        "TestRLE8.bmp",
        "Test1.bmp"
    };

    [Fact]
    public void CanEncode()
    {
        Assert.True(new Sketching.Formats.Bmp.Encoder().CanEncode("ASDF.bmp"));
        Assert.True(new Sketching.Formats.Bmp.Encoder().CanEncode("ASDF.dib"));
        Assert.False(new Sketching.Formats.Bmp.Encoder().CanEncode("ASDF.jpg"));
        Assert.False(new Sketching.Formats.Bmp.Encoder().CanEncode("bmp.gif"));
    }

    [Theory]
    [MemberData(nameof(InputFileNames))]
    public void Encode(string fileName)
    {
        using (var tempFile = File.OpenRead(InputDirectory + fileName))
        {
            var tempDecoder = new Sketching.Formats.Bmp.Decoder();
            var tempImage = tempDecoder.Decode(tempFile);
            var tempEncoder = new Sketching.Formats.Bmp.Encoder();
            using var tempFile2 = File.OpenWrite(OutputDirectory + fileName);
            tempEncoder.Encode(new BinaryWriter(tempFile2), tempImage);
        }
        Assert.True(CheckFileCorrect(fileName));
    }
}