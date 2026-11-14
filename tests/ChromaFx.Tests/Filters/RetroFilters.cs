using ChromaFx.Core.Quantization;
using ChromaFx.Retro;
using ChromaFx.Retro.Filters;
using ChromaFx.Tests.BaseClasses;
using Xunit;

namespace ChromaFx.Tests.Filters;

public class RetroFilters : FilterTestBaseClass
{
    public override string ExpectedDirectory => "./ExpectedResults/Filters/";

    public override string OutputDirectory => "./TestOutput/Filters/";

    public static readonly TheoryData<string, IFilter, Rectangle> Filters = new()
    {
        { "WebSafeQuantize", new WebSafeQuantize(), default },
        {
            "WebSafeQuantize-Partial",
            new WebSafeQuantize(),
            new Rectangle(100, 100, 500, 500)
        },
        { "CgaQuantize", new CgaQuantize(), default },
        { "CgaQuantize-Standard16", new CgaQuantize(CgaPaletteMode.Standard16), default },
        { "EgaQuantize", new EgaQuantize(), default },
        {
            "EgaQuantize-Partial",
            new EgaQuantize(),
            new Rectangle(100, 100, 500, 500)
        },
        { "VgaQuantize", new VgaQuantize(), default },
        {
            "VgaQuantize-Partial",
            new VgaQuantize(),
            new Rectangle(100, 100, 500, 500)
        },
        {
            "WebSafeQuantize-Bayer4x4",
            new WebSafeQuantize(OrderedDitherMethod.Bayer4x4, 0.4f),
            default
        },
        {
            "CgaQuantize-Bayer8x8",
            new CgaQuantize(CgaPaletteMode.Palette1High, OrderedDitherMethod.Bayer8x8, 0.5f),
            default
        },
        { "BadWebScale", new BadWebScale(0.25f), default },
        { "BadWebScale-WebSafe", new BadWebScale(0.25f, webSafe: true), default },
        { "Bevel", new Bevel(2, BevelStyle.Raised), default },
        { "Bevel-Pressed", new Bevel(3, BevelStyle.Pressed), default },
        { "RetroMode-320x200", new RetroMode(320, 200), default },
    };

    [Theory]
    [MemberData(nameof(Filters))]
    public void Run(string name, IFilter filter, Rectangle target)
    {
        CheckCorrect(name, filter, target);
    }
}

public class RetroOrderedDitherSyntheticTests
{
    [Fact]
    public void NoneOrZeroStrength_YieldsZeroBias()
    {
        Assert.Equal(
            0,
            OrderedDither.ChannelBias(
                3,
                5,
                OrderedDitherMethod.None,
                0.5f,
                OrderedDither.RetroBiasScale,
                round: true
            )
        );
        Assert.Equal(
            0,
            OrderedDither.ChannelBias(
                3,
                5,
                OrderedDitherMethod.Bayer4x4,
                0f,
                OrderedDither.RetroBiasScale,
                round: true
            )
        );
    }

    [Fact]
    public void Bayer4x4_OriginIsStronglyNegative()
    {
        // Cell 0 → threshold below 0.5 → negative bias at strength 0.5.
        Assert.True(
            OrderedDither.ChannelBias(
                0,
                0,
                OrderedDitherMethod.Bayer4x4,
                0.5f,
                OrderedDither.RetroBiasScale,
                round: true
            ) < 0
        );
    }
}

public class WebSafeQuantizeSyntheticTests
{
    private static readonly byte[] Levels = [0x00, 0x33, 0x66, 0x99, 0xCC, 0xFF];

    [Theory]
    [InlineData(0, 0x00)]
    [InlineData(25, 0x00)]
    [InlineData(26, 0x33)]
    [InlineData(51, 0x33)]
    [InlineData(76, 0x33)]
    [InlineData(77, 0x66)]
    [InlineData(127, 0x66)]
    [InlineData(128, 0x99)]
    [InlineData(153, 0x99)]
    [InlineData(204, 0xCC)]
    [InlineData(229, 0xCC)]
    [InlineData(230, 0xFF)]
    [InlineData(255, 0xFF)]
    public void SnapChannel_MapsToNearestWebSafeLevel(byte input, byte expected)
    {
        Assert.Equal(expected, WebSafeQuantize.SnapChannel(input));
    }

