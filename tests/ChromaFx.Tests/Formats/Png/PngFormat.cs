using ChromaFx.Tests.Formats.BaseClasses;
using Xunit;

namespace ChromaFx.Tests.Formats.Png;

public class PngFormat : FormatTestBase
{
    public override string ExpectedDirectory => "./ExpectedResults/Formats/Png/";

    public override string InputDirectory => "./TestImages/Formats/Png/";

    public override string OutputDirectory => "./TestOutput/Formats/Png/";

    public static readonly TheoryData<string> InputFileNames =
        ["splash.png", "48bit.png", "blur.png", "indexed.png", "splashbw.png"];

    [Fact]
    public void CanDecodeByteArray()
    {
        byte[] header = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
        Assert.True(new IO.Formats.Png.PngFormat().CanDecode(header));
        Assert.False(new IO.Formats.Png.PngFormat().CanDecode(BitConverter.GetBytes(19777)));
        Assert.False(new IO.Formats.Png.PngFormat().CanDecode(BitConverter.GetBytes(19779)));
    }

    [Fact]
    public void CanDecodeFileName()
    {
        Assert.True(new IO.Formats.Png.PngFormat().CanDecode("test.png"));
        Assert.False(new IO.Formats.Png.PngFormat().CanDecode("test.dib"));
        Assert.True(new IO.Formats.Png.PngFormat().CanDecode("TEST.PNG"));
        Assert.False(new IO.Formats.Png.PngFormat().CanDecode("TEST.DIB"));
        Assert.False(new IO.Formats.Png.PngFormat().CanDecode("test.jpg"));
        Assert.False(new IO.Formats.Png.PngFormat().CanDecode("PNG.jpg"));
    }

    [Fact]
    public void CanDecodeStream()
    {
        byte[] header = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
        Assert.True(new IO.Formats.Png.PngFormat().CanDecode(new MemoryStream(header)));
        Assert.False(
            new IO.Formats.Png.PngFormat().CanDecode(
                new MemoryStream(BitConverter.GetBytes(19777))
            )
        );
        Assert.False(
            new IO.Formats.Png.PngFormat().CanDecode(
                new MemoryStream(BitConverter.GetBytes(19779))
            )
        );
    }

    [Fact]
    public void CanEncode()
    {
        Assert.True(new IO.Formats.Png.PngFormat().CanEncode("ASDF.png"));
        Assert.False(new IO.Formats.Png.PngFormat().CanEncode("ASDF.bmp"));
        Assert.False(new IO.Formats.Png.PngFormat().CanEncode("ASDF.jpg"));
        Assert.False(new IO.Formats.Png.PngFormat().CanEncode("bmp.gif"));
    }

    [Fact]
    public void Decode()
    {
        using var tempFile = File.OpenRead("./TestImages/Formats/Png/splash.png");
        var imageFormat = new IO.Formats.Png.PngFormat();
        var tempImage = imageFormat.Decode(tempFile);
        Assert.Equal(241500, tempImage.Pixels.Length);
        Assert.Equal(500, tempImage.Width);
        Assert.Equal(483, tempImage.Height);
        Assert.Equal(500d / 483d, tempImage.PixelRatio);
    }

    [Theory]
    [MemberData(nameof(InputFileNames))]
    public void Encode(string fileName)
    {
        using (var tempFile = File.OpenRead(InputDirectory + fileName))
        {
            var imageFormat = new IO.Formats.Png.PngFormat();
            var tempImage = imageFormat.Decode(tempFile);
            using var tempFile2 = File.OpenWrite(OutputDirectory + fileName);
            Assert.True(imageFormat.Encode(new BinaryWriter(tempFile2), tempImage));
        }

        Assert.True(CheckDecodedPngCorrect(fileName));
    }
}
