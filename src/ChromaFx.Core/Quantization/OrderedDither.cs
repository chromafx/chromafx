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

namespace ChromaFx.Core.Quantization;

/// <summary>
/// Shared Bayer matrices and channel bias for ordered dither.
/// Callers choose scale / rounding so Quantize (~±32 at strength 1)
/// and Retro (~±127.5 at strength 1) stay bit-identical to their prior formulas.
/// </summary>
public static class OrderedDither
{
    /// <summary>
    /// Scale for Processing <c>Quantize</c> ordered dither:
    /// <c>(t − 0.5) × strength × 64</c> (equivalent to the old ±32 × 2 formula).
    /// </summary>
    public const float QuantizeBiasScale = 64f;

    /// <summary>
    /// Scale for Retro palette dither: <c>round((t − 0.5) × strength × 255)</c>.
    /// </summary>
    public const float RetroBiasScale = 255f;

    private static readonly byte[] Bayer4 =
    [
        0, 8, 2, 10,
        12, 4, 14, 6,
        3, 11, 1, 9,
        15, 7, 13, 5,
    ];

    private static readonly byte[] Bayer8 =
    [
        0, 32, 8, 40, 2, 34, 10, 42,
        48, 16, 56, 24, 50, 18, 58, 26,
        12, 44, 4, 36, 14, 46, 6, 38,
        60, 28, 52, 20, 62, 30, 54, 22,
        3, 35, 11, 43, 1, 33, 9, 41,
        51, 19, 59, 27, 49, 17, 57, 25,
        15, 47, 7, 39, 13, 45, 5, 37,
        63, 31, 55, 23, 61, 29, 53, 21,
    ];

    /// <summary>
    /// Bayer threshold in (0, 1) for the cell at (<paramref name="x"/>, <paramref name="y"/>).
    /// Returns 0.5 when <paramref name="method"/> is <see cref="OrderedDitherMethod.None"/>.
    /// </summary>
    public static float Threshold(int x, int y, OrderedDitherMethod method)
    {
        if (method == OrderedDitherMethod.Bayer4x4)
        {
            var cell = Bayer4[(y & 3) * 4 + (x & 3)];
            return (cell + 0.5f) / 16f;
        }

        if (method == OrderedDitherMethod.Bayer8x8)
        {
            var cell = Bayer8[(y & 7) * 8 + (x & 7)];
            return (cell + 0.5f) / 64f;
        }

        return 0.5f;
    }

    /// <summary>
    /// Integer channel bias: <c>(t − 0.5) × strength × scale</c>, optionally rounded.
    /// </summary>
    public static int ChannelBias(
        int x,
        int y,
        OrderedDitherMethod method,
        float strength,
        float scale,
        bool round = false
    )
    {
        if (method == OrderedDitherMethod.None || strength == 0f)
            return 0;

        var v = (Threshold(x, y, method) - 0.5f) * strength * scale;
        return round ? (int)MathF.Round(v) : (int)v;
    }

    /// <summary>
    /// Biases a channel and clamps to 0–255.
    /// </summary>
    public static byte BiasChannel(
        byte value,
        int x,
        int y,
        OrderedDitherMethod method,
        float strength,
        float scale,
        bool round = false
    )
    {
        var bias = ChannelBias(x, y, method, strength, scale, round);
        if (bias == 0)
            return value;
        return (byte)Math.Clamp(value + bias, 0, 255);
    }

    /// <summary>
    /// Precomputes per-cell biases for a Bayer matrix (length 16 or 64). Empty when method is None.
    /// Index as <c>(y &amp; mask) * side + (x &amp; mask)</c> with <c>side</c> 4 or 8.
    /// </summary>
    public static int[] BuildBiasTable(
        OrderedDitherMethod method,
        float strength,
        float scale,
        bool round = false
    )
    {
        if (method == OrderedDitherMethod.None || strength == 0f)
            return [];

        var side = method == OrderedDitherMethod.Bayer4x4 ? 4 : 8;
        var table = new int[side * side];
        for (var y = 0; y < side; y++)
        {
            for (var x = 0; x < side; x++)
                table[y * side + x] = ChannelBias(x, y, method, strength, scale, round);
        }

        return table;
    }
}
