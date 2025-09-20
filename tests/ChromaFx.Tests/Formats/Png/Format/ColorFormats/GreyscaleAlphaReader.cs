using ChromaFx.Colors;
using Xunit;

namespace ChromaFx.Tests.Formats.Png.Format.ColorFormats;

public class GreyscaleAlphaReader
{
    [Fact]
    public void ReadScanline()
    {
        var data = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };
        var result = new Color[10];
        var expectedResult = new[] {
            new Color(1, 1, 1, 2),
            new Color(3, 3, 3, 4),
            new Color(5, 5, 5, 6),
            new Color(7, 7, 7, 8),
            new Color(9, 9, 9, 0),
            new Color(1, 1, 1, 2),
            new Color(3, 3, 3, 4),
            new Color(5, 5, 5, 6),
            new Color(7, 7, 7, 8),
            new Color(9, 9, 9, 0)
        };
        var testObject = new ChromaFx.Formats.Png.Format.ColorFormats.GreyscaleAlphaReader();
        testObject.ReadScanline(data, result, new ChromaFx.Formats.Png.Format.Header(10, 1, 8, 4, 0, 0, 0), 0);
        Assert.Equal(expectedResult, result);
    }
}