    [Fact]
    public void SnapChannel_OnlyProducesWebSafeLevels()
    {
        for (var i = 0; i < 256; ++i)
        {
            var snapped = WebSafeQuantize.SnapChannel((byte)i);
            Assert.Contains(snapped, Levels);
        }
    }

    [Fact]
    public void Apply_PreservesAlphaAndSnapsRgb()
    {
        var image = new Image(2, 1);
        image.GetRowSpan(0)[0] = new Color(26, 128, 231, 128);
        image.GetRowSpan(0)[1] = new Color(255, 0, 51, 255);

        image.Mutate(ctx => ctx.WebSafeQuantize());

        var a = image.GetRowSpan(0)[0];
        Assert.Equal(0x33, a.Red);
        Assert.Equal(0x99, a.Green);
        Assert.Equal(0xFF, a.Blue);
        Assert.Equal(128, a.Alpha);

        var b = image.GetRowSpan(0)[1];
        Assert.Equal(0xFF, b.Red);
        Assert.Equal(0x00, b.Green);
        Assert.Equal(0x33, b.Blue);
        Assert.Equal(255, b.Alpha);
    }

    [Fact]
    public void Bayer4x4_CanChangeSnapNearThreshold()
    {
        // Mid-grey near a web-safe boundary; Bayer bias at (0,0) is strongly negative.
        var image = new Image(1, 1);
        image.GetRowSpan(0)[0] = new Color(26, 26, 26, 200);

        image.Mutate(ctx => ctx.WebSafeQuantize(OrderedDitherMethod.Bayer4x4, 0.5f));

        var p = image.GetRowSpan(0)[0];
        Assert.Equal(0x00, p.Red);
        Assert.Equal(0x00, p.Green);
        Assert.Equal(0x00, p.Blue);
        Assert.Equal(200, p.Alpha);
    }

    [Fact]
    public void None_MatchesDefaultConstructorPath()
    {
        var a = new Image(2, 2);
        var b = new Image(2, 2);
        for (var y = 0; y < 2; y++)
        {
            var ra = a.GetRowSpan(y);
            var rb = b.GetRowSpan(y);
            for (var x = 0; x < 2; x++)
            {
                var c = new Color((byte)(x * 80), (byte)(y * 90), 40, 255);
                ra[x] = c;
                rb[x] = c;
            }
        }

        a.Mutate(ctx => ctx.Apply(new WebSafeQuantize()));
        b.Mutate(ctx => ctx.WebSafeQuantize(OrderedDitherMethod.None, 0.5f));

        for (var y = 0; y < 2; y++)
        {
            var ra = a.GetRowSpan(y);
            var rb = b.GetRowSpan(y);
            for (var x = 0; x < 2; x++)
                Assert.Equal(ra[x], rb[x]);
        }
    }
}

public class CgaQuantizeSyntheticTests
{
    [Fact]
    public void Palette1High_MapsNearCyanMagentaWhite()
    {
        var image = new Image(4, 1);
        var row = image.GetRowSpan(0);
        row[0] = new Color(0, 0, 0, 200);
        row[1] = new Color(40, 240, 240, 200);
        row[2] = new Color(240, 40, 240, 200);
        row[3] = new Color(250, 250, 250, 200);

        image.Mutate(ctx => ctx.CgaQuantize(CgaPaletteMode.Palette1High));

        row = image.GetRowSpan(0);
        Assert.Equal(new Color(0, 0, 0, 200), row[0]);
        Assert.Equal(new Color(85, 255, 255, 200), row[1]);
        Assert.Equal(new Color(255, 85, 255, 200), row[2]);
        Assert.Equal(new Color(255, 255, 255, 200), row[3]);
    }

