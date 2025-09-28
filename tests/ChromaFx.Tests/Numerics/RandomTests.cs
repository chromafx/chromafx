using Xunit;

namespace ChromaFx.Tests.Numerics;

public class RandomTests
{
    [Fact]
    public void ThreadSafeNext()
    {
        Assert.InRange(Processing.Numerics.Random.ThreadSafeNext(-10, 10), -10, 10);
    }

    [Fact]
    public void ThreadSafeNextDecimal()
    {
        Assert.InRange(Processing.Numerics.Random.ThreadSafeNextDouble(-10, 10), -10, 10);
    }
}