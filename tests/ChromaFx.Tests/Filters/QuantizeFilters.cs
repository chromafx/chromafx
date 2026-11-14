using ChromaFx.Tests.BaseClasses;
using Xunit;

namespace ChromaFx.Tests.Filters;

public class QuantizeFilters : FilterTestBaseClass
{
    public override string ExpectedDirectory => "./ExpectedResults/Filters/";

    public override string OutputDirectory => "./TestOutput/Filters/";

    public static readonly TheoryData<string, IFilter, Rectangle> Filters = new()
    {
        { "Quantize-16", new Quantize(16), default },
        {
            "Quantize-16-Bayer4",
            new Quantize(16, DitherMethod.Bayer4x4, 0.5f),
            default
        },
        {
            "Quantize-16-Floyd",
            new Quantize(16, DitherMethod.FloydSteinberg),
            default
        },
        { "Quantize-16-Partial", new Quantize(16), new Rectangle(100, 100, 500, 500) },
    };

    [Theory]
    [MemberData(nameof(Filters))]
    public void Run(string name, IFilter filter, Rectangle target)
    {
        CheckCorrect(name, filter, target);
    }
}
