/*
 * Copyright 2026 Ho Tzin Mein
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using ChromaFx.Core;
using ChromaFx.Core.Colors;
using Xunit;

namespace ChromaFx.Tests;

public class ColorDifferenceTests
{
    [Fact]
    public void Cie76_Identical_IsZero()
    {
        var c = new Color(120, 80, 40, 255);
        Assert.Equal(0.0, ColorDifference.Cie76(c, c), 10);
        Assert.Equal(0.0, ColorDifference.Cie76((CieLab)c, (CieLab)c), 10);
    }

    [Fact]
    public void Cie76_BlackWhite_Near100()
    {
        var de = ColorDifference.Cie76(new Color(0, 0, 0, 255), new Color(255, 255, 255, 255));
        Assert.InRange(de, 95.0, 105.0);
    }

    [Fact]
    public void MeanCie76_IdenticalImages_IsZero()
    {
        var a = Solid(8, 8, new Color(50, 100, 150, 255));
        var b = Solid(8, 8, new Color(50, 100, 150, 255));
        Assert.Equal(0.0, ColorDifference.MeanCie76(a, b), 10);
    }

    [Fact]
    public void MeanCie76_MismatchSize_Throws()
    {
        var a = Solid(4, 4, new Color(0, 0, 0, 255));
        var b = Solid(4, 5, new Color(0, 0, 0, 255));
        Assert.Throws<ArgumentException>(() => ColorDifference.MeanCie76(a, b));
    }

    private static Image Solid(int w, int h, Color color)
    {
        var data = new Color[w * h];
        Array.Fill(data, color);
        return new Image(w, h, data);
    }
}
