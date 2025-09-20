using ChromaFx.Colors;
using Xunit;

namespace ChromaFx.Tests.Formats.Png.Format.ColorFormats;

public class TrueColorNoAlphaReader
{
    [Fact]
    public void ReadScanline()
    {
        var data = new byte[] {
            1, 2, 3,
            4, 5, 6,
            7, 8, 9,
            0, 1, 2,
            3, 4, 5,
            6, 7, 8,
            9, 0, 1,
            2, 3, 4,
            5, 6, 7,
            8, 9, 0
        };
        var result = new Color[10];
        var expectedResult = new[] {
            new Color(1, 2, 3),
            new Color(4, 5, 6),
            new Color(7, 8, 9),
            new Color(0, 1, 2),
            new Color(3, 4, 5),
            new Color(6, 7, 8),
            new Color(9, 0, 1),
            new Color(2, 3, 4),
            new Color(5, 6, 7),
            new Color(8, 9)
        };
        var testObject = new ChromaFx.Formats.Png.Format.ColorFormats.TrueColorNoAlphaReader();
        testObject.ReadScanline(data, result, new ChromaFx.Formats.Png.Format.Header(10, 1, 8, 3, 0, 0, 0), 0);
        Assert.Equal(expectedResult, result);
    }
}