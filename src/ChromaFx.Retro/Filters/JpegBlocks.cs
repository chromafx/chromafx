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
using ChromaFx.Processing.Filters;
using ChromaFx.Processing.Numerics;

namespace ChromaFx.Retro.Filters;

/// <summary>
/// Early low-quality JPEG macroblock look-alike: NxN blocks crush toward a flat average
/// with coarse chroma. Fun 8×8 blocks — not a DCT codec and not an IO round-trip.
/// </summary>
/// <param name="blockSize">
/// Block edge in pixels (clamped to 2…32). Default 8.
/// </param>
/// <param name="strength">
/// How hard to flatten each block (0–1). Default 0.85.
/// </param>
public sealed class JpegBlocks(int blockSize = 8, float strength = 0.85f) : IFilter
{
    /// <summary>Gets the block size in pixels.</summary>
    public int BlockSize { get; } = blockSize;

    /// <summary>Gets the flatten strength (0–1).</summary>
    public float Strength { get; } = strength;

    /// <inheritdoc />
    public Image Apply(Image image, Rectangle targetLocation = default)
    {
        if (image.Width <= 0 || image.Height <= 0)
            return image;

        var amount = Math.Clamp(Strength, 0f, 1f);
        if (amount <= 0f)
            return image;

        var bs = Math.Clamp(BlockSize, 2, 32);
        targetLocation = targetLocation.Normalize(image);
        var source = image.Clone();
        var w = image.Width;
        var h = image.Height;

        // Process block grid covering the target (and full image for clean averages).
        var x0 = targetLocation.Left / bs * bs;
        var y0 = targetLocation.Bottom / bs * bs;

        Parallel.For(
            0,
            (h - y0 + bs - 1) / bs,
            by =>
            {
                var by0 = y0 + by * bs;
                if (by0 >= h || by0 >= targetLocation.Top)
                    return;

                for (var bx0 = x0; bx0 < w && bx0 < targetLocation.Right; bx0 += bs)
                {
                    var bx1 = Math.Min(bx0 + bs, w);
                    var by1 = Math.Min(by0 + bs, h);

                    // Average RGB + luma for the block (from source).
                    long sumR = 0, sumG = 0, sumB = 0, count = 0;
                    for (var y = by0; y < by1; y++)
                    {
                        var row = source.GetRowSpan(y);
                        for (var x = bx0; x < bx1; x++)
                        {
                            var p = row[x];
                            sumR += p.Red;
                            sumG += p.Green;
                            sumB += p.Blue;
                            count++;
                        }
                    }

                    if (count == 0)
                        continue;

                    var avgR = (float)sumR / count;
                    var avgG = (float)sumG / count;
                    var avgB = (float)sumB / count;
                    var avgY = 0.299f * avgR + 0.587f * avgG + 0.114f * avgB;
                    // Coarse chroma (JPEG 4:2:0 vibe): one Cb/Cr for the whole block.
                    var avgCb = avgB - avgY;
                    var avgCr = avgR - avgY;

                    for (var y = by0; y < by1; y++)
                    {
                        if (y < targetLocation.Bottom || y >= targetLocation.Top)
                            continue;
                        var srcRow = source.GetRowSpan(y);
                        var dstRow = image.GetRowSpan(y);
                        for (var x = bx0; x < bx1; x++)
                        {
                            if (x < targetLocation.Left || x >= targetLocation.Right)
                                continue;

                            var src = srcRow[x];
                            var yLuma = 0.299f * src.Red + 0.587f * src.Green + 0.114f * src.Blue;
                            // Mildly quantize luma inside the block (4 levels of crush with strength).
                            var qStep = 8f + amount * 24f;
                            var qY = MathF.Round(yLuma / qStep) * qStep;
                            // Rebuild with block chroma + quantized luma → macroblock flatness.
                            var blockR = Math.Clamp(qY + avgCr, 0f, 255f);
                            var blockG = Math.Clamp(qY - 0.34414f * avgCb - 0.71414f * avgCr, 0f, 255f);
                            var blockB = Math.Clamp(qY + avgCb, 0f, 255f);
                            // Also pull toward pure average for extra "low quality" mush.
                            blockR = blockR * 0.55f + avgR * 0.45f;
                            blockG = blockG * 0.55f + avgG * 0.45f;
                            blockB = blockB * 0.55f + avgB * 0.45f;

                            var crushed = new Color(
                                (byte)(blockR + 0.5f),
                                (byte)(blockG + 0.5f),
                                (byte)(blockB + 0.5f),
                                src.Alpha
                            );
                            dstRow[x] = amount >= 1f ? crushed : Lerp(src, crushed, amount);
                        }
                    }
                }
            }
        );

        return image;
    }

    private static Color Lerp(Color a, Color b, float t) =>
        new(
            (byte)(a.Red + (b.Red - a.Red) * t + 0.5f),
            (byte)(a.Green + (b.Green - a.Green) * t + 0.5f),
            (byte)(a.Blue + (b.Blue - a.Blue) * t + 0.5f),
            a.Alpha
        );
}
