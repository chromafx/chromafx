using ChromaFx.Core;
using Xunit;

namespace ChromaFx.Tests.Formats;

/// <summary>
/// Decodes inputs whose expected pixels were produced by an independent decoder (Pillow) and saved
/// as RGBA PNG. Fixtures live in TestImages/Formats/Oracle as name.ext + name.expected.png.
/// </summary>
public class OracleDecodeTests
{
    private const string Directory = "./TestImages/Formats/Oracle/";

    public static readonly TheoryData<string, string> Cases = new()
    {
        { "../Bmp/Test1.bmp", "bmp_Test1" },
        { "../Bmp/Test4.bmp", "bmp_Test4" },
        { "../Bmp/Test8.bmp", "bmp_Test8" },
        { "../Bmp/TestRLE8.bmp", "bmp_TestRLE8" },
        { "bmp_rle4.bmp", "bmp_rle4" },
        { "bmp_32bit_rgb.bmp", "bmp_32bit_rgb" },
        { "bmp_topdown.bmp", "bmp_topdown" },
        { "png_gray1.png", "png_gray1" },
        { "png_gray16.png", "png_gray16" },
        { "tif_lzw.tif", "tif_lzw" },
        { "../Jpg/rgb.jpg", "jpg_rgb_9x3" },
        { "jpg_420_7x5.jpg", "jpg_420_7x5" },
    };

    [Theory]
    [MemberData(nameof(Cases))]
    public void DecodedPixelsMatchReference(string input, string expected)
    {
        var actual = Image.Load(Directory + input);
        var reference = Image.Load(Directory + expected + ".expected.png");

        Assert.Equal(reference.Width, actual.Width);
        Assert.Equal(reference.Height, actual.Height);
        for (var y = 0; y < reference.Height; y++)
        {
            var actualRow = actual.GetRowSpan(y);
            var referenceRow = reference.GetRowSpan(y);
            for (var x = 0; x < reference.Width; x++)
                Assert.True(
                    actualRow[x].Equals(referenceRow[x]),
                    $"{input} ({x},{y}): {actualRow[x]} != {referenceRow[x]}"
                );
        }
    }
}
