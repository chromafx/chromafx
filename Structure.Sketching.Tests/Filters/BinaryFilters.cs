﻿using Structure.Sketching.Colors;
using Structure.Sketching.Filters.Binary;
using Structure.Sketching.Filters.Interfaces;
using Structure.Sketching.Tests.BaseClasses;
using Xunit;

namespace Structure.Sketching.Tests.Filters
{
    public class BinaryFilters : FilterTestBaseClass
    {
        public override string ExpectedDirectory => "./ExpectedResults/Filters/";

        public override string OutputDirectory => "./TestOutput/Filters/";

        public static readonly TheoryData<string, IFilter, Sketching.Numerics.Rectangle> Filters = new()
        {
            { "AdaptiveThreshold", new AdaptiveThreshold(10,Color.White,Color.Black,.5f),default },
            { "Threshold", new Threshold(Color.White,Color.Black,.5f),default },
            { "AdaptiveThreshold-Partial", new AdaptiveThreshold(10,Color.White,Color.Black,.5f),new Sketching.Numerics.Rectangle(100,100,500,500) },
            { "Threshold-Partial", new Threshold(Color.White,Color.Black,.5f),new Sketching.Numerics.Rectangle(100,100,500,500) },
            { "NonMaximalSuppression", new NonMaximalSuppression(Color.White,Color.Black,0.8f,0.5f),default },
            { "NonMaximalSuppression-Partial", new NonMaximalSuppression(Color.White,Color.Black,0.8f,0.5f),new Sketching.Numerics.Rectangle(100,100,500,500) }
        };

        [Theory]
        [MemberData(nameof(Filters))]
        public void Run(string name, IFilter filter, Sketching.Numerics.Rectangle target)
        {
            CheckCorrect(name, filter, target);
        }
    }
}