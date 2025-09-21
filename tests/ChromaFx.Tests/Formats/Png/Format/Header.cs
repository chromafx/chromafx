using Xunit;

namespace ChromaFx.Tests.Formats.Png.Format;

public class Header
{
    [Fact]
    public void Create()
    {
        var testObject = new ChromaFx.Formats.Png.Format.Header(100, 101, 8, 6, 8, 7, 7);
        Assert.Equal(8, testObject.BitDepth);
        Assert.Equal(6, (byte)testObject.ColorType);
        Assert.Equal(8, testObject.CompressionMethod);
        Assert.Equal(7, testObject.FilterMethod);
        Assert.Equal(101, testObject.Height);
        Assert.Equal(7, testObject.InterlaceMethod);
        Assert.Equal(100, testObject.Width);
    }

    [Fact]
    public void CreateFromChunk()
    {
        byte[] data = [0, 0, 0, 100, 0, 0, 0, 101, 8, 6, 8, 7, 7];
        ChromaFx.Formats.Png.Format.Header testObject = new ChromaFx.Formats.Png.Format.Helpers.Chunk(12, "ASDF", data, 12);
        Assert.Equal(8, testObject.BitDepth);
        Assert.Equal(6, (byte)testObject.ColorType);
        Assert.Equal(8, testObject.CompressionMethod);
        Assert.Equal(7, testObject.FilterMethod);
        Assert.Equal(101, testObject.Height);
        Assert.Equal(7, testObject.InterlaceMethod);
        Assert.Equal(100, testObject.Width);
    }
}