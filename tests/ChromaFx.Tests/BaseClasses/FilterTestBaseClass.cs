using ChromaFx.IO;
using ChromaFx.Processing;
using ChromaFx.Processing.Filters.Interfaces;
using ChromaFx.Processing.Numerics;
using Xunit;

namespace ChromaFx.Tests.BaseClasses;

//[Collection("FilterCollection")]
public abstract class FilterTestBaseClass : TestBaseClass
{
    public static readonly List<string> Files =
        [
            "./TestImages/BitmapFilter.bmp"
            //"./TestImages/Formats/Bmp/Car.bmp",
            //"./TestImages/Formats/Png/splash.png",
            //"./TestImages/Formats/Png/indexed.png",
            //"./TestImages/Formats/Png/blur.png",
        ];

    protected void CheckCorrect(
        string name,
        IFilter filter,
        Rectangle target
    )
    {
        foreach (var file in Files)
        {
            var outputFileName =
                Path.GetFileNameWithoutExtension(file) + "-" + name + Path.GetExtension(file);
            file.LoadImage().ApplyFilter(filter, target).Save(OutputDirectory + outputFileName);
        }

        foreach (
            var outputFileName in Files.Select(
                file =>
                    Path.GetFileNameWithoutExtension(file) + "-" + name + Path.GetExtension(file)
            )
        )
        {
            Assert.True(
                CheckFileCorrect(
                    ExpectedDirectory + Path.GetFileName(outputFileName),
                    OutputDirectory + Path.GetFileName(outputFileName)
                ),
                outputFileName
            );
        }
    }
}
