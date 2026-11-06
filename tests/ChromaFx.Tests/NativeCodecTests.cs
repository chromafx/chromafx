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

using ChromaFx.Native.Encoders;
using ChromaFx.Native.Formats.Avif;
using ChromaFx.Native.Formats.Jxl;
using ChromaFx.Native.Formats.WebP;
using Xunit;

namespace ChromaFx.Tests;

/// <summary>
/// Still WebP, AVIF, and JPEG XL round-trips. Requires native/build.sh for this machine.
/// </summary>
public class NativeCodecTests
{
    [Fact]
    public void WebPProbeMatchesRiffHeader()
    {
        var format = new WebPFormat();
        Assert.True(format.CanDecode("shot.WEBP"));
        Assert.False(format.CanDecode("shot.png"));
        Assert.True(format.CanDecode("RIFF...."u8.ToArray().Concat("WEBP"u8.ToArray()).ToArray()));
    }

    [Fact]
    public void AvifProbeMatchesFtyp()
    {
        var format = new AvifFormat();
        Assert.True(format.CanDecode("shot.avif"));
        Assert.False(format.CanDecode("shot.png"));
        var header = new byte[16];
        header[4] = (byte)'f';
        header[5] = (byte)'t';
        header[6] = (byte)'y';
        header[7] = (byte)'p';
        header[8] = (byte)'a';
        header[9] = (byte)'v';
        header[10] = (byte)'i';
        header[11] = (byte)'f';
        Assert.True(format.CanDecode(header));
    }

    [Fact]
    public void WebPLosslessRoundTrip_IsExact()
    {
        var src = Sample(17, 9, alpha: true);
        var decoded = RoundTrip(src, new WebPEncoder { Lossless = true }, new WebPFormat());
        AssertPixels(src, decoded, maxChannelDelta: 0);
    }

    [Fact]
    public void WebPLossyRoundTrip_StaysWithinTolerance()
    {
        var src = Sample(16, 8, alpha: false);
        var decoded = RoundTrip(src, new WebPEncoder { Quality = 90 }, new WebPFormat());
        AssertPixels(src, decoded, maxChannelDelta: 28);
    }

    [Fact]
    public void AvifLosslessRoundTrip_IsExact()
    {
        var src = Sample(8, 8, alpha: true);
        var decoded = RoundTrip(src, new AvifEncoder { Lossless = true }, new AvifFormat());
        AssertPixels(src, decoded, maxChannelDelta: 0);
    }

    [Fact]
    public void AvifLossyRoundTrip_StaysWithinTolerance()
    {
        var src = Sample(8, 8, alpha: false);
        var decoded = RoundTrip(src, new AvifEncoder { Quality = 50 }, new AvifFormat());
        AssertPixels(src, decoded, maxChannelDelta: 40);
    }

    [Fact]
    public void JxlProbeMatchesSignatures()
    {
        var format = new JxlFormat();
        Assert.True(format.CanDecode("shot.jxl"));
        Assert.False(format.CanDecode("shot.png"));
        Assert.True(format.CanDecode([0xFF, 0x0A]));
        var container = new byte[12];
        container[3] = 0x0C;
        container[4] = (byte)'J';
        container[5] = (byte)'X';
        container[6] = (byte)'L';
        container[7] = (byte)' ';
        Assert.True(format.CanDecode(container));
    }

    [Fact]
    public void JxlLosslessRoundTrip_IsExact()
    {
        var src = Sample(17, 9, alpha: true);
        var decoded = RoundTrip(src, new JxlEncoder { Lossless = true }, new JxlFormat());
        AssertPixels(src, decoded, maxChannelDelta: 0);
    }

    [Fact]
    public void JxlLossyRoundTrip_StaysWithinTolerance()
    {
        var src = Sample(8, 8, alpha: false);
        var decoded = RoundTrip(src, new JxlEncoder { Quality = 90 }, new JxlFormat());
        AssertPixels(src, decoded, maxChannelDelta: 40);
    }

    [Fact]
    public void WebPLoadUsesRegisteredCodec()
    {
        var src = Sample(4, 4, alpha: false);
        using var ms = new MemoryStream();
        src.Save(ms, new WebPEncoder { Lossless = true });
        var loaded = Image.Load(ms.ToArray());
        AssertPixels(src, loaded, maxChannelDelta: 0);
    }

    private static Image RoundTrip(Image src, IImageEncoder encoder, IImageFormat format)
    {
        using var ms = new MemoryStream();
        src.Save(ms, encoder);
        Assert.True(ms.Length > 12);
        ms.Position = 0;
        return format.Decode(ms);
    }

    private static Image Sample(int width, int height, bool alpha)
    {
        var image = new Image(width, height);
        for (var y = 0; y < height; y++)
        {
            var row = image.GetRowSpan(y);
            for (var x = 0; x < width; x++)
            {
                row[x] = new Color(
                    (byte)(20 + x * 12),
                    (byte)(30 + y * 18),
                    (byte)(200 - x * 7),
                    alpha ? (byte)(255 - y * 10) : (byte)255
                );
            }
        }

        return image;
    }

    private static void AssertPixels(Image expected, Image actual, int maxChannelDelta)
    {
        Assert.Equal(expected.Width, actual.Width);
        Assert.Equal(expected.Height, actual.Height);
        for (var y = 0; y < expected.Height; y++)
        {
            var exp = expected.GetRowSpan(y);
            var got = actual.GetRowSpan(y);
            for (var x = 0; x < expected.Width; x++)
            {
                Assert.True(Math.Abs(exp[x].Red - got[x].Red) <= maxChannelDelta);
                Assert.True(Math.Abs(exp[x].Green - got[x].Green) <= maxChannelDelta);
                Assert.True(Math.Abs(exp[x].Blue - got[x].Blue) <= maxChannelDelta);
                Assert.True(Math.Abs(exp[x].Alpha - got[x].Alpha) <= maxChannelDelta);
            }
        }
    }
}
