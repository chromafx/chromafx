using ChromaFx.Processing.Filters.Drawing;
using ChromaFx.Tests.BaseClasses;
using Xunit;

namespace ChromaFx.Tests.Filters;

public class DrawingFilters : FilterTestBaseClass
{
    public override string ExpectedDirectory => "./ExpectedResults/Filters/";

    public override string OutputDirectory => "./TestOutput/Filters/";

    public static readonly TheoryData<string, IFilter, Processing.Numerics.Rectangle> Filters = new()
    {
        { "DrawingLine", new Line(Color.Fuchsia,0,0,500,1000),default },
        { "DrawingRectangle", new Processing.Filters.Drawing.Rectangle(Color.Fuchsia,false,new Processing.Numerics.Rectangle(0,0,500,1000)),default },
        { "DrawingEllipse", new Ellipse(Color.Fuchsia,false,100,100,new System.Numerics.Vector2(500,500)),default },
        { "DrawingFilledEllipse", new Ellipse(Color.Fuchsia,true,100,100,new System.Numerics.Vector2(500,500)),default },
        { "Fill-Purple", new Processing.Filters.Drawing.Rectangle(new Color(127,0,127,255),true,new Processing.Numerics.Rectangle(100,100,500,500)),default }
    };

    [Theory]
    [MemberData(nameof(Filters))]
    public void Run(string name, IFilter filter, Processing.Numerics.Rectangle target)
    {
        CheckCorrect(name, filter, target);
    }
}