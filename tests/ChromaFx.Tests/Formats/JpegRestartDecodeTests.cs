using ChromaFx.Core;
using Xunit;

namespace ChromaFx.Tests.Formats;

/// <summary>
/// Decodes inputs whose expected pixels were produced by an independent decoder (Pillow) and saved
/// as RGBA PNG. Fixtures live in TestImages/Formats/Oracle as name.ext + name.expected.png.
/// </summary>
public class JpegRestartDecodeTests
{
    private const string Directory = "./TestImages/Formats/Oracle/";
    private const int Tolerance = 3;

    [Theory]
    [InlineData("jpg_rst_420")]
    [InlineData("jpg_rst_444")]
    [InlineData("jpg_prog_rst")]
    [InlineData("jpg_gray_rst")]
    public void RestartIntervalsDecodeCloseToReference(string name)
    {
        var actual = Image.Load(Directory + name + ".jpg");
        var reference = Image.Load(Directory + name + ".expected.png");

        Assert.Equal(reference.Width, actual.Width);
        Assert.Equal(reference.Height, actual.Height);
        for (var y = 0; y < reference.Height; y++)
        {
            var actualRow = actual.GetRowSpan(y);
            var referenceRow = reference.GetRowSpan(y);
            for (var x = 0; x < reference.Width; x++)
            {
                var a = actualRow[x];
                var r = referenceRow[x];
                var delta = Math.Max(
                    Math.Max(Math.Abs(a.Red - r.Red), Math.Abs(a.Green - r.Green)),
                    Math.Max(Math.Abs(a.Blue - r.Blue), Math.Abs(a.Alpha - r.Alpha))
                );
                Assert.True(delta <= Tolerance, $"{name} ({x},{y}): {a} vs {r}");
            }
        }
    }

    [Fact]
    public void TruncatedFileThrows() =>
        Assert.ThrowsAny<Exception>(() => Image.Load(Directory + "jpg_truncated.jpg"));
}
