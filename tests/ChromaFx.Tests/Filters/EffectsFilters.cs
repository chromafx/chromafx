using ChromaFx.Processing.Filters.Convolution;
using ChromaFx.Processing.Filters.Effects;
using ChromaFx.Tests.BaseClasses;
using Xunit;

namespace ChromaFx.Tests.Filters;

public class EffectsFilters : FilterTestBaseClass
{
    public override string ExpectedDirectory => "./ExpectedResults/Filters/";

    public override string OutputDirectory => "./TestOutput/Filters/";

    public static readonly TheoryData<string, IFilter, Rectangle> Filters = new()
    {
        { "Pointillism", new Pointillism(5), default },
        { "Posterize", new Posterize(10), default },
        { "Solarize", new Solarize(1f), default },
        { "Replace-Black-For-White", new Replace(Color.Black, Color.White, 0.2f), default },
        { "Invert", new Invert(), default },
        { "Turbulence", new Turbulence(), default },
        { "Pixellate-10", new Pixellate(10), default },
        { "SinWave", new SinWave(10f, 10f, Direction.LeftToRight), default },
        {
            "Duotone",
            new Duotone(new Color(20, 40, 90), new Color(245, 210, 150)),
            default
        },
        { "Colorize", new Colorize(new Color(40, 120, 200), 0.85f), default },
        { "MotionBlur", new MotionBlur(11, 25f), default },
        { "ShadowsHighlights", new ShadowsHighlights(0.35f, -0.3f), default },
        { "OilPaint", new OilPaint(2, 12), default },
        { "Posterize-Partial", new Posterize(10), new Rectangle(100, 100, 500, 500) },
        { "Solarize-Partial", new Solarize(1f), new Rectangle(100, 100, 500, 500) },
        {
            "Replace-Black-For-White-Partial",
            new Replace(Color.Black, Color.White, 0.2f),
            new Rectangle(100, 100, 500, 500)
        },
        { "Invert-Partial", new Invert(), new Rectangle(100, 100, 500, 500) },
        { "Turbulence-Partial", new Turbulence(), new Rectangle(100, 100, 500, 500) },
        { "Pixellate-10-Partial", new Pixellate(10), new Rectangle(100, 100, 500, 500) },
        {
            "SinWave-Partial",
            new SinWave(10f, 10f, Direction.LeftToRight),
            new Rectangle(100, 100, 500, 500)
        },
    };

    [Theory]
    [MemberData(nameof(Filters))]
    public void Run(string name, IFilter filter, Rectangle target)
    {
        CheckCorrect(name, filter, target);
    }
}
