using ChromaFx.Tests.Formats.BaseClasses;
using Xunit;

namespace ChromaFx.Tests.Formats.Gif;

public class GifFormat : FormatTestBase
{
    public override string ExpectedDirectory => "./ExpectedResults/Formats/Gif/";

    public override string InputDirectory => "./TestImages/Formats/Gif/";

    public override string OutputDirectory => "./TestOutput/Formats/Gif/";

    public static readonly TheoryData<string> InputFileNames = new()
    {
        "giphy.gif",
        "rings.gif"
    };

    [Theory]
    [MemberData(nameof(InputFileNames))]
    public void Encode(string fileName)
    {
        using (var tempFile = File.OpenRead(InputDirectory + fileName))
        {
            var imageFormat = new ChromaFx.Formats.Gif.GifFormat();
            var tempImage = imageFormat.DecodeAnimation(tempFile);
            using var tempFile2 = File.OpenWrite(OutputDirectory + fileName);
            Assert.True(imageFormat.Encode(new BinaryWriter(tempFile2), tempImage));
        }
        Assert.True(CheckFileCorrect(fileName));
    }
}