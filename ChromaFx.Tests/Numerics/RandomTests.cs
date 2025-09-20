using ChromaFx.Numerics;
using Xunit;

namespace ChromaFx.Tests.Numerics;

public class RandomTests
{
    [Fact]
    public void ThreadSafeNext()
    {
        Assert.InRange(Random.ThreadSafeNext(-10, 10), -10, 10);
    }

    [Fact]
    public void ThreadSafeNextDecimal()
    {
        Assert.InRange(Random.ThreadSafeNextDouble(-10, 10), -10, 10);
    }
}