using ChromaFx.IO.Formats.Bmp.Format;
using Xunit;

namespace ChromaFx.Tests.Formats.Bmp.Format;

public class Palette
{
    [Fact]
    public void CreateByteArray()
    {
        var data = new byte[1024];
        var testFileHeader = new IO.Formats.Bmp.Format.Palette(256, data);
        Assert.Equal(256, testFileHeader.NumberOfColors);
        Assert.Equal(1024, testFileHeader.Data.Length);
    }

    [Fact]
    public void Read()
    {
        var data = new byte[1024];
        using var stream = new MemoryStream(data);
        var testFileHeader = ChromaFx.IO.Formats.Bmp.Format.Palette.Read(new IO.Formats.Bmp.Format.Header(1, 1, 24, 1, 0, 0, 256, 0, Compression.Rgb), stream);
        Assert.Equal(256, testFileHeader.NumberOfColors);
        Assert.Equal(1024, testFileHeader.Data.Length);
    }

    [Fact]
    public void Write()
    {
        var data = new byte[1024];
        var testFileHeader = new IO.Formats.Bmp.Format.Palette(256, data);
        using var bWriter = new BinaryWriter(new MemoryStream());
        ChromaFx.IO.Formats.Bmp.Format.Palette.Write(bWriter);
        Assert.Equal(0, bWriter.BaseStream.Length);
    }
}