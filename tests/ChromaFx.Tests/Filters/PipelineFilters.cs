using ChromaFx.Processing;
using ChromaFx.Processing.Filters.ColorMatrix;
using ChromaFx.Processing.Filters.Convolution;
using ChromaFx.Processing.Filters.Convolution.Enums;
using ChromaFx.Processing.Filters.Pipelines;
using ChromaFx.Tests.BaseClasses;
using Xunit;

namespace ChromaFx.Tests.Filters;

public class PipelineFilters : FilterTestBaseClass
{
    public override string ExpectedDirectory => "./ExpectedResults/Filters/";

    public override string OutputDirectory => "./TestOutput/Filters/";

    public static readonly TheoryData<string, IFilter, Rectangle> Filters = new()
    {
        { "Pipeline-Polaroid-Brightness", new ProcessingPipeline(true).AddFilter(new PolaroidColorMatrix()).AddFilter(new Brightness(.1f)),default },
        { "Pipeline-RC-Greyscale", new ProcessingPipeline(true).AddFilter(new RobertsCross()).AddFilter(new Greyscale601()),default },
        { "Pipeline-Polaroid-Brightness-Partial", new ProcessingPipeline(true).AddFilter(new PolaroidColorMatrix()).AddFilter(new Brightness(.1f)),new Rectangle(100,100,500,500) },
        { "Pipeline-RC-Greyscale-Partial", new ProcessingPipeline(true).AddFilter(new RobertsCross()).AddFilter(new Greyscale601()),new Rectangle(100,100,500,500) },
        { "CannyEdgeDetection", new CannyEdgeDetection(Color.White,Color.Black,0.8f,0.5f),default },
        { "NormalMap", new NormalMap(XDirection.LeftToRight,YDirection.BottomToTop),default },
        { "BumpMap", new BumpMap(Direction.LeftToRight),default },
        { "GaussianBlur-7", new GaussianBlur(7) ,default},
        { "Poloroid", new Polaroid(),default},
        { "Lomograph", new Lomograph() ,default},

        { "CannyEdgeDetection-Partial", new CannyEdgeDetection(Color.White,Color.Black,0.8f,0.5f),new Rectangle(100,100,500,500) },
        { "NormalMap-Partial", new NormalMap(XDirection.LeftToRight,YDirection.BottomToTop),new Rectangle(100,100,500,500) },
        { "BumpMap-Partial", new BumpMap(Direction.LeftToRight),new Rectangle(100,100,500,500) },
        { "GaussianBlur-7-Partial", new GaussianBlur(7) ,new Rectangle(100,100,500,500)},
        { "Poloroid-Partial", new Polaroid(),new Rectangle(100,100,500,500)},
        { "Lomograph-Partial", new Lomograph() ,new Rectangle(100,100,500,500)}
    };

    [Theory]
    [MemberData(nameof(Filters))]
    public void Run(string name, IFilter filter, Rectangle target)
    {
        CheckCorrect(name, filter, target);
    }
}