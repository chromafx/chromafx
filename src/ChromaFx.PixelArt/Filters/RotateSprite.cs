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
using ChromaFx.Processing.Filters.Interfaces;
using ChromaFx.Processing.Numerics;

namespace ChromaFx.PixelArt.Filters;

/// <summary>
/// RotSprite (Xenowhirl): similarity Scale2x to 8× (choose, never blend), pick a
/// sharp 0…7 sample offset, nearest-neighbor rotate while scaling back to 1× about
/// the sprite center, then restore overlooked single-pixel details. Output size is
/// the axis-aligned bounds of the rotated sprite. Cardinal angles (90 / 180 / 270)
/// use exact pixel remaps. Uncovered samples use <see cref="EmptyColor"/>.
/// Positive <see cref="Degrees"/> is counterclockwise.
/// </summary>
/// <seealso cref="IFilter"/>
/// <remarks>
/// Initializes a new instance of the <see cref="RotateSprite"/> class.
/// </remarks>
/// <param name="degrees">Rotation angle in degrees (counterclockwise).</param>
/// <param name="emptyColor">
/// Fill for samples outside the sprite. Default (<c>default</c> / transparent black)
/// resolves to opaque black so uncovered areas are not see-through.
/// </param>
public class RotateSprite(float degrees, Color emptyColor = default) : IFilter
{
    private const int ScaleFactor = 8;

    /// <summary>
    /// RGB squared-distance threshold for “similar” in the RotSprite Scale2x pre-pass
    /// (classic Scale2x uses exact equality).
    /// </summary>
    private const long SimilarityThreshold = 400;

    /// <summary>Opaque black used when empty fill is left at default.</summary>
    private static readonly Color OpaqueBlack = new(0, 0, 0, 255);

    /// <summary>Rotation angle in degrees (counterclockwise).</summary>
    public float Degrees { get; } = degrees;

    /// <summary>Fill for out-of-bounds samples (opaque black when ctor default).</summary>
    public Color EmptyColor { get; } = emptyColor.IntData == 0 ? OpaqueBlack : emptyColor;

    /// <inheritdoc />
    public Image Apply(Image image, Rectangle targetLocation = default)
    {
        if (image.Width <= 0 || image.Height <= 0)
            return image;

        var angle = PixelArtRotation.NormalizeDegrees(Degrees);
        if (angle is 0f)
            return image;

        var srcW = image.Width;
        var srcH = image.Height;
        var source = CopyTight(image);

        if (PixelArtRotation.IsCardinal(angle))
        {
            var (cardW, cardH, cardinal) = PixelArtRotation.ApplyCardinal(
                source,
                srcW,
                srcH,
                angle
            );
            image.ReCreate(cardW, cardH, cardinal);
            return image;
        }

        var radians = angle * (MathF.PI / 180f);
        // Forward (positive) rotation for AABB; sampling uses inverse (−angle).
        var sin = MathF.Sin(radians);
        var cos = MathF.Cos(radians);
        var (minX, minY, outW, outH) = PixelArtRotation.ComputeRotatedBounds(
            srcW,
            srcH,
            sin,
            cos
        );

        var (scaled, scaledW, scaledH) = UpscaleByFactor(source, srcW, srcH, ScaleFactor);
        var (ox, oy) = FindBestOffset(
            scaled,
            scaledW,
            scaledH,
            srcW,
            srcH,
            outW,
            outH,
            minX,
            minY,
            radians
        );
        var resultBuf = RotateAndDownscale(
            scaled,
            scaledW,
            scaledH,
            srcW,
            srcH,
            outW,
            outH,
            minX,
            minY,
            radians,
            ox,
            oy,
            EmptyColor
        );
        CorrectDetails(resultBuf, outW, outH, source, srcW, srcH, minX, minY, radians);

        image.ReCreate(outW, outH, resultBuf);
        return image;
    }

    /// <summary>Three similarity-Scale2x passes → 8× (choose pixels, never blend).</summary>
    private static (Color[] Buf, int Width, int Height) UpscaleByFactor(
        Color[] source,
        int width,
        int height,
        int factor
    )
    {
        var buf = source;
        var w = width;
        var h = height;
        for (var current = 1; current < factor; current *= 2)
        {
            buf = Upscale2xSimilar(buf, w, h);
            w *= 2;
            h *= 2;
        }

        return (buf, w, h);
    }

