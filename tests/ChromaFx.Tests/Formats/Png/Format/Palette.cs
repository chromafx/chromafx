using ChromaFx.Formats.Png.Format.Enums;
using ChromaFx.Formats.Png.Format.Helpers;
using Xunit;

namespace ChromaFx.Tests.Formats.Png.Format;

public class Palette
{
    [Fact]
    public void Create()
    {
        var testObject = new ChromaFx.Formats.Png.Format.Palette(new byte[512], PaletteType.Color);
        Assert.Equal(512, testObject.Data.Length);
        Assert.Equal(PaletteType.Color, testObject.Type);
        testObject = new ChromaFx.Formats.Png.Format.Palette(new byte[12], PaletteType.Alpha);
        Assert.Equal(12, testObject.Data.Length);
        Assert.Equal(PaletteType.Alpha, testObject.Type);
    }

    [Fact]
    public void CreateFromChunk()
    {
        ChromaFx.Formats.Png.Format.Palette testObject = new ChromaFx.Formats.Png.Format.Helpers.Chunk(1, ChunkTypes.Palette, new byte[512], 12);
        Assert.Equal(512, testObject.Data.Length);
        Assert.Equal(PaletteType.Color, testObject.Type);
        testObject = new ChromaFx.Formats.Png.Format.Helpers.Chunk(1, ChunkTypes.TransparencyInfo, new byte[12], 12);
        Assert.Equal(12, testObject.Data.Length);
        Assert.Equal(PaletteType.Alpha, testObject.Type);
    }
}