using System.IO;
using Xunit;

namespace ChromaFx.Tests.Formats.Png.Format;

public class FileHeader
{
    [Fact]
    public void Create()
    {
        var testObject = new ChromaFx.Formats.Png.Format.FileHeader();
        Assert.Equal(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }, ChromaFx.Formats.Png.Format.FileHeader.Header);
    }

    [Fact]
    public void CreateFromStream()
    {
        var testObject = ChromaFx.Formats.Png.Format.FileHeader.Read(new MemoryStream());
        Assert.Equal(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }, ChromaFx.Formats.Png.Format.FileHeader.Header);
    }

    [Fact]
    public void Write()
    {
        var testObject = new ChromaFx.Formats.Png.Format.FileHeader();
        using var writer = new BinaryWriter(new MemoryStream());
        testObject.Write(writer);
        writer.BaseStream.Seek(0, SeekOrigin.Begin);
        var result = new byte[8];
        writer.BaseStream.Read(result, 0, 8);
        Assert.Equal(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }, result);
    }
}