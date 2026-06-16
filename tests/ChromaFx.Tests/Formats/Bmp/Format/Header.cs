using ChromaFx.IO.Formats.Bmp.Format;
using Xunit;

namespace ChromaFx.Tests.Formats.Bmp.Format;

public class Header
{
    [Fact]
    public void CreateByteArray()
    {
        var data = new[]
        {
            BitConverter.GetBytes(200),
            BitConverter.GetBytes(44),
            BitConverter.GetBytes(40),
            BitConverter.GetBytes((short)1),
            BitConverter.GetBytes((short)24),
            BitConverter.GetBytes(0),
            BitConverter.GetBytes(1000),
            BitConverter.GetBytes(0),
            BitConverter.GetBytes(0),
            BitConverter.GetBytes(0),
            BitConverter.GetBytes(0)
        }.SelectMany(x => x).ToArray();
        var testFileHeader = new IO.Formats.Bmp.Format.Header(data);
        Assert.Equal(24, testFileHeader.Bpp);
        Assert.Equal(0, testFileHeader.ColorsImportant);
        Assert.Equal(0, testFileHeader.ColorsUsed);
        Assert.Equal(Compression.Rgb, testFileHeader.Compression);
        Assert.Equal(40, testFileHeader.Height);
        Assert.Equal(1000, testFileHeader.ImageSize);
        Assert.Equal(1, ChromaFx.IO.Formats.Bmp.Format.Header.Planes);
        Assert.Equal(44, testFileHeader.Width);
        Assert.Equal(0, testFileHeader.Xppm);
        Assert.Equal(0, testFileHeader.Yppm);
    }

    [Fact]
    public void CreateValues()
    {
        var testFileHeader = new IO.Formats.Bmp.Format.Header(44, 40, 24, 1000, 0, 0, 0, 0, Compression.Rgb);
        Assert.Equal(24, testFileHeader.Bpp);
        Assert.Equal(0, testFileHeader.ColorsImportant);
        Assert.Equal(0, testFileHeader.ColorsUsed);
        Assert.Equal(Compression.Rgb, testFileHeader.Compression);
        Assert.Equal(40, testFileHeader.Height);
        Assert.Equal(1000, testFileHeader.ImageSize);
        Assert.Equal(1, ChromaFx.IO.Formats.Bmp.Format.Header.Planes);
        Assert.Equal(44, testFileHeader.Width);
        Assert.Equal(0, testFileHeader.Xppm);
        Assert.Equal(0, testFileHeader.Yppm);
    }

    [Fact]
    public void Read()
    {
        var expected = new IO.Formats.Bmp.Format.Header(44, 40, 24, 1000, 0, 0, 0, 0, Compression.Rgb);
        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, leaveOpen: true))
        {
            expected.Write(writer);
        }
        stream.Position = 0;
        var testFileHeader = ChromaFx.IO.Formats.Bmp.Format.Header.Read(stream);
        Assert.Equal(24, testFileHeader.Bpp);
        Assert.Equal(0, testFileHeader.ColorsImportant);
        Assert.Equal(0, testFileHeader.ColorsUsed);
        Assert.Equal(Compression.Rgb, testFileHeader.Compression);
        Assert.Equal(40, testFileHeader.Height);
        Assert.Equal(1000, testFileHeader.ImageSize);
        Assert.Equal(1, ChromaFx.IO.Formats.Bmp.Format.Header.Planes);
        Assert.Equal(44, testFileHeader.Width);
        Assert.Equal(0, testFileHeader.Xppm);
        Assert.Equal(0, testFileHeader.Yppm);
    }

    [Fact]
    public void Write()
    {
        var testFileHeader = new IO.Formats.Bmp.Format.Header(44, 40, 24, 1000, 0, 0, 0, 0, Compression.Rgb);
        using var bWriter = new BinaryWriter(new MemoryStream());
        testFileHeader.Write(bWriter);
        Assert.Equal(40, bWriter.BaseStream.Length);
    }
}