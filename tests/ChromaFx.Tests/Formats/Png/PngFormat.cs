using ChromaFx.Tests.Formats.BaseClasses;
using Xunit;

namespace ChromaFx.Tests.Formats.Png;

public class PngFormat : FormatTestBase
{
    public override string ExpectedDirectory => "./ExpectedResults/Formats/Png/";

    public override string InputDirectory => "./TestImages/Formats/Png/";

    public override string OutputDirectory => "./TestOutput/Formats/Png/";

    public static readonly TheoryData<string> InputFileNames =
    [
        "splash.png",
        "48bit.png",
        "blur.png",
        "indexed.png",
        "splashbw.png",
    ];

    [Fact]
    public void Capabilities()
    {
        var format = new IO.Formats.Png.PngFormat();
        byte[] header = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
        Assert.True(format.CanDecode(header));
        Assert.False(format.CanDecode(BitConverter.GetBytes(19777)));
        Assert.True(format.CanDecode("test.png"));
        Assert.False(format.CanDecode("test.jpg"));
        Assert.True(format.CanDecode(new MemoryStream(header)));
        Assert.True(format.CanEncode("ASDF.png"));
        Assert.False(format.CanEncode("ASDF.bmp"));
    }

    [Fact]
    public void Decode()
    {
        using var tempFile = File.OpenRead("./TestImages/Formats/Png/splash.png");
        var tempImage = new IO.Formats.Png.PngFormat().Decode(tempFile);
        Assert.Equal(241500, tempImage.PixelCount);
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
            using var tempFile2 = File.Create(OutputDirectory + fileName);
            imageFormat.Encode(new BinaryWriter(tempFile2), tempImage);
        }

        Assert.True(CheckDecodedPngCorrect(fileName));
    }
}
