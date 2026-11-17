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
using ChromaFx.Processing;
using Xunit;

namespace ChromaFx.Tests.Filters;

public class CurvesSyntheticTests
{
    [Fact]
    public void IdentityCurve_LeavesPixelsUnchanged()
    {
        var image = new Image(2, 1, [new Color(40, 80, 120, 200), new Color(10, 20, 30, 255)]);
        image.Mutate(ctx => ctx.Curves(0f, 0.5f, 1f));
        Assert.Equal(new Color(40, 80, 120, 200), image[0, 0]);
        Assert.Equal(new Color(10, 20, 30, 255), image[1, 0]);
    }

    [Fact]
    public void MidLift_BrightensMidtones()
    {
        var image = new Image(1, 1, [new Color(128, 128, 128, 255)]);
        image.Mutate(ctx => ctx.Curves(0f, 0.65f, 1f));
        var c = image[0, 0];
        Assert.True(c.Red > 128);
        Assert.Equal(c.Red, c.Green);
        Assert.Equal(c.Green, c.Blue);
        Assert.Equal(255, c.Alpha);
    }

    [Fact]
    public void CustomPoints_BlackToWhite()
    {
        var image = new Image(1, 1, [new Color(0, 0, 0, 255)]);
        image.Mutate(ctx => ctx.Curves([(0f, 1f), (1f, 1f)]));
        Assert.Equal(new Color(255, 255, 255, 255), image[0, 0]);
    }
}
