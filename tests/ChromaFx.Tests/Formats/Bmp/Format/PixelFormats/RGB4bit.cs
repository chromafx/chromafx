using ChromaFx.Formats.Bmp.Format;
using ChromaFx.Formats.Bmp.Format.PixelFormats.Interfaces;
using ChromaFx.Tests.Formats.Bmp.Format.PixelFormats.BaseClasses;
using Xunit;

namespace ChromaFx.Tests.Formats.Bmp.Format.PixelFormats;

public class Rgb4Bit : FormatBaseFixture
{
    public override string FileName => "./TestImages/Formats/Bmp/Test4.bmp";
    public override IPixelFormat Format => new ChromaFx.Formats.Bmp.Format.PixelFormats.Rgb4Bit();

    [Fact]
    public void Decode()
    {
        using var tempFile = System.IO.File.Open(FileName, FileMode.Open, FileAccess.Read);
        var data = Format.Read(new ChromaFx.Formats.Bmp.Format.Header(44, 40, 0, 880, 0, 0, 0, 0, Compression.Rgb), tempFile);
        data = Format.Decode(new ChromaFx.Formats.Bmp.Format.Header(44, 40, 0, 880, 0, 0, 0, 0, Compression.Rgb), data, new ChromaFx.Formats.Bmp.Format.Palette(16, new byte[64]));
        Assert.Equal(7040, data.Length);
    }

    [Fact]
    public void Encode()
    {
        using var tempFile = System.IO.File.Open(FileName, FileMode.Open, FileAccess.Read);
        var data = Format.Read(new ChromaFx.Formats.Bmp.Format.Header(44, 40, 0, 880, 0, 0, 0, 0, Compression.Rgb), tempFile);
        data = Format.Decode(new ChromaFx.Formats.Bmp.Format.Header(44, 40, 0, 880, 0, 0, 0, 0, Compression.Rgb), data, new ChromaFx.Formats.Bmp.Format.Palette(16, new byte[64]));
        data = Format.Encode(new ChromaFx.Formats.Bmp.Format.Header(44, 40, 0, 880, 0, 0, 0, 0, Compression.Rgb), data, new ChromaFx.Formats.Bmp.Format.Palette(16, new byte[64]));
        Assert.Equal(7040, data.Length);
    }

    [Fact]
    public void Read()
    {
        using var tempFile = System.IO.File.Open(FileName, FileMode.Open, FileAccess.Read);
        var data = Format.Read(new ChromaFx.Formats.Bmp.Format.Header(44, 40, 0, 880, 0, 0, 0, 0, Compression.Rgb), tempFile);
        Assert.Equal(1760, data.Length);
    }
}