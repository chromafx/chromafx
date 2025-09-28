using Xunit;

namespace ChromaFx.Tests.Formats.Png.Format.Helpers;

public class ChunkTypes
{
    [Fact]
    public void Create()
    {
        var testObject = new IO.Formats.Png.Format.Helpers.ChunkTypes("ASDF");
        Assert.Equal("ASDF", testObject.Value);
        Assert.Equal("ASDF", testObject);
    }

    [Fact]
    public void Equality()
    {
        var testObject1 = new IO.Formats.Png.Format.Helpers.ChunkTypes("ASDF");
        var testObject2 = new IO.Formats.Png.Format.Helpers.ChunkTypes("ASDF");
        Assert.True(testObject1 == testObject2);
        Assert.True("ASDF" == testObject2);
        Assert.True(testObject1 == "ASDF");
    }

    [Fact]
    public void Inequality()
    {
        var testObject1 = new IO.Formats.Png.Format.Helpers.ChunkTypes("ASDF");
        var testObject2 = new IO.Formats.Png.Format.Helpers.ChunkTypes("ASDF2");
        Assert.False(testObject1 == testObject2);
        Assert.False("ASDF" == testObject2);
        Assert.False(testObject1 == "ASDF2");
    }
}