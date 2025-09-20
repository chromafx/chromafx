using ChromaFx.Tests.Formats.BaseClasses;
using Xunit;

namespace ChromaFx.Tests.Formats.Bmp;

public class Decoder : FormatTestBase
{
    public override string ExpectedDirectory => "./ExpectedResults/Formats/Bmp/Decoder/";

    public override string InputDirectory => "./TestImages/Formats/Bmp/Decoder/";

    public override string OutputDirectory => "./TestOutput/Formats/Bmp/Decoder/";

    public static readonly TheoryData<string> InputFileNames = new()
    {
        "Car.bmp",
        "Test24.bmp",
        "EncodingTest.bmp",
        "Test8.bmp",
        "Test4.bmp",
        "Test16.bmp",
        "Test32.bmp",
        "TestRLE8.bmp",
        "Test1.bmp"
    };

    [Fact]
    public void CanDecodeByteArray()
    {
        Assert.True(new ChromaFx.Formats.Bmp.Decoder().CanDecode(BitConverter.GetBytes(19778)));
        Assert.False(new ChromaFx.Formats.Bmp.Decoder().CanDecode(BitConverter.GetBytes(19777)));
        Assert.False(new ChromaFx.Formats.Bmp.Decoder().CanDecode(BitConverter.GetBytes(19779)));
    }

    [Fact]
    public void CanDecodeFileName()
    {
        Assert.True(new ChromaFx.Formats.Bmp.Decoder().CanDecode("test.bmp"));
        Assert.True(new ChromaFx.Formats.Bmp.Decoder().CanDecode("test.dib"));
        Assert.True(new ChromaFx.Formats.Bmp.Decoder().CanDecode("TEST.BMP"));
        Assert.True(new ChromaFx.Formats.Bmp.Decoder().CanDecode("TEST.DIB"));
        Assert.False(new ChromaFx.Formats.Bmp.Decoder().CanDecode("test.jpg"));
        Assert.False(new ChromaFx.Formats.Bmp.Decoder().CanDecode("BMP.jpg"));
    }

    [Fact]
    public void CanDecodeStream()
    {
        Assert.True(new ChromaFx.Formats.Bmp.Decoder().CanDecode(new MemoryStream(BitConverter.GetBytes(19778))));
        Assert.False(new ChromaFx.Formats.Bmp.Decoder().CanDecode(new MemoryStream(BitConverter.GetBytes(19777))));
        Assert.False(new ChromaFx.Formats.Bmp.Decoder().CanDecode(new MemoryStream(BitConverter.GetBytes(19779))));
    }

    [Fact]
    public void Decode()
    {
        using var tempFile = File.OpenRead("./TestImages/Formats/Bmp/EncodingTest.bmp");
        var tempDecoder = new ChromaFx.Formats.Bmp.Decoder();
        var tempImage = tempDecoder.Decode(tempFile);
        Assert.Equal(1760, tempImage.Pixels.Length);
        Assert.Equal(44, tempImage.Width);
        Assert.Equal(40, tempImage.Height);
        Assert.Equal(1.1d, tempImage.PixelRatio);
    }
}