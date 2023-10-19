﻿using Structure.Sketching.Colors;
using Structure.Sketching.Filters.Convolution.Enums;
using Structure.Sketching.Filters.Effects;
using Structure.Sketching.Filters.Interfaces;
using Structure.Sketching.Numerics;
using Structure.Sketching.Tests.BaseClasses;
using Xunit;

namespace Structure.Sketching.Tests.Filters
{
    public class EffectsFilters : FilterTestBaseClass
    {
        public override string ExpectedDirectory => "./ExpectedResults/Filters/";

        public override string OutputDirectory => "./TestOutput/Filters/";

        public static readonly TheoryData<string, IFilter, Rectangle> Filters = new()
        {
            { "Pointillism", new Pointillism(5),default },
            { "Posterize", new Posterize(10),default },
            { "Solarize", new Solarize(1f),default },
            { "Replace-Black-For-White", new Replace(Color.Black,Color.White,0.2f),default },
            { "Invert", new Invert(),default },
            { "Turbulence", new Turbulence(),default },
            { "Pixellate-10", new Pixellate(10) ,default},
            { "SinWave", new SinWave(10f,10f,Direction.LeftToRight),default },

            { "Posterize-Partial", new Posterize(10),new Rectangle(100,100,500,500) },
            { "Solarize-Partial", new Solarize(1f),new Rectangle(100,100,500,500) },
            { "Replace-Black-For-White-Partial", new Replace(Color.Black,Color.White,0.2f),new Rectangle(100,100,500,500) },
            { "Invert-Partial", new Invert(),new Rectangle(100,100,500,500) },
            { "Turbulence-Partial", new Turbulence(),new Rectangle(100,100,500,500) },
            { "Pixellate-10-Partial", new Pixellate(10) ,new Rectangle(100,100,500,500)},
            { "SinWave-Partial", new SinWave(10f,10f,Direction.LeftToRight),new Rectangle(100,100,500,500) }
        };

        [Theory]
        [MemberData(nameof(Filters))]
        public void Run(string name, IFilter filter, Rectangle target)
        {
            CheckCorrect(name, filter, target);
        }
    }
}