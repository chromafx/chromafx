using ChromaFx.Colors.ColorSpaces;
using ChromaFx.Colors.ColorSpaces.Interfaces;
using System;
using Xunit;

namespace ChromaFx.Tests.Colors.ColorSpaces;

public class Conversion
{
    public static readonly TheoryData<Func<ChromaFx.Colors.Color, IColorSpace>, IColorSpace> ConversionTestData = new()
    {
        {x=>(Bgra)x,new Bgra(25,140,132,51) },
        {x=>(CieLab)x,new CieLab(55.818348999038136d,-16.853418868401192d,54.700336854544005d) },
        {x=>(YCbCr)x,new YCbCr(124.49799999999999d,71.849888d,133.35088d) },
        {x=>(Hsv)x,new Hsv(64.17f,0.82f,0.55f) },
        {x=>(Xyz)x,new Xyz(19.069271087036391d,23.731887647203653d,4.4953578376945806d) },
        {x=>(Yxy)x,new Yxy(23.731887647203653d,0.40318552970034038d,0.50176819282471141d) },
        {x=>(Cieluv)x,new Cieluv(55.818348999038136d,-1.10255145019876d,59.059378241483785d) },
        {x=>(Cielch)x,new Cielch(55.8183479309082d,57.237789252554272d,107.12429940735514d) },
        {x=>(HunterLab)x,new HunterLab(48.715385297874477d,-15.379442134009619d,28.629607681009084d) }
    };

    public static readonly TheoryData<Func<IColorSpace, ChromaFx.Colors.Color>, IColorSpace> ConversionTestData2 = new()
    {
        {x=>(Bgra)x,new Bgra(25, 140, 132, 255) },
        {x=>(CieLab)x,new CieLab(55.818348999038136d,-16.853418868401192d,54.700336854544005d) },
        {x=>(YCbCr)x,new YCbCr(124.49799999999999d,71.849888d,133.35088d) },
        {x=>(Hsv)x,new Hsv(64.17f,0.82f,0.55f) },
        {x=>(Xyz)x,new Xyz(19.069271087036391d,23.731887647203653d,4.4953578376945806d) },
        {x=>(Yxy)x,new Yxy(23.731887647203653d,0.40318552970034038d,0.50176819282471141d) },
        {x=>(Cieluv)x,new Cieluv(55.818348999038136d,-1.10255145019876d,59.059378241483785d) },
        {x=>(Cielch)x,new Cielch(55.8183479309082d,57.237789252554272d,107.12429940735514d) },
        {x=>(HunterLab)x,new HunterLab(48.715385297874477d,-15.379442134009619d,28.629607681009084d) }
    };

    [Theory]
    [MemberData(nameof(ConversionTestData))]
    public void CheckCorrect(Func<ChromaFx.Colors.Color, IColorSpace> conversionFunc, IColorSpace expected)
    {
        var actual = conversionFunc(new ChromaFx.Colors.Color(132, 140, 25, 51));
        Assert.Equal(expected, actual);
    }

    [Theory]
    [MemberData(nameof(ConversionTestData2))]
    public void CheckCorrectToRgb(Func<IColorSpace, ChromaFx.Colors.Color> conversionFunction, IColorSpace value)
    {
        var actual = conversionFunction(value);
        Assert.Equal(new ChromaFx.Colors.Color(132, 140, 25, 255), actual);
    }
}