    private static Color[] Upscale2xSimilar(Color[] source, int srcW, int srcH)
    {
        var dstW = srcW * 2;
        var output = new Color[dstW * srcH * 2];

        for (var y = 0; y < srcH; ++y)
        {
            var outY0 = y * 2 * dstW;
            var outY1 = outY0 + dstW;
            for (var x = 0; x < srcW; ++x)
            {
                var p = source[y * srcW + x];
                var a = y > 0 ? source[(y - 1) * srcW + x] : p;
                var b = x + 1 < srcW ? source[y * srcW + x + 1] : p;
                var c = x > 0 ? source[y * srcW + x - 1] : p;
                var d = y + 1 < srcH ? source[(y + 1) * srcW + x] : p;

                //   A
                // C P B
                //   D
                var e0 = Similar(c, a) && !Similar(c, d) && !Similar(a, b) ? a : p;
                var e1 = Similar(a, b) && !Similar(a, c) && !Similar(b, d) ? b : p;
                var e2 = Similar(d, c) && !Similar(d, b) && !Similar(c, a) ? c : p;
                var e3 = Similar(b, d) && !Similar(b, a) && !Similar(d, c) ? d : p;

                var ox = x * 2;
                output[outY0 + ox] = e0;
                output[outY0 + ox + 1] = e1;
                output[outY1 + ox] = e2;
                output[outY1 + ox + 1] = e3;
            }
        }

        return output;
    }

    private static bool Similar(Color u, Color v) => SquaredRgbDistance(u, v) <= SimilarityThreshold;

    private static (int OffsetX, int OffsetY) FindBestOffset(
        Color[] scaled,
        int scaledW,
        int scaledH,
        int srcW,
        int srcH,
        int outW,
        int outH,
        float minX,
        float minY,
        float radians
    )
    {
        PixelArtRotation.InverseCosSin(radians, out var cos, out var sin);
        var upCx = scaledW / 2;
        var upCy = scaledH / 2;
        var srcCx = srcW / 2.0;
        var srcCy = srcH / 2.0;

        var bestScore = double.MaxValue;
        var bestX = 0;
        var bestY = 0;

        for (var oy = 0; oy < ScaleFactor; ++oy)
        {
            for (var ox = 0; ox < ScaleFactor; ++ox)
            {
                double score = 0;
                for (var y = 0; y < outH; ++y)
                {
                    for (var x = 0; x < outW; ++x)
                    {
                        MapToUpscaled(
                            x,
                            y,
                            minX,
                            minY,
                            srcCx,
                            srcCy,
                            cos,
                            sin,
                            upCx,
                            upCy,
                            ox,
                            oy,
                            out var sx,
                            out var sy
                        );
                        var center = SampleNearest(scaled, scaledW, scaledH, sx, sy, default);
                        var right = SampleNearest(scaled, scaledW, scaledH, sx + 1, sy, default);
                        var down = SampleNearest(scaled, scaledW, scaledH, sx, sy + 1, default);
                        score += SquaredRgbDistance(center, right);
                        score += SquaredRgbDistance(center, down);
                    }
                }

                if (score < bestScore)
                {
                    bestScore = score;
                    bestX = ox;
                    bestY = oy;
                }
            }
        }

        return (bestX, bestY);
    }

    private static Color[] RotateAndDownscale(
        Color[] scaled,
        int scaledW,
        int scaledH,
        int srcW,
        int srcH,
        int outW,
        int outH,
        float minX,
        float minY,
        float radians,
        int offsetX,
        int offsetY,
        Color empty
    )
    {
        PixelArtRotation.InverseCosSin(radians, out var cos, out var sin);
        var upCx = scaledW / 2;
        var upCy = scaledH / 2;
        var srcCx = srcW / 2.0;
        var srcCy = srcH / 2.0;
        var output = new Color[outW * outH];

        for (var y = 0; y < outH; ++y)
        {
            for (var x = 0; x < outW; ++x)
            {
                MapToUpscaled(
                    x,
                    y,
                    minX,
                    minY,
                    srcCx,
                    srcCy,
                    cos,
                    sin,
                    upCx,
                    upCy,
                    offsetX,
                    offsetY,
                    out var sx,
                    out var sy
                );
                output[y * outW + x] = SampleNearest(scaled, scaledW, scaledH, sx, sy, empty);
            }
        }

        return output;
    }

