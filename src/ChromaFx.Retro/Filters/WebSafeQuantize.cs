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
using ChromaFx.Core.Quantization;
using ChromaFx.Processing.Filters;
using ChromaFx.Processing.Numerics;

namespace ChromaFx.Retro.Filters;

/// <summary>
/// Snaps each RGB channel to the Netscape web-safe 6×6×6 cube
/// (<c>00</c>, <c>33</c>, <c>66</c>, <c>99</c>, <c>CC</c>, <c>FF</c>). Alpha is preserved.
/// Optional ordered dither biases channels before the LUT snap.
/// </summary>
/// <seealso cref="IFilter"/>
/// <param name="dither">Ordered dither method; default is none.</param>
/// <param name="strength">Dither strength (0 = none; ~0.3–0.6 typical). Ignored when dither is <see cref="OrderedDitherMethod.None"/>.</param>
public sealed class WebSafeQuantize(
    OrderedDitherMethod dither = OrderedDitherMethod.None,
    float strength = 0.5f
) : IFilter
{
    private static readonly byte[] Lut = CreateLut();

    /// <summary>Gets the dither method.</summary>
    public OrderedDitherMethod Dither { get; } = dither;

    /// <summary>Gets the dither strength.</summary>
    public float Strength { get; } = strength;

    /// <inheritdoc />
    public Image Apply(Image image, Rectangle targetLocation = default) =>
        ChannelLutFilter.Apply(image, targetLocation, Lut, Dither, Strength);

    /// <summary>Maps a single channel to the nearest web-safe level.</summary>
    internal static byte SnapChannel(byte value) => Lut[value];

    private static byte[] CreateLut()
    {
        var lut = new byte[256];
        for (var i = 0; i < 256; ++i)
            lut[i] = (byte)(((i + 25) / 51) * 51);
        return lut;
    }
}
