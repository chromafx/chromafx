using ChromaFx.Tests.Formats.BaseClasses;
using Xunit;

namespace ChromaFx.Tests.Formats.Bmp;

public class BmpFormat : FormatTestBase
{
    public override string ExpectedDirectory => "./ExpectedResults/Formats/Bmp/";

    public override string InputDirectory => "./TestImages/Formats/Bmp/";

    public override string OutputDirectory => "./TestOutput/Formats/Bmp/";

    public static readonly TheoryData<string> InputFileNames =
    [
        "Car.bmp",
        "Test24.bmp",
        "EncodingTest.bmp",
        "Test8.bmp",
        "Test4.bmp",
        "Test16.bmp",
        "Test32.bmp",
        "TestRLE8.bmp",
        "Test1.bmp",
    ];

    [Fact]
    public void Capabilities()
    {
        var format = new IO.Formats.Bmp.BmpFormat();
        Assert.True(format.CanDecode(BitConverter.GetBytes(19778)));
        Assert.False(format.CanDecode(BitConverter.GetBytes(19777)));
        Assert.True(format.CanDecode("test.bmp"));
        Assert.True(format.CanDecode("test.dib"));
        Assert.False(format.CanDecode("test.jpg"));
        Assert.True(format.CanDecode(new MemoryStream(BitConverter.GetBytes(19778))));
        Assert.True(format.CanEncode("ASDF.bmp"));
        Assert.False(format.CanEncode("ASDF.jpg"));
    }

    [Fact]
    public void Decode()
    {
        using var tempFile = File.OpenRead("./TestImages/Formats/Bmp/EncodingTest.bmp");
        var tempImage = new IO.Formats.Bmp.BmpFormat().Decode(tempFile);
        Assert.Equal(1760, tempImage.PixelCount);
        Assert.Equal(44, tempImage.Width);
        Assert.Equal(40, tempImage.Height);
        Assert.Equal(44d / 40d, tempImage.PixelRatio);
    }

    [Theory]
    [MemberData(nameof(InputFileNames))]
    public void Encode(string fileName)
    {
        using (var tempFile = File.OpenRead(InputDirectory + fileName))
        {
            var imageFormat = new IO.Formats.Bmp.BmpFormat();
            var tempImage = imageFormat.Decode(tempFile);
            using var tempFile2 = File.OpenWrite(OutputDirectory + fileName);
            Assert.True(imageFormat.Encode(new BinaryWriter(tempFile2), tempImage));
        }
        Assert.True(CheckFileCorrect(fileName));
    }
}
