using ChromaFx.Processing.Filters.Smoothing;
using ChromaFx.Tests.BaseClasses;
using Xunit;

namespace ChromaFx.Tests.Filters;

public class SmoothingFilters : FilterTestBaseClass
{
    public override string ExpectedDirectory => "./ExpectedResults/Filters/";

    public override string OutputDirectory => "./TestOutput/Filters/";

    public static readonly TheoryData<string, IFilter, Rectangle> Filters = new()
    {
        { "SNNBlur-5", new SnnBlur(5),default },
        { "Kuwahara-7", new Kuwahara(7),default },
        { "Median-5", new Median(5),default },

        { "SNNBlur-5-Partial", new SnnBlur(5),new Rectangle(100,100,500,500) },
        { "Kuwahara-7-Partial", new Kuwahara(7),new Rectangle(100,100,500,500) },
        { "Median-5-Partial", new Median(5),new Rectangle(100,100,500,500) }
    };

    [Theory]
    [MemberData(nameof(Filters))]
    public void Run(string name, IFilter filter, Rectangle target)
    {
        CheckCorrect(name, filter, target);
    }
}