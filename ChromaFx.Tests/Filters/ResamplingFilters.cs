using ChromaFx.Filters.Interfaces;
using ChromaFx.Filters.Resampling;
using ChromaFx.Filters.Resampling.Enums;
using ChromaFx.Numerics;
using ChromaFx.Tests.BaseClasses;
using Xunit;

namespace ChromaFx.Tests.Filters;

public class ResamplingFilters : FilterTestBaseClass
{
    public override string ExpectedDirectory => "./ExpectedResults/Filters/";

    public override string OutputDirectory => "./TestOutput/Filters/";

    public static readonly TheoryData<string, IFilter, Rectangle> Filters = new()
    {
        { "Scale", new Scale(2000,2000,ResamplingFiltersAvailable.Hermite),default },
        { "Translate", new Translate(50,50),default },
        { "Crop", new Crop(),new Rectangle(100,100,500,500) },
        { "Flip-Vertical", new Flip(FlipDirection.Vertical),default },
        { "Flip-Horizontal", new Flip(FlipDirection.Horizontal),default },
        { "Flip-Both", new Flip(FlipDirection.Horizontal|FlipDirection.Vertical),default },
        { "Rotate-45", new Rotate(45f),default },
        { "Skew-45-80", new Skew(45f,80f,ResamplingFiltersAvailable.Hermite),default },
        { "ResizeCanvas-UpperLeft-100x100", new ResizeCanvas(100,100),default },
        { "ResizeCanvas-Center-100x100", new ResizeCanvas(100,100,ResizeOptions.Center),default },
        { "ResizeCanvas-UpperLeft-2000x2000", new ResizeCanvas(2000,2000),default },
        { "ResizeCanvas-Center-2000x2000", new ResizeCanvas(2000,2000,ResizeOptions.Center),default },

        { "Scale-Partial", new Scale(2000,2000),new Rectangle(100,100,500,500) },
        { "Translate-Partial", new Translate(50,50),new Rectangle(150,150,500,500) },
        { "Flip-Vertical-Partial", new Flip(FlipDirection.Vertical),new Rectangle(100,100,500,500) },
        { "Flip-Horizontal-Partial", new Flip(FlipDirection.Horizontal),new Rectangle(100,100,500,500) },
        { "Flip-Both-Partial", new Flip(FlipDirection.Horizontal|FlipDirection.Vertical),new Rectangle(100,100,500,500) },
        { "Rotate-45-Partial", new Rotate(45f),new Rectangle(100,100,500,500) },
        { "Skew-45-80-Partial", new Skew(45f,80f),new Rectangle(100,100,500,500) },

        {"Resize-Bilinear-100x100",new Resize(100,100,ResamplingFiltersAvailable.Bilinear),default },
        {"Resize-Bilinear-2000x2000",new Resize(2000,2000,ResamplingFiltersAvailable.Bilinear),default },
        {"Resize-NearestNeighbor-2000x2000",new Resize(2000,2000,ResamplingFiltersAvailable.NearestNeighbor),default },
        {"Resize-NearestNeighbor-100x100",new Resize(100,100,ResamplingFiltersAvailable.NearestNeighbor) ,default},
        {"Resize-Box-100x100",new Resize(100,100,ResamplingFiltersAvailable.Box),default },
        {"Resize-Box-2000x2000",new Resize(2000,2000,ResamplingFiltersAvailable.Box),default },
        {"Resize-Robidoux-2000x2000",new Resize(2000,2000,ResamplingFiltersAvailable.Robidoux),default },
        {"Resize-Robidoux-100x100",new Resize(100,100,ResamplingFiltersAvailable.Robidoux),default },
        {"Resize-RobidouxSharp-2000x2000",new Resize(2000,2000,ResamplingFiltersAvailable.RobidouxSharp) ,default},
        {"Resize-RobidouxSharp-100x100",new Resize(100,100,ResamplingFiltersAvailable.RobidouxSharp) ,default},
        {"Resize-RobidouxSoft-2000x2000",new Resize(2000,2000,ResamplingFiltersAvailable.RobidouxSoft) ,default},
        {"Resize-RobidouxSoft-100x100",new Resize(100,100,ResamplingFiltersAvailable.RobidouxSoft),default },
        {"Resize-Bicubic-2000x2000",new Resize(2000,2000,ResamplingFiltersAvailable.Bicubic) ,default},
        {"Resize-Bicubic-100x100",new Resize(100,100,ResamplingFiltersAvailable.Bicubic),default },
        {"Resize-Bell-2000x2000",new Resize(2000,2000,ResamplingFiltersAvailable.Bell),default },
        {"Resize-Bell-100x100",new Resize(100,100,ResamplingFiltersAvailable.Bell),default },
        {"Resize-CatmullRom-2000x2000",new Resize(2000,2000,ResamplingFiltersAvailable.CatmullRom),default },
        {"Resize-CatmullRom-100x100",new Resize(100,100,ResamplingFiltersAvailable.CatmullRom),default },
        {"Resize-Cosine-2000x2000",new Resize(2000,2000,ResamplingFiltersAvailable.Cosine),default },
        {"Resize-Cosine-100x100",new Resize(100,100,ResamplingFiltersAvailable.Cosine),default },
        {"Resize-CubicBSpline-2000x2000",new Resize(2000,2000,ResamplingFiltersAvailable.CubicBSpline) ,default},
        {"Resize-CubicBSpline-100x100",new Resize(100,100,ResamplingFiltersAvailable.CubicBSpline),default },
        {"Resize-CubicConvolution-2000x2000",new Resize(2000,2000,ResamplingFiltersAvailable.CubicConvolution),default },
        {"Resize-CubicConvolution-100x100",new Resize(100,100,ResamplingFiltersAvailable.CubicConvolution),default },
        {"Resize-Hermite-2000x2000",new Resize(2000,2000,ResamplingFiltersAvailable.Hermite),default },
        {"Resize-Hermite-100x100",new Resize(100,100,ResamplingFiltersAvailable.Hermite),default },
        {"Resize-Lanczos3-2000x2000",new Resize(2000,2000,ResamplingFiltersAvailable.Lanczos3),default },
        {"Resize-Lanczos3-100x100",new Resize(100,100,ResamplingFiltersAvailable.Lanczos3) ,default},
        {"Resize-Lanczos8-2000x2000",new Resize(2000,2000,ResamplingFiltersAvailable.Lanczos8),default },
        {"Resize-Lanczos8-100x100",new Resize(100,100,ResamplingFiltersAvailable.Lanczos8),default },
        {"Resize-Mitchell-2000x2000",new Resize(2000,2000,ResamplingFiltersAvailable.Mitchell),default },
        {"Resize-Mitchell-100x100",new Resize(100,100,ResamplingFiltersAvailable.Mitchell),default },
        {"Resize-Quadratic-2000x2000",new Resize(2000,2000,ResamplingFiltersAvailable.Quadratic),default },
        {"Resize-Quadratic-100x100",new Resize(100,100,ResamplingFiltersAvailable.Quadratic),default },
        {"Resize-QuadraticBSpline-2000x2000",new Resize(2000,2000,ResamplingFiltersAvailable.QuadraticBSpline),default },
        {"Resize-QuadraticBSpline-100x100",new Resize(100,100,ResamplingFiltersAvailable.QuadraticBSpline),default },
        {"Resize-Triangle-2000x2000",new Resize(2000,2000,ResamplingFiltersAvailable.Triangle),default },
        {"Resize-Triangle-100x100",new Resize(100,100,ResamplingFiltersAvailable.Triangle),default }
    };

    [Theory]
    [MemberData(nameof(Filters))]
    public void Run(string name, IFilter filter, Rectangle target)
    {
        CheckCorrect(name, filter, target);
    }
}