    private static void CorrectDetails(
        Color[] output,
        int outW,
        int outH,
        Color[] source,
        int srcW,
        int srcH,
        float minX,
        float minY,
        float radians
    )
    {
        PixelArtRotation.InverseCosSin(radians, out var cos, out var sin);
        var srcCx = srcW / 2.0;
        var srcCy = srcH / 2.0;
        var fixes = new List<(int X, int Y, Color Color)>();

        for (var y = 0; y < outH; ++y)
        {
            for (var x = 0; x < outW; ++x)
            {
                var pixel = output[y * outW + x];
                PixelArtRotation.MapToSource(
                    x,
                    y,
                    minX,
                    minY,
                    srcCx,
                    srcCy,
                    cos,
                    sin,
                    out var sx,
                    out var sy
                );
                if (!TrySampleNearest(source, srcW, srcH, sx, sy, out var corresponding))
                    continue;
                if (pixel == corresponding)
                    continue;
                if (CountIdenticalNeighbors(output, outW, outH, x, y, pixel) >= 3)
                    fixes.Add((x, y, corresponding));
            }
        }

        foreach (var (x, y, color) in fixes)
            output[y * outW + x] = color;
    }

    private static int CountIdenticalNeighbors(
        Color[] buf,
        int width,
        int height,
        int x,
        int y,
        Color reference
    )
    {
        var count = 0;
        if (x > 0 && buf[y * width + x - 1] == reference)
            ++count;
        if (x + 1 < width && buf[y * width + x + 1] == reference)
            ++count;
        if (y > 0 && buf[(y - 1) * width + x] == reference)
            ++count;
        if (y + 1 < height && buf[(y + 1) * width + x] == reference)
            ++count;
        return count;
    }

    /// <summary>
    /// Output pixel (x,y) sits at rotated-space (minX+x, minY+y); inverse-rotate about
    /// the source center into 1× source space, then lift into the 8× buffer + offset.
    /// </summary>
    private static void MapToUpscaled(
        int x,
        int y,
        float minX,
        float minY,
        double srcCx,
        double srcCy,
        double cos,
        double sin,
        int upCx,
        int upCy,
        double offsetX,
        double offsetY,
        out double sx,
        out double sy
    )
    {
        PixelArtRotation.MapToSource(
            x,
            y,
            minX,
            minY,
            srcCx,
            srcCy,
            cos,
            sin,
            out var srcX,
            out var srcY
        );
        sx = (srcX - srcCx) * ScaleFactor + upCx + offsetX;
        sy = (srcY - srcCy) * ScaleFactor + upCy + offsetY;
    }

    private static Color SampleNearest(
        Color[] buf,
        int width,
        int height,
        double x,
        double y,
        Color empty
    ) => TrySampleNearest(buf, width, height, x, y, out var c) ? c : empty;

    private static bool TrySampleNearest(
        Color[] buf,
        int width,
        int height,
        double x,
        double y,
        out Color color
    )
    {
        var ix = (int)Math.Round(x);
        var iy = (int)Math.Round(y);
        if ((uint)ix >= (uint)width || (uint)iy >= (uint)height)
        {
            color = default;
            return false;
        }

        color = buf[iy * width + ix];
        return true;
    }

    private static long SquaredRgbDistance(Color a, Color b)
    {
        long dr = a.Red - b.Red;
        long dg = a.Green - b.Green;
        long db = a.Blue - b.Blue;
        return dr * dr + dg * dg + db * db;
    }

    private static Color[] CopyTight(Image image)
    {
        var buf = new Color[image.Width * image.Height];
        for (var y = 0; y < image.Height; ++y)
            image.GetRowSpan(y).CopyTo(buf.AsSpan(y * image.Width, image.Width));
        return buf;
    }
}
