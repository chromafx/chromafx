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
    /// Scale2x / Eagle / SaI and RotateSprite goldens.
    /// </summary>
    private static readonly string[] PixelArtFiles = ["./TestImages/PixelArt/fixture128.bmp"];

    public static readonly TheoryData<string, IFilter, Rectangle> Filters = new()
    {
        { "Scale2x", new Scale2x(), default },
        { "Scale3x", new Scale3x(), default },
        { "Hq2x", new Hq2x(), default },
        { "Hq3x", new Hq3x(), default },
        { "Hq4x", new Hq4x(), default },
        { "CleanEdge4x", new CleanEdge(4), default },
        { "Eagle", new Eagle(), default },
        { "SuperEagle", new SuperEagle(), default },
        { "Scale2xSaI", new Scale2xSaI(), default },
        { "Super2xSaI", new Super2xSaI(), default },
        { "RotateSprite45", new RotateSprite(45f), default },
        { "CleanEdgeRotate45", new CleanEdgeRotate(45f), default },
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
    public void Scale3x_TriplesDimensions()
    {
        var image = new Image(3, 2, [Bg, Bg, Bg, Bg, Bg, Bg]);
        image.Mutate(ctx => ctx.Scale3x());
        Assert.Equal(9, image.Width);
        Assert.Equal(6, image.Height);
    }

    [Fact]
    public void Hq2x_DoublesDimensions()
    {
        var image = new Image(3, 2, [Bg, Bg, Bg, Bg, Bg, Bg]);
        image.Mutate(ctx => ctx.Hq2x());
        Assert.Equal(6, image.Width);
        Assert.Equal(4, image.Height);
    }

    [Fact]
    public void Hq3x_And_Hq4x_MultiplyDimensions()
    {
        var image3 = new Image(2, 2, [Bg, Bg, Bg, Bg]);
        image3.Mutate(ctx => ctx.Hq3x());
        Assert.Equal(6, image3.Width);
        Assert.Equal(6, image3.Height);

        var image4 = new Image(2, 2, [Bg, Bg, Bg, Bg]);
        image4.Mutate(ctx => ctx.Hq4x());
        Assert.Equal(8, image4.Width);
        Assert.Equal(8, image4.Height);
    }

    [Fact]
    public void CleanEdge_ScalesByFactor()
    {
        var image = new Image(2, 2, [Fg, Bg, Bg, Fg]);
        image.Mutate(ctx => ctx.CleanEdge(3));
        Assert.Equal(6, image.Width);
        Assert.Equal(6, image.Height);
    }

    [Fact]
    public void CleanEdge_UniformFill_StaysUniform()
    {
        var image = new Image(2, 2, [Fg, Fg, Fg, Fg]);
        new CleanEdge(4).Apply(image);
        Assert.Equal(8, image.Width);
        Assert.Equal(8, image.Height);
        for (var y = 0; y < 8; ++y)
        {
            var row = image.GetRowSpan(y);
            for (var x = 0; x < 8; ++x)
                Assert.Equal(Fg, row[x]);
        }
    }

    [Fact]
    public void Hq2x_UniformFill_StaysUniform()
    {
        var image = new Image(2, 2, [Fg, Fg, Fg, Fg]);
        new Hq2x().Apply(image);
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
    public void Scale3x_UniformFill_StaysUniform()
    {
        var image = new Image(2, 2, [Fg, Fg, Fg, Fg]);
        new Scale3x().Apply(image);

        Assert.Equal(6, image.Width);
        Assert.Equal(6, image.Height);
        for (var y = 0; y < 6; ++y)
        {
            var row = image.GetRowSpan(y);
            for (var x = 0; x < 6; ++x)
                Assert.Equal(Fg, row[x]);
        }
    }

    [Fact]
    public void Scale3x_LShape_ExpandsCornerTowardMatchingNeighbors()
    {
        // Center E=Bg with B=Fg (up) and D=Fg (left) → E0 becomes Fg; center E4 stays Bg.
        var pixels = new Color[9];
        Array.Fill(pixels, Bg);
        pixels[1] = Fg; // B relative to center
        pixels[3] = Fg; // D relative to center
        var image = new Image(3, 3, pixels);
        new Scale3x().Apply(image);

        // Center source (1,1) maps to dest (3,3)-(5,5).
        Assert.Equal(Fg, image.GetRowSpan(3)[3]); // E0
        Assert.Equal(Bg, image.GetRowSpan(4)[4]); // E4
        Assert.Equal(Bg, image.GetRowSpan(5)[5]); // E8
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

public class EagleSaISyntheticTests
{
    private static readonly Color Bg = Color.Black;
    private static readonly Color Fg = Color.White;

    [Theory]
    [InlineData(typeof(Eagle))]
    [InlineData(typeof(SuperEagle))]
    [InlineData(typeof(Scale2xSaI))]
    [InlineData(typeof(Super2xSaI))]
    public void DoublesDimensions(Type filterType)
    {
        var filter = (IFilter)Activator.CreateInstance(filterType)!;
        var image = new Image(3, 2, [Bg, Bg, Bg, Bg, Bg, Bg]);
        filter.Apply(image);
        Assert.Equal(6, image.Width);
        Assert.Equal(4, image.Height);
    }

    [Fact]
    public void Eagle_IsolatedPixel_Disappears()
    {
        // Classic Eagle quirk: lone Fg on Bg vanishes.
        var pixels = new Color[9];
        Array.Fill(pixels, Bg);
        pixels[4] = Fg;
        var image = new Image(3, 3, pixels);
        new Eagle().Apply(image);

        for (var y = 2; y <= 3; ++y)
        {
            var row = image.GetRowSpan(y);
            for (var x = 2; x <= 3; ++x)
                Assert.Equal(Bg, row[x]);
        }
    }

    [Fact]
    public void Eagle_UniformFill_StaysUniform()
    {
        var image = new Image(2, 2, [Fg, Fg, Fg, Fg]);
        new Eagle().Apply(image);
        for (var y = 0; y < 4; ++y)
        {
            var row = image.GetRowSpan(y);
            for (var x = 0; x < 4; ++x)
                Assert.Equal(Fg, row[x]);
        }
    }

    [Fact]
    public void Scale2xSaI_UniformFill_StaysUniform()
    {
        var image = new Image(2, 2, [Fg, Fg, Fg, Fg]);
        new Scale2xSaI().Apply(image);
        for (var y = 0; y < 4; ++y)
        {
            var row = image.GetRowSpan(y);
            for (var x = 0; x < 4; ++x)
                Assert.Equal(Fg, row[x]);
        }
    }

    [Theory]
    [InlineData(typeof(Scale2xSaI))]
    [InlineData(typeof(Super2xSaI))]
    [InlineData(typeof(SuperEagle))]
    public void BlendingScalers_OpaqueCheckerboard_KeepsOpaqueAlpha(Type filterType)
    {
        // Fg Bg
        // Bg Fg — forces Interpolate/QInterpolate paths; alpha must stay 255.
        var image = new Image(2, 2, [Fg, Bg, Bg, Fg]);
        ((IFilter)Activator.CreateInstance(filterType)!).Apply(image);
        AssertAllOpaque(image);
    }

    private static void AssertAllOpaque(Image image)
    {
        for (var y = 0; y < image.Height; ++y)
        {
            var row = image.GetRowSpan(y);
            for (var x = 0; x < image.Width; ++x)
                Assert.Equal(255, row[x].Alpha);
        }
    }
}

public class RotateSpriteSyntheticTests
{
    private static readonly Color Bg = Color.Black;
    private static readonly Color Fg = Color.White;

    [Fact]
    public void RotateSprite_ZeroDegrees_IsNoOp()
    {
        var pixels = new Color[] { Fg, Bg, Bg, Fg };
        var image = new Image(2, 2, pixels);
        image.Mutate(ctx => ctx.RotateSprite(0f));
        Assert.Equal(2, image.Width);
        Assert.Equal(2, image.Height);
        Assert.Equal(Fg, image.GetRowSpan(0)[0]);
        Assert.Equal(Bg, image.GetRowSpan(0)[1]);
        Assert.Equal(Bg, image.GetRowSpan(1)[0]);
        Assert.Equal(Fg, image.GetRowSpan(1)[1]);
    }

    [Fact]
    public void RotateSprite_90_SwapsDimensions()
    {
        // 3×2 → 2×3 (exact cardinal remap)
        var image = new Image(
            3,
            2,
            new int[] { 1, 2, 3, 4, 5, 6 }.Select(v => new Color((byte)v, 0, 0)).ToArray()
        );
        new RotateSprite(90f).Apply(image);
        Assert.Equal(2, image.Width);
        Assert.Equal(3, image.Height);
        Assert.Equal(new Color(4, 0, 0), image.GetRowSpan(0)[0]);
        Assert.Equal(new Color(1, 0, 0), image.GetRowSpan(0)[1]);
        Assert.Equal(new Color(5, 0, 0), image.GetRowSpan(1)[0]);
        Assert.Equal(new Color(2, 0, 0), image.GetRowSpan(1)[1]);
        Assert.Equal(new Color(6, 0, 0), image.GetRowSpan(2)[0]);
        Assert.Equal(new Color(3, 0, 0), image.GetRowSpan(2)[1]);
    }

    [Fact]
    public void RotateSprite_45_GrowsCanvas()
    {
        var image = new Image(8, 8, Enumerable.Repeat(Fg, 64).ToArray());
        new RotateSprite(45f).Apply(image);
        Assert.True(image.Width > 8);
        Assert.True(image.Height > 8);
    }

    [Fact]
    public void PixelArt2xHelpers_EaglePass_DiffersFromScale2x_OnIsolatedPixel()
    {
        // Lone Fg: Eagle erases the center block; Scale2x keeps it.
        var pixels = new Color[9];
        Array.Fill(pixels, Bg);
        pixels[4] = Fg;
        var scale2x = new Image(3, 3, pixels);
        var eagle = new Image(3, 3, (Color[])pixels.Clone());
        PixelArt2xHelpers.ApplyPasses(scale2x, PixelArt2xAlgorithm.Scale2x, 1);
        PixelArt2xHelpers.ApplyPasses(eagle, PixelArt2xAlgorithm.Eagle, 1);

        Assert.Equal(Fg, scale2x.GetRowSpan(2)[2]);
        Assert.Equal(Bg, eagle.GetRowSpan(2)[2]);
    }

    [Fact]
    public void RotateSprite_DefaultEmpty_IsOpaqueBlack()
    {
        Assert.Equal(new Color(0, 0, 0, 255), new RotateSprite(45f).EmptyColor);
    }

    [Fact]
    public void RotateSprite_Checkerboard45_KeepsOpaqueAlpha()
    {
        var pixels = new Color[16];
        for (var i = 0; i < 16; ++i)
            pixels[i] = ((i % 2) ^ (i / 4 % 2)) == 0 ? Fg : Bg;
        var image = new Image(4, 4, pixels);
        new RotateSprite(45f).Apply(image);
        Assert.True(image.Width >= 4);
        Assert.True(image.Height >= 4);
        for (var y = 0; y < image.Height; ++y)
        {
            var row = image.GetRowSpan(y);
            for (var x = 0; x < image.Width; ++x)
                Assert.Equal(255, row[x].Alpha);
        }
    }

    [Fact]
    public void CleanEdgeRotate_ZeroDegrees_IsNoOp()
    {
        var pixels = new Color[] { Fg, Bg, Bg, Fg };
        var image = new Image(2, 2, pixels);
        image.Mutate(ctx => ctx.CleanEdgeRotate(0f));
        Assert.Equal(2, image.Width);
        Assert.Equal(2, image.Height);
        Assert.Equal(Fg, image.GetRowSpan(0)[0]);
        Assert.Equal(Bg, image.GetRowSpan(0)[1]);
        Assert.Equal(Bg, image.GetRowSpan(1)[0]);
        Assert.Equal(Fg, image.GetRowSpan(1)[1]);
    }

    [Fact]
    public void CleanEdgeRotate_90_MatchesRotateSpriteCardinal()
    {
        var pixels = new int[] { 1, 2, 3, 4, 5, 6 }
            .Select(v => new Color((byte)v, 0, 0))
            .ToArray();
        var a = new Image(3, 2, pixels);
        var b = new Image(3, 2, (Color[])pixels.Clone());
        new CleanEdgeRotate(90f).Apply(a);
        new RotateSprite(90f).Apply(b);
        Assert.Equal(b.Width, a.Width);
        Assert.Equal(b.Height, a.Height);
        for (var y = 0; y < a.Height; ++y)
        {
            var ra = a.GetRowSpan(y);
            var rb = b.GetRowSpan(y);
            for (var x = 0; x < a.Width; ++x)
                Assert.Equal(rb[x], ra[x]);
        }
    }

    [Fact]
    public void CleanEdgeRotate_45_MatchesRotateSpriteAabbSize()
    {
        var fill = Enumerable.Repeat(Fg, 64).ToArray();
        var a = new Image(8, 8, fill);
        var b = new Image(8, 8, (Color[])fill.Clone());
        new CleanEdgeRotate(45f).Apply(a);
        new RotateSprite(45f).Apply(b);
        Assert.Equal(b.Width, a.Width);
        Assert.Equal(b.Height, a.Height);
        Assert.True(a.Width > 8);
        Assert.True(a.Height > 8);
    }

    [Fact]
    public void CleanEdgeRotate_DefaultEmpty_IsOpaqueBlack()
    {
        Assert.Equal(new Color(0, 0, 0, 255), new CleanEdgeRotate(45f).EmptyColor);
    }

    [Fact]
    public void CleanEdgeRotate_Checkerboard45_KeepsOpaqueAlpha()
    {
        var pixels = new Color[16];
        for (var i = 0; i < 16; ++i)
            pixels[i] = ((i % 2) ^ (i / 4 % 2)) == 0 ? Fg : Bg;
        var image = new Image(4, 4, pixels);
        new CleanEdgeRotate(45f).Apply(image);
        Assert.True(image.Width >= 4);
        Assert.True(image.Height >= 4);
        for (var y = 0; y < image.Height; ++y)
        {
            var row = image.GetRowSpan(y);
            for (var x = 0; x < image.Width; ++x)
                Assert.Equal(255, row[x].Alpha);
        }
    }
}
