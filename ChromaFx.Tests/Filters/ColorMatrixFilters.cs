using ChromaFx.Filters.ColorMatrix;
using ChromaFx.Filters.ColorMatrix.ColorBlindness;
using ChromaFx.Filters.Interfaces;
using ChromaFx.Numerics;
using ChromaFx.Tests.BaseClasses;
using Xunit;

namespace ChromaFx.Tests.Filters;

public class ColorMatrixFilters : FilterTestBaseClass
{
    public override string ExpectedDirectory => "./ExpectedResults/Filters/";

    public override string OutputDirectory => "./TestOutput/Filters/";

    public static readonly TheoryData<string, IFilter, Rectangle> Filters = new()
    {
        { "Alpha-50", new Alpha(.5f),default },
        { "BlackWhite", new BlackWhite(),default },
        { "BlueFilter", new BlueFilter(),default },
        { "Tritanopia", new Tritanopia(),default },
        { "Tritanomaly", new Tritanomaly(),default },
        { "Protanopia", new Protanopia(),default },
        { "Protanomaly", new Protanomaly(),default },
        { "Deuteranopia", new Deuteranopia(),default },
        { "Achromatomaly", new Achromatomaly(),default },
        { "Deuteranomaly", new Deuteranomaly(),default },
        { "Achromatopsia", new Achromatopsia(),default },
        { "Brightness-50", new Brightness(.5f),default },
        { "Brightness--50", new Brightness(-.5f),default },
        { "GreenFilter", new GreenFilter(),default },
        { "Greyscale601", new Greyscale601(),default },
        { "Greyscale709", new Greyscale709(),default },
        { "LomographColorMatrix", new LomographColorMatrix(),default },
        { "PolaroidColorMatrix", new PolaroidColorMatrix(),default },
        { "RedFilter", new RedFilter(),default },
        { "Contrast-50", new Contrast(1.5f),default },
        { "Contrast--50", new Contrast(0.5f),default },
        { "Hue-90", new Hue(90),default },
        { "Hue-180", new Hue(180),default },
        { "Hue-270", new Hue(270),default },
        { "Kodachrome", new Kodachrome(),default },
        { "Saturation-50", new Saturation(.5f),default },
        { "Saturation--50", new Saturation(-.5f),default },
        { "Sepiatone", new Sepiatone(),default },
        { "MatrixMultiplication", new PolaroidColorMatrix() * new Brightness(.1f) ,default},
        { "Temperature--CandleLight", new Temperature(Temperature.Temperatures.CandleLight), default },
        { "Temperature--Twilight", new Temperature(Temperature.Temperatures.Twilight), default },
        { "Temperature--Moonlight", new Temperature(Temperature.Temperatures.Moonlight), default },
        { "Temperature--SunAtNoon", new Temperature(Temperature.Temperatures.SunAtNoon), default },
        { "Temperature--CloudySky", new Temperature(Temperature.Temperatures.CloudySky), default },
        { "Temperature--OutdoorShade", new Temperature(Temperature.Temperatures.OutdoorShade), default },
        { "Temperature--ClearBlueSky", new Temperature(Temperature.Temperatures.ClearBlueSky), default },
        { "Temperature--15000", new Temperature(15000), default },
        { "Temperature--25000", new Temperature(25000), default },
        { "Temperature--40000", new Temperature(40000), default },
        { "ColorMatrix", new ColorMatrix(new Matrix5X5(1,0,1,0,0,0,.5f,0,0,0,2.5f,-1,-1,0,0,0,0,0,0,0,0,0,0,0,0)),default },

        { "Alpha-50-Partial", new Alpha(.5f),new Rectangle(100,100,500,500) },
        { "BlackWhite-Partial", new BlackWhite(),new Rectangle(100,100,500,500) },
        { "BlueFilter-Partial", new BlueFilter(),new Rectangle(100,100,500,500) },
        { "Tritanopia-Partial", new Tritanopia(),new Rectangle(100,100,500,500) },
        { "Tritanomaly-Partial", new Tritanomaly(),new Rectangle(100,100,500,500) },
        { "Protanopia-Partial", new Protanopia(),new Rectangle(100,100,500,500) },
        { "Protanomaly-Partial", new Protanomaly(),new Rectangle(100,100,500,500) },
        { "Deuteranopia-Partial", new Deuteranopia(),new Rectangle(100,100,500,500) },
        { "Achromatomaly-Partial", new Achromatomaly(),new Rectangle(100,100,500,500) },
        { "Deuteranomaly-Partial", new Deuteranomaly(),new Rectangle(100,100,500,500) },
        { "Achromatopsia-Partial", new Achromatopsia(),new Rectangle(100,100,500,500) },
        { "Brightness-50-Partial", new Brightness(.5f),new Rectangle(100,100,500,500) },
        { "Brightness--50-Partial", new Brightness(-.5f),new Rectangle(100,100,500,500) },
        { "GreenFilter-Partial", new GreenFilter(),new Rectangle(100,100,500,500) },
        { "Greyscale601-Partial", new Greyscale601(),new Rectangle(100,100,500,500) },
        { "Greyscale709-Partial", new Greyscale709(),new Rectangle(100,100,500,500) },
        { "Hue-90-Partial", new Hue(90),new Rectangle(100,100,500,500) },
        { "Hue-180-Partial", new Hue(180),new Rectangle(100,100,500,500) },
        { "Hue-270-Partial", new Hue(270),new Rectangle(100,100,500,500) },
        { "Kodachrome-Partial", new Kodachrome(),new Rectangle(100,100,500,500) },
        { "LomographColorMatrix-Partial", new LomographColorMatrix(),new Rectangle(100,100,500,500) },
        { "PolaroidColorMatrix-Partial", new PolaroidColorMatrix(),new Rectangle(100,100,500,500) },
        { "RedFilter-Partial", new RedFilter(),new Rectangle(100,100,500,500) },
        { "Saturation-50-Partial", new Saturation(.5f),new Rectangle(100,100,500,500) },
        { "Saturation--50-Partial", new Saturation(-.5f),new Rectangle(100,100,500,500) },
        { "Sepiatone-Partial", new Sepiatone(),new Rectangle(100,100,500,500) },
        { "MatrixMultiplication-Partial", new PolaroidColorMatrix() * new Brightness(.1f) ,new Rectangle(100,100,500,500)},
        { "Contrast-50-Partial", new Contrast(1.5f),new Rectangle(100,100,500,500) },
        { "Contrast--50-Partial", new Contrast(0.5f),new Rectangle(100,100,500,500) },
        { "ColorMatrix-Partial", new ColorMatrix(new Matrix5X5(1,0,1,0,0,0,.5f,0,0,0,2.5f,-1,-1,0,0,0,0,0,0,0,0,0,0,0,0)),new Rectangle(100,100,500,500) },
    };

    [Theory]
    [MemberData(nameof(Filters))]
    public void Run(string name, IFilter filter, Rectangle target)
    {
        CheckCorrect(name, filter, target);
    }
}