using ChromaFx.IO;
using ChromaFx.IO.Formats;
using ChromaFx.Processing;
using ChromaFx.Tests.BaseClasses;
using Xunit;

namespace ChromaFx.Tests;

public class ImageTests : FilterTestBaseClass
{
    public override string ExpectedDirectory => "./ExpectedResults/Image/";

    public override string OutputDirectory => "./TestOutput/Image/";

    public static string SecondImage => "./TestImages/Formats/Bmp/Car.bmp";

    public static readonly TheoryData<string, Func<Image, int, Image>, int> ShiftOperations =
        new() { { "BrightnessUp", (x, y) => x.BrightnessUp(y), 128 }, { "BrightnessDown", (x, y) => x.BrightnessDown(y), 128 } };

    public static readonly TheoryData<string, Func<Image, Image>> UnaryOperations =
        new() { { "Invert", x => x.Invert() } };

    [Fact]
    public void BadDataConstructor()
    {
        var tempImage = new Image(-1, -1, (byte[])null);
        Assert.Equal(1, tempImage.Width);
        Assert.Equal(1, tempImage.Height);
        Assert.Equal(1, tempImage.PixelRatio);
        Assert.Null(tempImage.Pixels);
    }

    [Theory]
    [MemberData(nameof(ShiftOperations))]
    public void CheckShiftOperators(string name, Func<Image, int, Image> operation, int value)
    {
        foreach (var file in Files)
        {
            var outputFileName =
                Path.GetFileNameWithoutExtension(file) + "-" + name + Path.GetExtension(file);
            var testImage = file.LoadImage();
            var resultImage = operation(testImage, value);
            resultImage.Save(OutputDirectory + outputFileName);
        }

        foreach (
            var outputFileName in Files.Select(
                file =>
                    Path.GetFileNameWithoutExtension(file) + "-" + name + Path.GetExtension(file)
            )
        )
        {
            Assert.True(
                CheckFileCorrect(
                    ExpectedDirectory + Path.GetFileName(outputFileName),
                    OutputDirectory + Path.GetFileName(outputFileName)
                ),
                outputFileName
            );
        }
    }

    [Theory]
    [MemberData(nameof(UnaryOperations))]
    public void CheckUnaryOperators(string name, Func<Image, Image> operation)
    {
        foreach (var file in Files)
        {
            var outputFileName =
                Path.GetFileNameWithoutExtension(file) + "-" + name + Path.GetExtension(file);
            var testImage = file.LoadImage();
            var resultImage = operation(testImage);
            resultImage.Save(OutputDirectory + outputFileName);
        }

        foreach (
            var outputFileName in Files.Select(
                file =>
                    Path.GetFileNameWithoutExtension(file) + "-" + name + Path.GetExtension(file)
            )
        )
        {
            Assert.True(
                CheckFileCorrect(
                    ExpectedDirectory + Path.GetFileName(outputFileName),
                    OutputDirectory + Path.GetFileName(outputFileName)
                ),
                outputFileName
            );
        }
    }

    [Fact]
    public void NoDataConstructor()
    {
        var tempImage = new Image(1, 1);
        Assert.Equal(1, tempImage.Width);
        Assert.Equal(1, tempImage.Height);
        Assert.Equal(1, tempImage.PixelRatio);
        Assert.Equal(new Color(0, 0, 0, 0), tempImage.Pixels[0]);
    }

    [Fact]
    public void ToAsciiArt()
    {
        var testImage = new Image(
            1,
            10,
            new byte[]
            {
                25,
                51,
                76,
                102,
                127,
                153,
                178,
                204,
                229,
                255,
                25,
                51,
                76,
                102,
                127,
                153,
                178,
                204,
                229,
                255,
                25,
                51,
                76,
                102,
                127,
                153,
                178,
                204,
                229,
                255,
                25,
                51,
                76,
                102,
                127,
                153,
                178,
                204,
                229,
                255
            }
        );
        var s = Environment.NewLine;
        Assert.Equal($"#{s}.{s}-{s}*{s}={s}", testImage.ToAsciiArt());
    }

    [Fact]
    public void ToBase64String()
    {
        var testImage = new Image(
            1,
            10,
            new byte[]
            {
                25,
                51,
                76,
                102,
                127,
                153,
                178,
                204,
                229,
                255,
                25,
                51,
                76,
                102,
                127,
                153,
                178,
                204,
                229,
                255,
                25,
                51,
                76,
                102,
                127,
                153,
                178,
                204,
                229,
                255,
                25,
                51,
                76,
                102,
                127,
                153,
                178,
                204,
                229,
                255
            }
        );
        Assert.Equal(
            "Qk1eAAAAAAAAADYAAAAoAAAAAQAAAAoAAAABABgAAAAAACgAAAAAAAAAAAAAAAAAAAAAAAAA5cyyAH9mTAAZ/+UAspl/AEwzGQDlzLIAf2ZMABn/5QCymX8ATDMZAA==",
            testImage.ToBase64String(FileFormats.Bmp)
        );
    }
}
