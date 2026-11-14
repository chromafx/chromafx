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

using ChromaFx.Processing.Filters.ColorMatrix;
using Xunit;

namespace ChromaFx.Tests.Filters;

public class WhiteBalanceFilters
{
    [Fact]
    public void WhiteBalance_WarmIlluminant_BoostsBlueRelativeToRed()
    {
        // Candle-ish cast: warm pixels need blue lift / red attenuation when correcting.
        var image = Solid(180, 120, 60);
        var before = image.GetRowSpan(0)[0];
        new WhiteBalance(1800).Apply(image);
        var after = image.GetRowSpan(0)[0];

        Assert.True(after.Blue - before.Blue > after.Red - before.Red);
    }

    [Fact]
    public void AutoWhiteBalance_GrayWorld_NeutralizesCast()
    {
        var image = Solid(200, 100, 80);
        new AutoWhiteBalance(AutoWhiteBalanceMethod.GrayWorld).Apply(image);
        var c = image.GetRowSpan(0)[0];
        var spread = Math.Max(c.Red, Math.Max(c.Green, c.Blue)) - Math.Min(c.Red, Math.Min(c.Green, c.Blue));
        Assert.True(spread < 40, $"channels still diverge: R={c.Red} G={c.Green} B={c.Blue}");
    }

    [Fact]
    public void AutoWhiteBalance_WhitePatch_NeutralizesCast()
    {
        var image = Solid(120, 90, 60);
        new AutoWhiteBalance(AutoWhiteBalanceMethod.WhitePatch, 99f).Apply(image);
        var c = image.GetRowSpan(0)[0];
        var spread =
            Math.Max(c.Red, Math.Max(c.Green, c.Blue))
            - Math.Min(c.Red, Math.Min(c.Green, c.Blue));
        Assert.True(spread < 40, $"channels still diverge: R={c.Red} G={c.Green} B={c.Blue}");
    }

    [Fact]
    public void KelvinTint_And_WhiteBalance_AreOpposingDirections()
    {
        var tinted = Solid(128, 128, 128);
        new KelvinTint(1800).Apply(tinted);
        var corrected = tinted.Clone();
        new WhiteBalance(1800).Apply(corrected);

        var t = tinted.GetRowSpan(0)[0];
        var c = corrected.GetRowSpan(0)[0];
        // Tint warms (more R relative to B); WB for same Kelvin should cool back toward neutral.
        Assert.True(t.Red >= t.Blue);
        Assert.True(Math.Abs(c.Red - c.Blue) < Math.Abs(t.Red - t.Blue));
    }

    private static Image Solid(byte r, byte g, byte b)
    {
        var pixels = new Color[16];
        Array.Fill(pixels, new Color(r, g, b));
        return new Image(4, 4, pixels);
    }
}
