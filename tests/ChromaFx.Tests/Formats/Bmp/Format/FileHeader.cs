using Xunit;

namespace ChromaFx.Tests.Formats.Bmp.Format;

public class FileHeader
{
    [Fact]
    public void CreateByteArray()
    {
        var data = new[] { BitConverter.GetBytes((short)19778), BitConverter.GetBytes(1000), BitConverter.GetBytes(0), BitConverter.GetBytes(54) }.SelectMany(x => x).ToArray();
        var testFileHeader = new IO.Formats.Bmp.Format.FileHeader(data);
        Assert.Equal(1000, testFileHeader.FileSize);
        Assert.Equal(54, testFileHeader.Offset);
        Assert.Equal(0, ChromaFx.IO.Formats.Bmp.Format.FileHeader.Reserved);
        Assert.Equal(19778, ChromaFx.IO.Formats.Bmp.Format.FileHeader.Type);
    }

    [Fact]
    public void CreateValues()
    {
        var testFileHeader = new IO.Formats.Bmp.Format.FileHeader(1000, 54);
        Assert.Equal(1000, testFileHeader.FileSize);
        Assert.Equal(54, testFileHeader.Offset);
        Assert.Equal(0, ChromaFx.IO.Formats.Bmp.Format.FileHeader.Reserved);
        Assert.Equal(19778, ChromaFx.IO.Formats.Bmp.Format.FileHeader.Type);
    }

    [Fact]
    public void Read()
    {
        var data = new[] { BitConverter.GetBytes((short)19778), BitConverter.GetBytes(1000), BitConverter.GetBytes(0), BitConverter.GetBytes(54) }.SelectMany(x => x).ToArray();
        using var stream = new MemoryStream(data);
        var testFileHeader = ChromaFx.IO.Formats.Bmp.Format.FileHeader.Read(stream);
        Assert.Equal(1000, testFileHeader.FileSize);
        Assert.Equal(54, testFileHeader.Offset);
        Assert.Equal(0, ChromaFx.IO.Formats.Bmp.Format.FileHeader.Reserved);
        Assert.Equal(19778, ChromaFx.IO.Formats.Bmp.Format.FileHeader.Type);
    }

    [Fact]
    public void Write()
    {
        var testFileHeader = new IO.Formats.Bmp.Format.FileHeader(1000, 54);
        using var bWriter = new BinaryWriter(new MemoryStream());
        testFileHeader.Write(bWriter);
        Assert.Equal(14, bWriter.BaseStream.Length);
    }
}