    [Fact]
    public void Palette2Low_UsesBrownNotDarkYellow()
    {
        var image = new Image(1, 1);
        image.GetRowSpan(0)[0] = new Color(170, 85, 0, 255);

        image.Mutate(ctx => ctx.CgaQuantize(CgaPaletteMode.Palette2Low));

        Assert.Equal(new Color(170, 85, 0, 255), image.GetRowSpan(0)[0]);
    }

    [Fact]
    public void Monochrome2_PreservesAlpha()
    {
        var image = new Image(2, 1);
        image.GetRowSpan(0)[0] = new Color(10, 10, 10, 64);
        image.GetRowSpan(0)[1] = new Color(200, 200, 200, 128);

        image.Mutate(ctx => ctx.CgaQuantize(CgaPaletteMode.Monochrome2));

        Assert.Equal(new Color(0, 0, 0, 64), image.GetRowSpan(0)[0]);
        Assert.Equal(new Color(255, 255, 255, 128), image.GetRowSpan(0)[1]);
    }
}

public class EgaQuantizeSyntheticTests
{
    private static readonly byte[] Levels = [0x00, 0x55, 0xAA, 0xFF];

    [Theory]
    [InlineData(0, 0x00)]
    [InlineData(42, 0x00)]
    [InlineData(43, 0x55)]
    [InlineData(85, 0x55)]
    [InlineData(127, 0x55)]
    [InlineData(128, 0xAA)]
    [InlineData(170, 0xAA)]
    [InlineData(212, 0xAA)]
    [InlineData(213, 0xFF)]
    [InlineData(255, 0xFF)]
    public void SnapChannel_MapsToNearestEgaLevel(byte input, byte expected)
    {
        Assert.Equal(expected, EgaQuantize.SnapChannel(input));
    }

    [Fact]
    public void SnapChannel_OnlyProducesEgaLevels()
    {
        for (var i = 0; i < 256; ++i)
            Assert.Contains(EgaQuantize.SnapChannel((byte)i), Levels);
    }

    [Fact]
    public void Apply_PreservesAlphaAndSnapsRgb()
    {
        var image = new Image(1, 1);
        image.GetRowSpan(0)[0] = new Color(43, 128, 213, 90);

        image.Mutate(ctx => ctx.EgaQuantize());

        var p = image.GetRowSpan(0)[0];
        Assert.Equal(0x55, p.Red);
        Assert.Equal(0xAA, p.Green);
        Assert.Equal(0xFF, p.Blue);
        Assert.Equal(90, p.Alpha);
    }
}

public class VgaQuantizeSyntheticTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 0)]
    [InlineData(2, 4)]
    [InlineData(127, 128)]
    [InlineData(252, 252)]
    [InlineData(253, 252)]
    [InlineData(254, 252)]
    [InlineData(255, 252)]
    public void SnapChannel_MapsToSixBitDacLevels(byte input, byte expected)
    {
        Assert.Equal(expected, VgaQuantize.SnapChannel(input));
    }

    [Fact]
    public void SnapChannel_NeverWrapsNearWhiteToBlack()
    {
        for (var i = 250; i < 256; ++i)
        {
            var snapped = VgaQuantize.SnapChannel((byte)i);
            Assert.True(snapped >= 248, $"channel {i} snapped to {snapped}");
        }
    }

    [Fact]
    public void Apply_PreservesAlphaAndSnapsRgb()
    {
        var image = new Image(1, 1);
        image.GetRowSpan(0)[0] = new Color(255, 3, 7, 77);

        image.Mutate(ctx => ctx.VgaQuantize());

        var p = image.GetRowSpan(0)[0];
        Assert.Equal(252, p.Red);
        Assert.Equal(4, p.Green);
        Assert.Equal(8, p.Blue);
        Assert.Equal(77, p.Alpha);
    }
}

public class BadWebScaleSyntheticTests
{
    [Fact]
    public void RestoresOriginalDimensions()
    {
        var image = new Image(40, 20);
        image.Mutate(ctx => ctx.BadWebScale(0.25f));
        Assert.Equal(40, image.Width);
        Assert.Equal(20, image.Height);
    }

