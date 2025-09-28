using Xunit;

namespace ChromaFx.Tests.Formats.Png.Format.Helpers;

public class Chunk
{
    [Fact]
    public void Create()
    {
        var testObject = new IO.Formats.Png.Format.Helpers.Chunk(10, "ASDF", [1, 2, 3, 4], 12);
        Assert.Equal(10, testObject.Length);
        Assert.Equal("ASDF", testObject.Type);
        Assert.Equal(new byte[] { 1, 2, 3, 4 }, testObject.Data);
        Assert.Equal((uint)12, testObject.Crc);
    }
}