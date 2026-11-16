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

using ChromaFx.Processing.Filters.Effects;
using Xunit;

namespace ChromaFx.Tests.Filters;

/// <summary>
/// Behavioral coverage for <see cref="FilmGrain"/> (seeded value noise; not golden BMPs).
/// </summary>
public class FilmGrainFilters
{
    [Fact]
    public void AmountZero_LeavesImageUnchanged()
    {
        var image = Solid(80, 100, 120, 24, 24);
        var before = Snapshot(image);
        new FilmGrain(amount: 0f, seed: 1).Apply(image);
        Assert.Equal(0, CountChanged(before, image));
    }

    [Fact]
    public void SameSeed_IsDeterministic()
    {
        var a = Solid(64, 64, 64, 32, 32);
        var b = Solid(64, 64, 64, 32, 32);
        new FilmGrain(0.4f, 1.5f, 0.1f, seed: 42).Apply(a);
        new FilmGrain(0.4f, 1.5f, 0.1f, seed: 42).Apply(b);
        Assert.Equal(0, CountChanged(Snapshot(a), b));
    }

    [Fact]
    public void DifferentSeeds_Differ()
    {
        var a = Solid(64, 64, 64, 32, 32);
        var b = Solid(64, 64, 64, 32, 32);
        new FilmGrain(0.5f, seed: 1).Apply(a);
        new FilmGrain(0.5f, seed: 2).Apply(b);
        Assert.True(CountChanged(Snapshot(a), b) > 0);
    }

    [Fact]
    public void MonoGrain_KeepsGrayChannelsEqual()
    {
        var image = Solid(128, 128, 128, 16, 16);
        new FilmGrain(0.5f, 1.25f, colorAmount: 0f, seed: 7).Apply(image);
        for (var y = 0; y < image.Height; y++)
        {
            var row = image.GetRowSpan(y);
            for (var x = 0; x < image.Width; x++)
            {
                var c = row[x];
                Assert.Equal(c.Red, c.Green);
                Assert.Equal(c.Green, c.Blue);
            }
        }
    }

    [Fact]
    public void ShadowsPerturbMoreThanHighlights()
    {
        var image = new Image(32, 8);
        for (var y = 0; y < 8; y++)
        {
            var row = image.GetRowSpan(y);
            for (var x = 0; x < 16; x++)
                row[x] = new Color(32, 32, 32);
            for (var x = 16; x < 32; x++)
                row[x] = new Color(220, 220, 220);
        }

        var before = Snapshot(image);
        new FilmGrain(0.6f, 1f, 0f, seed: 99).Apply(image);

        var shadowDelta = 0L;
        var highlightDelta = 0L;
        for (var y = 0; y < 8; y++)
        {
            var row = image.GetRowSpan(y);
            for (var x = 0; x < 16; x++)
                shadowDelta += ChannelAbsDelta(before[y * 32 + x], row[x]);
            for (var x = 16; x < 32; x++)
                highlightDelta += ChannelAbsDelta(before[y * 32 + x], row[x]);
        }

        Assert.True(shadowDelta > highlightDelta);
        Assert.True(shadowDelta > 0);
    }

    private static int ChannelAbsDelta(Color a, Color b) =>
        Math.Abs(a.Red - b.Red) + Math.Abs(a.Green - b.Green) + Math.Abs(a.Blue - b.Blue);

    private static Image Solid(byte r, byte g, byte b, int w, int h)
    {
        var pixels = new Color[w * h];
        Array.Fill(pixels, new Color(r, g, b));
        return new Image(w, h, pixels);
    }

    private static Color[] Snapshot(Image image)
    {
        var copy = new Color[image.Width * image.Height];
        for (var y = 0; y < image.Height; y++)
            image.GetRowSpan(y).CopyTo(copy.AsSpan(y * image.Width, image.Width));
        return copy;
    }

    private static int CountChanged(Color[] before, Image after)
    {
        var n = 0;
        for (var y = 0; y < after.Height; y++)
        {
            var row = after.GetRowSpan(y);
            for (var x = 0; x < after.Width; x++)
            {
                if (row[x] != before[y * after.Width + x])
                    n++;
            }
        }
        return n;
    }
}