    [Fact]
    public void InvalidFactor_IsNoOp()
    {
        var image = new Image(4, 4);
        image.GetRowSpan(0)[0] = new Color(12, 34, 56, 78);
        image.Mutate(ctx => ctx.BadWebScale(1f));
        Assert.Equal(new Color(12, 34, 56, 78), image.GetRowSpan(0)[0]);
    }

    [Fact]
    public void DownUp_MakesBlockyFromFineDetail()
    {
        var image = new Image(4, 4);
        for (var y = 0; y < 4; y++)
        {
            var row = image.GetRowSpan(y);
            for (var x = 0; x < 4; x++)
                row[x] = ((x + y) & 1) == 0 ? Color.White : Color.Black;
        }

        image.Mutate(ctx => ctx.BadWebScale(0.5f));

        // After 2×2 NN down + up, each 2×2 block is uniform.
        Assert.Equal(image.GetRowSpan(0)[0], image.GetRowSpan(0)[1]);
        Assert.Equal(image.GetRowSpan(0)[0], image.GetRowSpan(1)[0]);
        Assert.Equal(image.GetRowSpan(0)[0], image.GetRowSpan(1)[1]);
    }
}

public class BevelSyntheticTests
{
    [Fact]
    public void Raised_ExpandsAndLightensTopLeft()
    {
        var image = new Image(4, 4);
        image.GetRowSpan(0).Fill(new Color(100, 100, 100));
        for (var y = 1; y < 4; y++)
            image.GetRowSpan(y).Fill(new Color(100, 100, 100));

        image.Mutate(ctx => ctx.Bevel(2, BevelStyle.Raised, strength: 1f));

        Assert.Equal(8, image.Width);
        Assert.Equal(8, image.Height);
        // Top-left lightened toward white; bottom-right darkened.
        Assert.True(image.GetRowSpan(0)[0].Red > 100);
        Assert.True(image.GetRowSpan(7)[7].Red < 100);
    }

    [Fact]
    public void Pressed_DarkensTopLeft()
    {
        var image = new Image(6, 6);
        for (var y = 0; y < 6; y++)
            image.GetRowSpan(y).Fill(new Color(128, 128, 128));

        image.Mutate(ctx =>
            ctx.Bevel(1, BevelStyle.Pressed, expandCanvas: false, strength: 1f)
        );

        Assert.True(image.GetRowSpan(0)[0].Red < 128);
        Assert.True(image.GetRowSpan(5)[5].Red > 128);
    }

    [Fact]
    public void ZeroDepth_IsNoOp()
    {
        var image = new Image(3, 3);
        image.GetRowSpan(1)[1] = new Color(1, 2, 3, 4);
        image.Mutate(ctx => ctx.Bevel(0));
        Assert.Equal(3, image.Width);
        Assert.Equal(new Color(1, 2, 3, 4), image.GetRowSpan(1)[1]);
    }

    [Fact]
    public void PreservesAlpha()
    {
        var image = new Image(4, 4);
        for (var y = 0; y < 4; y++)
            image.GetRowSpan(y).Fill(new Color(80, 80, 80, 200));

        image.Mutate(ctx => ctx.Bevel(1, expandCanvas: false, strength: 0.5f));
        Assert.Equal(200, image.GetRowSpan(0)[0].Alpha);
    }
}

public class RetroModeSyntheticTests
{
    [Fact]
    public void ResizesWithNearestNeighbor()
    {
        var image = new Image(10, 10);
        image.Mutate(ctx => ctx.RetroMode(320, 200));
        Assert.Equal(320, image.Width);
        Assert.Equal(200, image.Height);
    }

    [Fact]
    public void SameSize_IsNoOp()
    {
        var image = new Image(32, 24);
        image.GetRowSpan(0)[0] = new Color(9, 8, 7, 6);
        image.Mutate(ctx => ctx.RetroMode(32, 24));
        Assert.Equal(new Color(9, 8, 7, 6), image.GetRowSpan(0)[0]);
    }
}
