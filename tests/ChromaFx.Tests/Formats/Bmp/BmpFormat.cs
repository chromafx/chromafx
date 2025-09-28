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
        "Test1.bmp"
    ];

    [Fact]
    public void CanDecodeByteArray()
    {
        Assert.True(new IO.Formats.Bmp.BmpFormat().CanDecode(BitConverter.GetBytes(19778)));
        Assert.False(new IO.Formats.Bmp.BmpFormat().CanDecode(BitConverter.GetBytes(19777)));
        Assert.False(new IO.Formats.Bmp.BmpFormat().CanDecode(BitConverter.GetBytes(19779)));
    }

    [Fact]
    public void CanDecodeFileName()
    {
        Assert.True(new IO.Formats.Bmp.BmpFormat().CanDecode("test.bmp"));
        Assert.True(new IO.Formats.Bmp.BmpFormat().CanDecode("test.dib"));
        Assert.True(new IO.Formats.Bmp.BmpFormat().CanDecode("TEST.BMP"));
        Assert.True(new IO.Formats.Bmp.BmpFormat().CanDecode("TEST.DIB"));
        Assert.False(new IO.Formats.Bmp.BmpFormat().CanDecode("test.jpg"));
        Assert.False(new IO.Formats.Bmp.BmpFormat().CanDecode("BMP.jpg"));
    }

    [Fact]
    public void CanDecodeStream()
    {
        Assert.True(new IO.Formats.Bmp.BmpFormat().CanDecode(new MemoryStream(BitConverter.GetBytes(19778))));
        Assert.False(new IO.Formats.Bmp.BmpFormat().CanDecode(new MemoryStream(BitConverter.GetBytes(19777))));
        Assert.False(new IO.Formats.Bmp.BmpFormat().CanDecode(new MemoryStream(BitConverter.GetBytes(19779))));
    }

    [Fact]
    public void CanEncode()
    {
        Assert.True(new IO.Formats.Bmp.BmpFormat().CanEncode("ASDF.bmp"));
        Assert.True(new IO.Formats.Bmp.BmpFormat().CanEncode("ASDF.dib"));
        Assert.False(new IO.Formats.Bmp.BmpFormat().CanEncode("ASDF.jpg"));
        Assert.False(new IO.Formats.Bmp.BmpFormat().CanEncode("bmp.gif"));
    }

    [Fact]
    public void Decode()
    {
        using var tempFile = File.OpenRead("./TestImages/Formats/Bmp/EncodingTest.bmp");
        var imageFormat = new IO.Formats.Bmp.BmpFormat();
        var tempImage = imageFormat.Decode(tempFile);
        Assert.Equal(1760, tempImage.Pixels.Length);
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