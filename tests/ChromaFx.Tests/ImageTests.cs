using ChromaFx.Tests.BaseClasses;
using Xunit;

namespace ChromaFx.Tests;

public class ImageTests : FilterTestBaseClass
{
    public override string ExpectedDirectory => "./ExpectedResults/Image/";

    public override string OutputDirectory => "./TestOutput/Image/";

    public static string SecondImage => "./TestImages/Formats/Bmp/Car.bmp";

    public static readonly TheoryData<string, Func<Image, int, Image>, int> ShiftOperations = new()
    {
        { "BrightnessUp", (x, y) => x.Clone(c => c.Brightness(Math.Abs(y) / 255f)), 128 },
        { "BrightnessDown", (x, y) => x.Clone(c => c.Brightness(-Math.Abs(y) / 255f)), 128 },
    };

    public static readonly TheoryData<string, Func<Image, Image>> UnaryOperations = new()
    {
        { "Invert", x => x.Clone(c => c.Invert()) },
    };

    [Fact]
    public void BadDataConstructor()
    {
        var tempImage = new Image(-1, -1, (byte[])null);
        Assert.Equal(1, tempImage.Width);
        Assert.Equal(1, tempImage.Height);
        Assert.Equal(1, tempImage.PixelRatio);
        Assert.Equal(1, tempImage.PixelCount);
        Assert.Equal(new Color(0, 0, 0, 0), tempImage[0, 0]);
    }

