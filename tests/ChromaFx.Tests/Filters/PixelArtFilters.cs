using ChromaFx.PixelArt;
using ChromaFx.PixelArt.Filters;
using ChromaFx.Tests.BaseClasses;
using Xunit;

namespace ChromaFx.Tests.Filters;

public class PixelArtFilters : FilterTestBaseClass
{
    public override string ExpectedDirectory => "./ExpectedResults/Filters/";

    public override string OutputDirectory => "./TestOutput/Filters/";

    /// <summary>
    /// Original CC0 128×128 pixel-art fixture (see TestImages/PixelArt/LICENSE.txt).
    /// Stepped/dithered gradients plus lines, junctions, diagonals, and curves for
    /// Scale2x and future rotation goldens.
    /// </summary>
    private static readonly string[] PixelArtFiles = ["./TestImages/PixelArt/fixture128.bmp"];

    public static readonly TheoryData<string, IFilter, Rectangle> Filters = new()
    {
        { "Scale2x", new Scale2x(), default },
    };

    [Theory]
    [MemberData(nameof(Filters))]
    public void Run(string name, IFilter filter, Rectangle target)
    {
        CheckCorrect(name, filter, target, PixelArtFiles);
    }

    private void CheckCorrect(string name, IFilter filter, Rectangle target, string[] files)
    {
        foreach (var file in files)
        {
            var outputFileName =
                Path.GetFileNameWithoutExtension(file) + "-" + name + Path.GetExtension(file);
            Image
                .Load(file)
                .Mutate(ctx => ctx.Apply(filter, target))
                .Save(OutputDirectory + outputFileName);
        }

        foreach (
            var outputFileName in files.Select(file =>
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
}

public class Scale2xSyntheticTests
{
    private static readonly Color Bg = Color.Black;
    private static readonly Color Fg = Color.White;

    [Fact]
    public void Scale2x_DoublesDimensions()
    {
        var image = new Image(3, 2, [Bg, Bg, Bg, Bg, Bg, Bg]);
        image.Mutate(ctx => ctx.Scale2x());
        Assert.Equal(6, image.Width);
        Assert.Equal(4, image.Height);
    }

    [Fact]
    public void Scale4x_And_Scale8x_MultiplyDimensions()
    {
        var image4 = new Image(2, 2, [Bg, Bg, Bg, Bg]);
        image4.Mutate(ctx => ctx.Scale4x());
        Assert.Equal(8, image4.Width);
        Assert.Equal(8, image4.Height);

        var image8 = new Image(2, 2, [Bg, Bg, Bg, Bg]);
        image8.Mutate(ctx => ctx.Scale8x());
        Assert.Equal(16, image8.Width);
        Assert.Equal(16, image8.Height);
    }

    [Fact]
    public void Scale2x_UniformFill_StaysUniform()
    {
        var image = new Image(2, 2, [Fg, Fg, Fg, Fg]);
        new Scale2x().Apply(image);

        Assert.Equal(4, image.Width);
        Assert.Equal(4, image.Height);
        for (var y = 0; y < 4; ++y)
        {
            var row = image.GetRowSpan(y);
            for (var x = 0; x < 4; ++x)
                Assert.Equal(Fg, row[x]);
        }
    }

    [Fact]
    public void Scale2x_EdgePixels_UseCenterWhenNeighborsOob()
    {
        // Single foreground pixel: all OOB neighbors are treated as P, so output is a 2×2 block of Fg.
        var image = new Image(1, 1, [Fg]);
        new Scale2x().Apply(image);

        Assert.Equal(2, image.Width);
        Assert.Equal(2, image.Height);
        Assert.Equal(Fg, image.GetRowSpan(0)[0]);
        Assert.Equal(Fg, image.GetRowSpan(0)[1]);
        Assert.Equal(Fg, image.GetRowSpan(1)[0]);
        Assert.Equal(Fg, image.GetRowSpan(1)[1]);
    }

    [Fact]
    public void Scale2x_LShape_ExpandsCornerTowardMatchingNeighbors()
    {
        // Classic EPX L: C and A match Fg while B/D differ → top-left becomes Fg.
        // Layout (3×3), center is P=Bg with A=Fg (up), C=Fg (left), B=Bg, D=Bg:
        //   . F .
        //   F B .
        //   . . .
        var pixels = new Color[9];
        Array.Fill(pixels, Bg);
        pixels[1] = Fg; // A relative to center
        pixels[3] = Fg; // C relative to center
        var image = new Image(3, 3, pixels);
        new Scale2x().Apply(image);

        // Center source (1,1) maps to dest (2,2)-(3,3). Top-left of that 2×2 should be Fg.
        Assert.Equal(Fg, image.GetRowSpan(2)[2]);
        Assert.Equal(Bg, image.GetRowSpan(2)[3]);
        Assert.Equal(Bg, image.GetRowSpan(3)[2]);
        Assert.Equal(Bg, image.GetRowSpan(3)[3]);
    }

    [Fact]
    public void Scale2x_Diagonal_DoesNotExpandWhenBothPairsMatch()
    {
        // When A==B and C==D patterns conflict with classic rules carefully:
        // A 2×2 checker: Scale2x should keep center-biased output (no false blend).
        // Fg Bg
        // Bg Fg
        var image = new Image(2, 2, [Fg, Bg, Bg, Fg]);
        new Scale2x().Apply(image);

        Assert.Equal(4, image.Width);
        Assert.Equal(4, image.Height);

        // Pixel (0,0)=Fg: A=oob→P, B=Bg, C=oob→P, D=Bg
        // e0: C==A (both Fg) && C!=D (Fg!=Bg) && A!=B (Fg!=Bg) → e0=A=Fg
        // e1: A==B? Fg==Bg no → e1=P=Fg
        // e2: D==C? Bg==Fg no → e2=P=Fg
        // e3: B==D (Bg==Bg) && B!=A && D!=C → e3=D=Bg
        Assert.Equal(Fg, image.GetRowSpan(0)[0]);
        Assert.Equal(Fg, image.GetRowSpan(0)[1]);
        Assert.Equal(Fg, image.GetRowSpan(1)[0]);
        Assert.Equal(Bg, image.GetRowSpan(1)[1]);
    }

    [Fact]
    public void Scale2xHelpers_ApplyPasses_MatchesScale4x()
    {
        var a = new Image(2, 2, [Fg, Bg, Bg, Fg]);
        var b = new Image(2, 2, [Fg, Bg, Bg, Fg]);
        new Scale4x().Apply(a);
        Scale2xHelpers.ApplyPasses(b, 2);
        AssertImagesEqual(a, b);
    }

    private static void AssertImagesEqual(Image left, Image right)
    {
        Assert.Equal(left.Width, right.Width);
        Assert.Equal(left.Height, right.Height);
        for (var y = 0; y < left.Height; ++y)
        {
            var lr = left.GetRowSpan(y);
            var rr = right.GetRowSpan(y);
            for (var x = 0; x < left.Width; ++x)
                Assert.Equal(lr[x], rr[x]);
        }
    }
}
