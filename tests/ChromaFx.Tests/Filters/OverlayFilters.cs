using ChromaFx.IO;
using ChromaFx.Processing.Filters.Overlays;
using ChromaFx.Processing.Filters.Resampling;
using ChromaFx.Processing.Filters.Resampling.Enums;
using ChromaFx.Tests.BaseClasses;
using Xunit;

namespace ChromaFx.Tests.Filters;

public class OverlayFilters : FilterTestBaseClass
{
    public override string ExpectedDirectory => "./ExpectedResults/Filters/";

    public override string OutputDirectory => "./TestOutput/Filters/";

    public static readonly TheoryData<string, IFilter, Rectangle> Filters = new()
    {
        { "Blend-50", new Blend(new Resize(500,500,ResamplingFiltersAvailable.Bilinear).Apply("./TestImages/Formats/Bmp/EncodingTest.bmp".LoadImage()),0.5f),new Rectangle(100,100,500,500) },
        { "Glow",new Glow(Color.Aqua,0.4f,0.4f),default },
        { "Vignette",new Vignette(Color.Aqua,0.4f,0.4f),default }
    };

    [Theory]
    [MemberData(nameof(Filters))]
    public void Run(string name, IFilter filter, Rectangle target)
    {
        CheckCorrect(name, filter, target);
    }
}