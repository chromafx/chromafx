using ChromaFx.Processing.Filters.Overlays;
using ChromaFx.Tests.BaseClasses;
using Xunit;

namespace ChromaFx.Tests.Filters;

public class OverlayFilters : FilterTestBaseClass
{
    public override string ExpectedDirectory => "./ExpectedResults/Filters/";

    public override string OutputDirectory => "./TestOutput/Filters/";

    private static Image Overlay500() =>
        Image.Load("./TestImages/Formats/Bmp/EncodingTest.bmp").Mutate(c => c.Resize(500, 500));

    public static readonly TheoryData<string, IFilter, Rectangle> Filters = new()
    {
        {
            "Blend-50",
            new Blend(Overlay500(), 0.5f),
            new Rectangle(100, 100, 500, 500)
        },
        {
            "Blend-Multiply",
            new Blend(Overlay500(), 0.75f, BlendMode.Multiply),
            new Rectangle(100, 100, 500, 500)
        },
        {
            "Blend-Screen",
            new Blend(Overlay500(), 0.75f, BlendMode.Screen),
            new Rectangle(100, 100, 500, 500)
        },
        {
            "Blend-Overlay",
            new Blend(Overlay500(), 0.75f, BlendMode.Overlay),
            new Rectangle(100, 100, 500, 500)
        },
        {
            "Blend-SoftLight",
            new Blend(Overlay500(), 0.75f, BlendMode.SoftLight),
            new Rectangle(100, 100, 500, 500)
        },
        { "Glow", new Glow(Color.Aqua, 0.4f, 0.4f), default },
        { "Vignette", new Vignette(Color.Aqua, 0.4f, 0.4f), default },
    };

    [Theory]
    [MemberData(nameof(Filters))]
    public void Run(string name, IFilter filter, Rectangle target)
    {
        CheckCorrect(name, filter, target);
    }
}
