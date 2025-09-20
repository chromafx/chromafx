using ChromaFx.Filters.Interfaces;
using ChromaFx.Filters.Sharpening;
using ChromaFx.Numerics;
using ChromaFx.Tests.BaseClasses;
using Xunit;

namespace ChromaFx.Tests.Filters;

public class SharpeningFilters : FilterTestBaseClass
{
    public override string ExpectedDirectory => "./ExpectedResults/Filters/";

    public override string OutputDirectory => "./TestOutput/Filters/";

    public static readonly TheoryData<string, IFilter, Rectangle> Filters = new()
    {
        { "Unsharp", new Unsharp(3,0.2f),default },
        { "Unsharp-Partial", new Unsharp(3,0.2f),new Rectangle(100,100,500,500) }
    };

    [Theory]
    [MemberData(nameof(Filters))]
    public void Run(string name, IFilter filter, Rectangle target)
    {
        CheckCorrect(name, filter, target);
    }
}