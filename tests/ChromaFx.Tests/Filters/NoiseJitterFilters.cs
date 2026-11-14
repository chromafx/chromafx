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
using ChromaFx.Processing.Filters.Effects;
using Xunit;

namespace ChromaFx.Tests.Filters;

/// <summary>
/// Behavioral (non-golden) coverage for unseeded RNG filters. Orphan BMP goldens
/// for Noise/Jitter are intentionally not byte-compared.
/// </summary>
public class NoiseJitterFilters
{
    [Fact]
    public void Noise_PerturbsSomePixels()
    {
        var image = Solid(128, 128, 128, 32, 32);
        var before = Snapshot(image);
        new Noise(40).Apply(image);
        var changed = CountChanged(before, image);
        Assert.True(changed > 0);
    }

    [Fact]
    public void Jitter_MovesPixelsWithinBounds()
    {
        // Distinct left/right halves so displacement is observable.
        var pixels = new Color[16 * 8];
        for (var y = 0; y < 8; y++)
        for (var x = 0; x < 16; x++)
            pixels[y * 16 + x] = x < 8 ? Color.Black : Color.White;

        var image = new Image(16, 8, pixels);
        var before = Snapshot(image);
        new Jitter(3).Apply(image);
        Assert.True(CountChanged(before, image) > 0);

        // Every output pixel remains black or white (gathered from source).
        for (var y = 0; y < image.Height; y++)
        {
            var row = image.GetRowSpan(y);
            for (var x = 0; x < image.Width; x++)
            {
                var c = row[x];
                Assert.True(
                    (c.Red == 0 && c.Green == 0 && c.Blue == 0)
                        || (c.Red == 255 && c.Green == 255 && c.Blue == 255)
                );
            }
        }
    }

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
