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
    };

    [Theory]
    [MemberData(nameof(Filters))]
    public void Run(string name, IFilter filter, Rectangle target)
    {
        CheckCorrect(name, filter, target);
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
