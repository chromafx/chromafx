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

public class PerspectiveSyntheticTests
{
    [Fact]
    public void ZeroAmount_LeavesCenterPixel()
    {
        var image = Solid(32, 32, new Color(200, 100, 50, 255));
        image.Mutate(ctx => ctx.Perspective(0f, 0f));
        Assert.Equal(new Color(200, 100, 50, 255), image[16, 16]);
    }

    [Fact]
    public void Horizontal_DoesNotThrow()
    {
        var image = Solid(64, 48, new Color(10, 20, 30, 255));
        image.Mutate(ctx => ctx.Perspective(0.2f, 0f));
        Assert.Equal(64, image.Width);
        Assert.Equal(48, image.Height);
    }

    private static Image Solid(int w, int h, Color color)
    {
        var data = new Color[w * h];
        Array.Fill(data, color);
        return new Image(w, h, data);
    }
}
