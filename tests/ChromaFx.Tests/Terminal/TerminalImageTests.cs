using System.Collections;
using Xunit;

namespace ChromaFx.Tests.Terminal;

public class TerminalImageTests
{
    [Fact]
    public void Kitty_FromPng_SingleChunk()
    {
        var png = EncodeTinyPng();
        var sequence = KittyGraphics.FromPng(png);
        Assert.StartsWith("\u001b_Ga=T,f=100,m=0;", sequence);
        Assert.EndsWith("\u001b\\", sequence);
        Assert.Equal(1, CountOccurrences(sequence, "\u001b_G"));
    }

    [Fact]
    public void Kitty_FromPng_ChunksLargePayload()
    {
        // Payload sized so base64 exceeds one 4096-byte APC chunk (need not be valid PNG).
        var raw = new byte[4000];
        raw.AsSpan().Fill(0x5A);
        var sequence = KittyGraphics.FromPng(raw);
        Assert.True(CountOccurrences(sequence, "\u001b_G") > 1);
        Assert.Contains("a=T,f=100", sequence);
        Assert.Contains("m=1;", sequence);
        Assert.Contains("m=0;", sequence);
        Assert.EndsWith("\u001b\\", sequence);
        Assert.True(
            sequence.LastIndexOf("m=1;", StringComparison.Ordinal)
                < sequence.LastIndexOf("m=0;", StringComparison.Ordinal)
        );
    }

    [Fact]
    public void Kitty_IncludesCellSize()
    {
        var sequence = KittyGraphics.FromPng(EncodeTinyPng(), columns: 24, rows: 12);
        Assert.Contains(",c=24,r=12,", sequence);
    }

    [Fact]
    public void ITerm2_FromPng_Osc1337()
    {
        var png = EncodeTinyPng();
        var sequence = ITerm2InlineImage.FromPng(png, columns: 20, rows: 10);
        Assert.StartsWith("\u001b]1337;File=inline=1;size=", sequence);
        Assert.Contains(";width=20;height=10:", sequence);
        Assert.EndsWith("\u0007", sequence);
    }

    [Fact]
    public void Detect_KittyFromEnvironment()
    {
        var caps = TerminalImageCapabilities.Detect(
            new Hashtable
            {
                ["KITTY_WINDOW_ID"] = "1",
                ["TERM"] = "xterm-kitty",
            }
        );
        Assert.True(caps.SupportsKitty);
        Assert.Equal(TerminalImageProtocol.Kitty, caps.Preferred);
    }

    [Fact]
    public void Detect_ITerm2FromEnvironment()
    {
        var caps = TerminalImageCapabilities.Detect(
            new Hashtable { ["TERM_PROGRAM"] = "iTerm.app" }
        );
        Assert.True(caps.SupportsITerm2);
        Assert.Equal(TerminalImageProtocol.ITerm2, caps.Preferred);
    }

    [Fact]
    public void Detect_TrueColorFallsBackToAnsi()
    {
        var caps = TerminalImageCapabilities.Detect(
            new Hashtable { ["COLORTERM"] = "truecolor", ["TERM"] = "xterm-256color" }
        );
        Assert.False(caps.SupportsKitty);
        Assert.False(caps.SupportsITerm2);
        Assert.True(caps.SupportsTrueColor);
        Assert.Equal(TerminalImageProtocol.AnsiArt, caps.Preferred);
    }

    [Fact]
    public void EncodePng_ThenKitty_RoundTripPattern()
    {
        var image = new Image(1, 1, [new Color(10, 20, 30)]);
        var sequence = KittyGraphics.FromPng(image.ToByteArray(FileFormats.Png), columns: 4, rows: 2);
        Assert.Contains(",c=4,r=2,", sequence);
        Assert.StartsWith("\u001b_Ga=T,f=100,", sequence);
    }

    [Fact]
    public void Detect_WezTermPrefersKitty()
    {
        var caps = TerminalImageCapabilities.Detect(
            new Hashtable { ["TERM_PROGRAM"] = "WezTerm" }
        );
        Assert.True(caps.SupportsKitty);
        Assert.True(caps.SupportsITerm2);
        Assert.Equal(TerminalImageProtocol.Kitty, caps.Preferred);
    }

    [Fact]
    public void Kitty_RejectsNegativeCellSize()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            KittyGraphics.FromPng(EncodeTinyPng(), columns: -1)
        );
    }

    private static byte[] EncodeTinyPng() =>
        new Image(1, 1, [new Color(10, 20, 30)]).ToByteArray(FileFormats.Png);

    private static int CountOccurrences(string haystack, string needle)
    {
        var count = 0;
        for (var i = 0; (i = haystack.IndexOf(needle, i, StringComparison.Ordinal)) >= 0; i++)
            count++;
        return count;
    }
}
