using ChromaFx.IO;
using ChromaFx.Processing.Filters.Arithmetic;
using ChromaFx.Tests.BaseClasses;
using Xunit;

namespace ChromaFx.Tests.Filters;

public class ArithmeticFilters : FilterTestBaseClass
{
    public override string ExpectedDirectory => "./ExpectedResults/Filters/";

    public override string OutputDirectory => "./TestOutput/Filters/";

    public static readonly TheoryData<string, IFilter, Rectangle> Filters = new()
    {
        { "XOr", new XOr("./TestImages/Formats/Bmp/Car.bmp".LoadImage()),new Rectangle(100,100,500,500)},
        { "Or", new Or("./TestImages/Formats/Bmp/Car.bmp".LoadImage()),new Rectangle(100,100,500,500)},
        { "And", new And("./TestImages/Formats/Bmp/Car.bmp".LoadImage()),new Rectangle(100,100,500,500)},
        { "Subtract", new Subtract("./TestImages/Formats/Bmp/Car.bmp".LoadImage()),new Rectangle(100,100,500,500)},
        { "Add", new Add("./TestImages/Formats/Bmp/Car.bmp".LoadImage()),new Rectangle(100,100,500,500)},
        { "Division", new Division("./TestImages/Formats/Bmp/Car.bmp".LoadImage()),new Rectangle(100,100,500,500)},
        { "Multiplication", new Multiplication("./TestImages/Formats/Bmp/Car.bmp".LoadImage()),new Rectangle(100,100,500,500)},
        { "Modulo", new Modulo("./TestImages/Formats/Bmp/Car.bmp".LoadImage()),new Rectangle(100,100,500,500)}
    };

    [Theory]
    [MemberData(nameof(Filters))]
    public void Run(string name, IFilter filter, Rectangle target)
    {
        CheckCorrect(name, filter, target);
    }
}