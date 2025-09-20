using System;
using System.IO;
using ChromaFx.Tests.Formats.BaseClasses;
using Xunit;

namespace ChromaFx.Tests.Formats.Png;

public class Decoder : FormatTestBase
{
    public override string ExpectedDirectory => "./ExpectedResults/Formats/Png/Decoder/";

    public override string InputDirectory => "./TestImages/Formats/Png/";

    public override string OutputDirectory => "./TestOutput/Formats/Png/Decoder/";

    public static readonly TheoryData<string> InputFileNames =
        new() { "splash.png", "48bit.png", "blur.png", "indexed.png", "splashbw.png", };

    [Fact]
    public void CanDecodeByteArray()
    {
        byte[] header = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        Assert.True(new ChromaFx.Formats.Png.Decoder().CanDecode(header));
        Assert.False(new ChromaFx.Formats.Png.Decoder().CanDecode(BitConverter.GetBytes(19777)));
        Assert.False(new ChromaFx.Formats.Png.Decoder().CanDecode(BitConverter.GetBytes(19779)));
    }

    [Fact]
    public void CanDecodeFileName()
    {
        Assert.True(new ChromaFx.Formats.Png.Decoder().CanDecode("test.png"));
        Assert.False(new ChromaFx.Formats.Png.Decoder().CanDecode("test.dib"));
        Assert.True(new ChromaFx.Formats.Png.Decoder().CanDecode("TEST.PNG"));
        Assert.False(new ChromaFx.Formats.Png.Decoder().CanDecode("TEST.DIB"));
        Assert.False(new ChromaFx.Formats.Png.Decoder().CanDecode("test.jpg"));
        Assert.False(new ChromaFx.Formats.Png.Decoder().CanDecode("PNG.jpg"));
    }

    [Fact]
    public void CanDecodeStream()
    {
        byte[] header = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        Assert.True(new ChromaFx.Formats.Png.Decoder().CanDecode(new MemoryStream(header)));
        Assert.False(
            new ChromaFx.Formats.Png.Decoder().CanDecode(
                new MemoryStream(BitConverter.GetBytes(19777))
            )
        );
        Assert.False(
            new ChromaFx.Formats.Png.Decoder().CanDecode(
                new MemoryStream(BitConverter.GetBytes(19779))
            )
        );
    }

    [Fact]
    public void Decode()
    {
        using var tempFile = File.OpenRead("./TestImages/Formats/Png/splash.png");
        var tempDecoder = new ChromaFx.Formats.Png.Decoder();
        var tempImage = tempDecoder.Decode(tempFile);
        Assert.Equal(241500, tempImage.Pixels.Length);
        Assert.Equal(500, tempImage.Width);
        Assert.Equal(483, tempImage.Height);
        Assert.Equal(500d / 483d, tempImage.PixelRatio);
    }
}
