using Xunit;

namespace ChromaFx.Tests.Formats.Png.Format.Filters;

public class PaethFilter
{
    [Fact]
    public void Creation()
    {
        var testObject = new IO.Formats.Png.Format.Filters.PaethFilter();
        Assert.Equal(4, testObject.FilterValue);
    }

    [Fact]
    public void Decode()
    {
        var testObject = new IO.Formats.Png.Format.Filters.PaethFilter();
        var result = testObject.Decode([1, 2, 3, 4, 5, 6, 7, 8, 9], [9, 8, 7, 6, 5, 4, 3, 2, 1], 1);
        Assert.Equal(new byte[] { 10, 11, 14, 18, 23, 29, 36, 44, 53 }, result);
    }

    [Fact]
    public void Encode()
    {
        var testObject = new IO.Formats.Png.Format.Filters.PaethFilter();
        var result = testObject.Encode([1, 2, 3, 4, 5, 6, 7, 8, 9], [9, 8, 7, 6, 5, 4, 3, 2, 1], 1);
        Assert.Equal(new byte[] { 4, 248, 254, 11, 6, 250, 1, 13, 7, 252 }, result);
    }
}