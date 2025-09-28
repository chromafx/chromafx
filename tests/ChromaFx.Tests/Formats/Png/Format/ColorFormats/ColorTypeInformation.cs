using Xunit;

namespace ChromaFx.Tests.Formats.Png.Format.ColorFormats;

public class ColorTypeInformation
{
    [Fact]
    public void Create()
    {
        var testObject = new IO.Formats.Png.Format.ColorFormats.ColorTypeInformation(1, [1], (_, _) => null);
        Assert.Equal(1, testObject.ScanlineFactor);
        Assert.Single(testObject.SupportedBitDepths);
        Assert.Equal(1, testObject.SupportedBitDepths[0]);
        Assert.Null(testObject.ScanlineReaderFactory(null, null));
    }
}