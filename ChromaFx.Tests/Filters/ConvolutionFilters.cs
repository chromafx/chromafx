using ChromaFx.Filters.Convolution;
using ChromaFx.Filters.Interfaces;
using ChromaFx.Numerics;
using ChromaFx.Tests.BaseClasses;
using Xunit;

namespace ChromaFx.Tests.Filters;

public class ConvolutionFilters : FilterTestBaseClass
{
    public override string ExpectedDirectory => "./ExpectedResults/Filters/";

    public override string OutputDirectory => "./TestOutput/Filters/";

    public static readonly TheoryData<string, IFilter, Rectangle> Filters = new()
    {
        { "SobelEmboss", new SobelEmboss(),default },
        { "BoxBlur-3", new BoxBlur(3),default },
        { "BoxBlur-5", new BoxBlur(5),default },
        { "Robinson", new Robinson(),default },
        { "Emboss", new Emboss(),default },
        { "LaplaceEdgeDetection", new LaplaceEdgeDetection(),default },
        { "Sharpen", new Sharpen(),default },
        { "SharpenLess", new SharpenLess(),default },
        { "LaplacianOfGaussianEdgeDetector", new LaplacianOfGaussianEdgeDetector(),default },
        { "Kayyali", new Kayyali(),default },
        { "Kirsch", new Kirsch(),default },
        { "Prewitt", new Prewitt() ,default},
        { "RobertsCross", new RobertsCross(),default },
        { "Scharr", new Scharr(),default },

        { "SobelEmboss-Partial", new SobelEmboss(),new Rectangle(100,100,500,500) },
        { "BoxBlur-3-Partial", new BoxBlur(3),new Rectangle(100,100,500,500) },
        { "BoxBlur-5-Partial", new BoxBlur(5),new Rectangle(100,100,500,500) },
        { "Emboss-Partial", new Emboss(),new Rectangle(100,100,500,500) },
        { "LaplaceEdgeDetection-Partial", new LaplaceEdgeDetection(),new Rectangle(100,100,500,500) },
        { "Sharpen-Partial", new Sharpen(),new Rectangle(100,100,500,500) },
        { "SharpenLess-Partial", new SharpenLess(),new Rectangle(100,100,500,500) },
        { "LaplacianOfGaussianEdgeDetector-Partial", new LaplacianOfGaussianEdgeDetector(),new Rectangle(100,100,500,500) },
        { "Kayyali-Partial", new Kayyali(),new Rectangle(100,100,500,500) },
        { "Kirsch-Partial", new Kirsch(),new Rectangle(100,100,500,500) },
        { "Prewitt-Partial", new Prewitt() ,new Rectangle(100,100,500,500)},
        { "RobertsCross-Partial", new RobertsCross(),new Rectangle(100,100,500,500) },
        { "Robinson-Partial", new Robinson(),new Rectangle(100,100,500,500) },
        { "Scharr-Partial", new Scharr(),new Rectangle(100,100,500,500) }
    };

    [Theory]
    [MemberData(nameof(Filters))]
    public void Run(string name, IFilter filter, Rectangle target)
    {
        CheckCorrect(name, filter, target);
    }
}