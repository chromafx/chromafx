using ChromaFx.Tests.Formats.BaseClasses;
using Xunit;

namespace ChromaFx.Tests.Formats.Jpeg;

public class JpegFormat : FormatTestBase
{
    public override string ExpectedDirectory => "./ExpectedResults/Formats/Jpg/";

    public override string InputDirectory => "./TestImages/Formats/Jpg/";

    public override string OutputDirectory => "./TestOutput/Formats/Jpg/";

    public static readonly TheoryData<string> InputFileNames = new()
    {
        "Calliphora.jpg",
        "Floorplan.jpeg",
        "gamma_dalai_lama_gray.jpg",
        "rgb.jpg",
        "maltese_puppy-wide.jpg"
    };

    [Theory]
    [MemberData(nameof(InputFileNames))]
    public void Encode(string fileName)
    {
        using (var tempFile = File.OpenRead(InputDirectory + fileName))
        {
            var imageFormat = new ChromaFx.Formats.Jpeg.JpegFormat();
            var tempImage = imageFormat.Decode(tempFile);
            using var tempFile2 = File.OpenWrite(OutputDirectory + fileName);
            Assert.True(imageFormat.Encode(new BinaryWriter(tempFile2), tempImage));
        }
        Assert.True(CheckFileCorrect(fileName));
    }
}