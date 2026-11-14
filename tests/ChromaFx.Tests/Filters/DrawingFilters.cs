using System.Numerics;
using ChromaFx.Processing.Filters.Drawing;
using ChromaFx.Tests.BaseClasses;
using Xunit;

namespace ChromaFx.Tests.Filters;

public class DrawingFilters : FilterTestBaseClass
{
    public override string ExpectedDirectory => "./ExpectedResults/Filters/";

    public override string OutputDirectory => "./TestOutput/Filters/";

    public static readonly TheoryData<string, IFilter, Processing.Numerics.Rectangle> Filters =
        new()
        {
            { "DrawingLine", new Line(Color.Fuchsia, 0, 0, 500, 1000), default },
            {
                "DrawingRectangle",
                new Processing.Filters.Drawing.Rectangle(
                    Color.Fuchsia,
                    false,
                    new Processing.Numerics.Rectangle(0, 0, 500, 1000)
                ),
                default
            },
            {
                "DrawingEllipse",
                new Ellipse(Color.Fuchsia, false, 100, 100, new Vector2(500, 500)),
                default
            },
            {
                "DrawingFilledEllipse",
                new Ellipse(Color.Fuchsia, true, 100, 100, new Vector2(500, 500)),
                default
            },
            {
                "Fill-Purple",
                new Processing.Filters.Drawing.Rectangle(
                    new Color(127, 0, 127, 255),
                    true,
                    new Processing.Numerics.Rectangle(100, 100, 500, 500)
                ),
                default
            },
            {
                "DrawingPolygon",
                new Polygon(
                    Color.Fuchsia,
                    false,
                    new Vector2(100, 100),
                    new Vector2(500, 150),
                    new Vector2(400, 600),
                    new Vector2(150, 500)
                ),
                default
            },
            {
                "DrawingFilledPolygon",
                new Polygon(
                    new Color(127, 0, 127, 255),
                    true,
                    new Vector2(120, 120),
                    new Vector2(480, 180),
                    new Vector2(420, 580),
                    new Vector2(160, 520)
                ),
                default
            },
            {
                "DrawingPolyline",
                new Polyline(
                    Color.Fuchsia,
                    new Vector2(50, 50),
                    new Vector2(400, 200),
                    new Vector2(200, 700),
                    new Vector2(700, 900)
                ),
                default
            },
            {
                "DrawingThickPolyline",
                new Polyline(
                    Color.Fuchsia,
                    new Vector2(80, 80),
                    new Vector2(600, 250),
                    new Vector2(300, 800)
                )
                {
                    StrokeWidth = 7,
                },
                default
            },
            { "FloodFill", new FloodFill(Color.Fuchsia, 400, 400), default },
        };

    [Theory]
    [MemberData(nameof(Filters))]
    public void Run(string name, IFilter filter, Processing.Numerics.Rectangle target)
    {
        CheckCorrect(name, filter, target);
    }
}
