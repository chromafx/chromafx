using ChromaFx.IO.Formats.Bmp.Format;
using Xunit;

namespace ChromaFx.Tests.Formats.Bmp.Format;

public class Body
{
    [Fact]
    public void CreateByteArray()
    {
        var data = new byte[1600];
        var testBody = new IO.Formats.Bmp.Format.Body(data);
        Assert.Equal(1600, testBody.Data.Length);
    }

    [Fact]
    public void CreateImage()
    {
        var data = new byte[1600];
        var image = new Image(10, 40, data);
        var testBody = new IO.Formats.Bmp.Format.Body(image, new IO.Formats.Bmp.Format.Header(10, 40, 24, 1280, 0, 0, 0, 0, Compression.Rgb));
        Assert.Equal(1280, testBody.Data.Length);
    }

    [Fact]
    public void Read()
    {
        var data = new byte[5280];
        using var stream = new MemoryStream(data);
        var testBody = ChromaFx.IO.Formats.Bmp.Format.Body.Read(new IO.Formats.Bmp.Format.Header(44, 40, 24, 5280, 0, 0, 0, 0, Compression.Rgb), null, stream);
        Assert.Equal(7040, testBody.Data.Length);
    }

    [Fact]
    public void Write()
    {
        var data = new byte[7040];
        var image = new Image(44, 40, data);
        var testBody = new IO.Formats.Bmp.Format.Body(image, new IO.Formats.Bmp.Format.Header(44, 40, 24, 1280, 0, 0, 0, 0, Compression.Rgb));
        using var bWriter = new BinaryWriter(new MemoryStream());
        testBody.Write(bWriter);
        Assert.Equal(5280, bWriter.BaseStream.Length);
    }
}