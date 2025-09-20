using Xunit;

namespace ChromaFx.Tests.Numerics;

public class RandomTests
{
    [Fact]
    public void ThreadSafeNext()
    {
        Assert.InRange(ChromaFx.Numerics.Random.ThreadSafeNext(-10, 10), -10, 10);
    }

    [Fact]
    public void ThreadSafeNextDecimal()
    {
        Assert.InRange(ChromaFx.Numerics.Random.ThreadSafeNextDouble(-10, 10), -10, 10);
    }
}