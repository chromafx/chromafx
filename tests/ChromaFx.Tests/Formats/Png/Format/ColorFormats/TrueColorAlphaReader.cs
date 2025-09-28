using Xunit;

namespace ChromaFx.Tests.Formats.Png.Format.ColorFormats;

public class TrueColorAlphaReader
{
    [Fact]
    public void ReadScanline()
    {
        var data = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };
        var result = new Color[10];
        var expectedResult = new[] {
            new Color(1, 2, 3, 4),
            new Color(5, 6, 7, 8),
            new Color(9, 0, 1, 2),
            new Color(3, 4, 5, 6),
            new Color(7, 8, 9, 0),
            new Color(1, 2, 3, 4),
            new Color(5, 6, 7, 8),
            new Color(9, 0, 1, 2),
            new Color(3, 4, 5, 6),
            new Color(7, 8, 9, 0)
        };
        var testObject = new IO.Formats.Png.Format.ColorFormats.TrueColorAlphaReader();
        testObject.ReadScanline(data, result, new IO.Formats.Png.Format.Header(10, 1, 8, 6, 0, 0, 0), 0);
        Assert.Equal(expectedResult, result);
    }
}