    [Theory]
    [MemberData(nameof(ShiftOperations))]
    public void CheckShiftOperators(string name, Func<Image, int, Image> operation, int value)
    {
        foreach (var file in Files)
        {
            var outputFileName =
                Path.GetFileNameWithoutExtension(file) + "-" + name + Path.GetExtension(file);
            var testImage = Image.Load(file);
            var resultImage = operation(testImage, value);
            resultImage.Save(OutputDirectory + outputFileName);
        }

        foreach (
            var outputFileName in Files.Select(file =>
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
            var testImage = Image.Load(file);
            var resultImage = operation(testImage);
            resultImage.Save(OutputDirectory + outputFileName);
        }

        foreach (
            var outputFileName in Files.Select(file =>
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
        Assert.Equal(new Color(0, 0, 0, 0), tempImage[0, 0]);
    }

    private static Image CreateAsciiArtFixture() =>
        new(
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
                255,
            }
        );

    [Fact]
    public void ToAsciiArt()
    {
        var s = Environment.NewLine;
        Assert.Equal($":{s}-{s}:{s}-{s}:{s}", CreateAsciiArtFixture().ToAsciiArt());
    }

    [Fact]
    public void ToAsciiArt_CustomCharacters()
    {
        var art = CreateAsciiArtFixture()
            .ToAsciiArt(new AsciiArtOptions { Characters = "01" });
        var lines = art.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(5, lines.Length);
        Assert.All(lines, line => Assert.Equal(1, line.Length));
        Assert.All(lines, line => Assert.True(line[0] is '0' or '1'));
    }

    [Fact]
    public void ToAsciiArt_Invert_ReversesRamp()
    {
        // Fixture pixels have varying alpha, so Invert both reverses the ramp and
        // composites against a dark background (not a pure glyph swap of the default).
        var s = Environment.NewLine;
        Assert.Equal(
            $"-{s}-{s}*{s}:{s}+{s}",
            CreateAsciiArtFixture().ToAsciiArt(new AsciiArtOptions { Invert = true })
        );
    }

    [Fact]
    public void ToAsciiArt_FitsMaxWidthAndMaxHeight()
    {
        var pixels = new Color[16];
        Array.Fill(pixels, new Color(0, 0, 0, 255));
        var image = new Image(4, 4, pixels);

        var art = image.ToAsciiArt(new AsciiArtOptions { MaxWidth = 2, MaxHeight = 1 });
        var lines = art.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        Assert.Single(lines);
        Assert.Equal(2, lines[0].Length);
        Assert.Equal("##", lines[0]);
    }

    [Fact]
    public void ToAsciiArt_TransparentPixel_UsesBackground()
    {
        var transparent = new Image(1, 1, [new Color(0, 0, 0, 0)]);
        // Light terminal: transparent → white → space; dark (Invert): → black → space.
        Assert.Equal(" " + Environment.NewLine, transparent.ToAsciiArt());
        Assert.Equal(
            " " + Environment.NewLine,
            transparent.ToAsciiArt(new AsciiArtOptions { Invert = true })
        );

        var semi = new Image(1, 1, [new Color(0, 0, 0, 128)]);
        Assert.Equal("+" + Environment.NewLine, semi.ToAsciiArt());
    }

    [Fact]
    public void ToAnsiArt_HalfBlock_TrueColor()
    {
        // 1×2: red on top, blue on bottom → one ▀ cell with FG red / BG blue.
        var image = new Image(1, 2, [new Color(255, 0, 0), new Color(0, 0, 255)]);
        var art = image.ToAnsiArt(new AnsiArtOptions { MaxWidth = 1, MaxHeight = 1 });
        Assert.Equal(
            "\u001b[38;2;255;0;0m\u001b[48;2;0;0;255m\u2580\u001b[0m" + Environment.NewLine,
            art
        );
    }

    [Fact]
    public void ToAnsiArt_FullBlock()
    {
        var image = new Image(1, 1, [new Color(10, 20, 30)]);
        var art = image.ToAnsiArt(
            new AnsiArtOptions
            {
                MaxWidth = 1,
                MaxHeight = 1,
                UseHalfBlocks = false,
            }
        );
        Assert.Equal(
            "\u001b[38;2;10;20;30m\u2588\u001b[0m" + Environment.NewLine,
            art
        );
    }

    [Fact]
    public void ToAnsiArt_TransparentUsesBackgroundColor()
    {
        var image = new Image(1, 1, [new Color(0, 0, 0, 0)]);
        var art = image.ToAnsiArt(
            new AnsiArtOptions
            {
                MaxWidth = 1,
                MaxHeight = 1,
                UseHalfBlocks = false,
                BackgroundColor = new Color(1, 2, 3),
            }
        );
        Assert.Equal(
            "\u001b[38;2;1;2;3m\u2588\u001b[0m" + Environment.NewLine,
            art
        );
    }

    [Fact]
    public void ToAnsiArt_FitsMaxWidthAndMaxHeight()
    {
        var pixels = new Color[16];
        Array.Fill(pixels, new Color(255, 0, 0));
        var image = new Image(4, 4, pixels);
        var art = image.ToAnsiArt(
            new AnsiArtOptions
            {
                MaxWidth = 2,
                MaxHeight = 2,
                UseHalfBlocks = false,
                // Explicit 1: auto full-block aspect is 2 (→ 2×1); force square cells.
                CharacterAspect = 1f,
            }
        );
        var lines = art.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(2, lines.Length);
        // Two full blocks per row; FG set once then reused, then reset.
        Assert.Equal("\u001b[38;2;255;0;0m\u2588\u2588\u001b[0m", lines[0]);
        Assert.Equal("\u001b[38;2;255;0;0m\u2588\u2588\u001b[0m", lines[1]);
    }

    [Fact]
    public void ToAnsiArt_FullBlock_AutoAspectIsTwo()
    {
        var pixels = new Color[16];
        Array.Fill(pixels, new Color(255, 0, 0));
        var image = new Image(4, 4, pixels);
        var art = image.ToAnsiArt(
            new AnsiArtOptions
            {
                MaxWidth = 2,
                MaxHeight = 2,
                UseHalfBlocks = false,
            }
        );
        var lines = art.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        // Auto CharacterAspect = 2 → 2×1 inside the max box.
        Assert.Single(lines);
        Assert.Equal("\u001b[38;2;255;0;0m\u2588\u2588\u001b[0m", lines[0]);
    }

    [Fact]
    public void ToAsciiArt_ExactSize_Letterboxes()
    {
        var image = new Image(2, 2, [new Color(0, 0, 0), new Color(0, 0, 0), new Color(0, 0, 0), new Color(0, 0, 0)]);
        // Aspect 2 → content 2×1 inside a 4×3 box → pad left/right 1, pad top/bottom 1.
        var art = image.ToAsciiArt(
            new AsciiArtOptions
            {
                MaxWidth = 4,
                MaxHeight = 3,
                SizeMode = TerminalArtSizeMode.Exact,
                Characters = "# ",
            }
        );
        var s = Environment.NewLine;
        Assert.Equal($"    {s} ## {s}    {s}", art);
    }

    [Fact]
    public void ToAsciiArt_ExplicitBackgroundColor()
    {
        var image = new Image(1, 1, [new Color(0, 0, 0, 0)]);
        // Transparent over light-gray chrome → light side of a 2-char ramp.
        var art = image.ToAsciiArt(
            new AsciiArtOptions
            {
                BackgroundColor = new Color(200, 200, 200),
                Characters = "#=",
            }
        );
        Assert.Equal("=" + Environment.NewLine, art);
    }

    [Fact]
    public void ToAnsiArt_ExactSize_Letterboxes()
    {
        var image = new Image(1, 1, [new Color(255, 0, 0)]);
        var art = image.ToAnsiArt(
            new AnsiArtOptions
            {
                MaxWidth = 3,
                MaxHeight = 1,
                SizeMode = TerminalArtSizeMode.Exact,
                UseHalfBlocks = false,
                CharacterAspect = 1f,
                BackgroundColor = new Color(0, 0, 0),
            }
        );
        // Content 1×1 centered → pad left 1, pad right 1; black pads, red center.
        Assert.Equal(
            "\u001b[38;2;0;0;0m\u2588\u001b[38;2;255;0;0m\u2588\u001b[38;2;0;0;0m\u2588\u001b[0m"
                + Environment.NewLine,
            art
        );
    }

    [Fact]
    public void ToAnsiArt_Ansi256()
    {
        var image = new Image(1, 1, [new Color(255, 0, 0)]);
        var art = image.ToAnsiArt(
            new AnsiArtOptions
            {
                MaxWidth = 1,
                MaxHeight = 1,
                UseHalfBlocks = false,
                ColorMode = AnsiColorMode.Ansi256,
            }
        );
        // 255,0,0 → cube (5,0,0) → 16 + 36*5 = 196
        Assert.Equal("\u001b[38;5;196m\u2588\u001b[0m" + Environment.NewLine, art);
    }

    [Fact]
    public void ToAnsiArt_Exact_RequiresBothDimensions()
    {
        var image = new Image(1, 1, [new Color(0, 0, 0)]);
        Assert.Throws<ArgumentException>(() =>
            image.ToAnsiArt(
                new AnsiArtOptions { SizeMode = TerminalArtSizeMode.Exact, MaxWidth = 4 }
            )
        );
    }

    [Fact]
    public void AppendAsciiArt_MatchesToAsciiArt()
    {
        var image = CreateAsciiArtFixture();
        var options = new AsciiArtOptions { MaxWidth = 1, Invert = true };
        var builder = new System.Text.StringBuilder();
        image.AppendAsciiArt(builder, options);
        Assert.Equal(image.ToAsciiArt(options), builder.ToString());
    }

    [Fact]
    public void WriteAnsiArt_MatchesToAnsiArt()
    {
        var image = new Image(1, 2, [new Color(255, 0, 0), new Color(0, 0, 255)]);
        var options = new AnsiArtOptions { MaxWidth = 1, MaxHeight = 1 };
        var buffer = new System.Buffers.ArrayBufferWriter<char>();
        image.WriteAnsiArt(buffer, options);
        Assert.Equal(image.ToAnsiArt(options), new string(buffer.WrittenSpan));
    }

    [Fact]
    public void ToByteArray_RoundTripsPngPixels()
    {
        var source = new Image(2, 1, [new Color(1, 2, 3), new Color(4, 5, 6)]);
        var bytes = source.ToByteArray(FileFormats.Png);
        Assert.True(bytes.Length > 8);
        Assert.Equal(0x89, bytes[0]); // PNG signature

        using var stream = new MemoryStream(bytes);
        var loaded = Image.Load(stream);
        Assert.Equal(2, loaded.Width);
        Assert.Equal(1, loaded.Height);
        Assert.Equal(new Color(1, 2, 3), loaded[0, 0]);
        Assert.Equal(new Color(4, 5, 6), loaded[1, 0]);
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
                255,
            }
        );
        Assert.Equal(
            "Qk1eAAAAAAAAADYAAAAoAAAAAQAAAAoAAAABABgAAAAAACgAAAAAAAAAAAAAAAAAAAAAAAAA5cyyAH9mTAAZ/+UAspl/AEwzGQDlzLIAf2ZMABn/5QCymX8ATDMZAA==",
            testImage.ToBase64String(FileFormats.Bmp)
        );
    }
}
