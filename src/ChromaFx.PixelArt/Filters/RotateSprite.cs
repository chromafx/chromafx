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
/// Pixel-art–friendly rotation (RotSprite-style): Scale2x to 8×, pick a sharp
/// sample offset, then nearest-neighbor rotate while scaling back to 1×.
/// Does not introduce new colors. Positive <see cref="Degrees"/> is counterclockwise.
/// </summary>
/// <seealso cref="IFilter"/>
/// <remarks>
/// Initializes a new instance of the <see cref="RotateSprite"/> class.
/// </remarks>
/// <param name="degrees">Rotation angle in degrees (counterclockwise).</param>
/// <param name="emptyColor">Fill for pixels outside the rotated sprite.</param>
public class RotateSprite(float degrees, Color emptyColor = default) : IFilter
{
    private const int ScaleFactor = 8;

    /// <summary>Rotation angle in degrees (counterclockwise).</summary>
    public float Degrees { get; } = degrees;

    /// <summary>Fill for uncovered pixels after rotation.</summary>
    public Color EmptyColor { get; } = emptyColor;

    /// <inheritdoc />
    public Image Apply(Image image, Rectangle targetLocation = default)
    {
        if (image.Width <= 0 || image.Height <= 0)
            return image;

        var angle = NormalizeDegrees(Degrees);
        if (angle is 0f)
            return image;

        var work = image.Clone();
        Scale2xHelpers.ApplyPasses(work, 3);

        var scaled = CopyTight(work);
        var scaledW = work.Width;
        var scaledH = work.Height;

        Color[] result;
        int outW;
        int outH;

        if (IsOrthogonal(angle))
        {
            (outW, outH, result) = ApplyOrthogonal(scaled, scaledW, scaledH, angle, EmptyColor);
        }
        else
        {
            var radians = angle * (MathF.PI / 180f);
            var sin = MathF.Sin(radians);
            var cos = MathF.Cos(radians);
            var (minX, minY, highW, highH) = ComputeRotatedBounds(scaledW, scaledH, sin, cos);
            outW = Math.Max(1, (int)MathF.Ceiling(highW / ScaleFactor));
            outH = Math.Max(1, (int)MathF.Ceiling(highH / ScaleFactor));
            var (ox, oy) = FindBestOffset(
                scaled,
                scaledW,
                scaledH,
                outW,
                outH,
                minX,
                minY,
                sin,
                cos
            );
            result = RotateAndDownscale(
                scaled,
                scaledW,
                scaledH,
                outW,
                outH,
                minX,
                minY,
                sin,
                cos,
                ox,
                oy,
                EmptyColor
            );
        }

        image.ReCreate(outW, outH, result);
        return image;
    }

    private static float NormalizeDegrees(float degrees)
    {
        var a = degrees % 360f;
        if (a < 0f)
            a += 360f;
        for (var i = 0; i <= 3; ++i)
        {
            var target = i * 90f;
            if (Math.Abs(a - target) < 1e-3f || (target == 0f && Math.Abs(a - 360f) < 1e-3f))
                return target >= 360f ? 0f : target;
        }

        return a;
    }

    private static bool IsOrthogonal(float angle) =>
        angle is 0f or 90f or 180f or 270f;

    private static (int Width, int Height, Color[] Pixels) ApplyOrthogonal(
        Color[] scaled,
        int scaledW,
        int scaledH,
        float angle,
        Color empty
    )
    {
        var (w, h, down) = Downscale(scaled, scaledW, scaledH, ScaleFactor, empty);
        return angle switch
        {
            90f => Rotate90(down, w, h),
            180f => Rotate180(down, w, h),
            270f => Rotate270(down, w, h),
            _ => (w, h, down),
        };
    }

    private static (float MinX, float MinY, float HighW, float HighH) ComputeRotatedBounds(
        int width,
        int height,
        float sin,
        float cos
    )
    {
        var fw = (float)width;
        var fh = (float)height;
        Span<(float X, float Y)> corners =
        [
            RotatePoint(0, 0, sin, cos),
            RotatePoint(fw, 0, sin, cos),
            RotatePoint(0, fh, sin, cos),
            RotatePoint(fw, fh, sin, cos),
        ];

        var minX = corners[0].X;
        var maxX = corners[0].X;
        var minY = corners[0].Y;
        var maxY = corners[0].Y;
        for (var i = 1; i < 4; ++i)
        {
            minX = MathF.Min(minX, corners[i].X);
            maxX = MathF.Max(maxX, corners[i].X);
            minY = MathF.Min(minY, corners[i].Y);
            maxY = MathF.Max(maxY, corners[i].Y);
        }

        return (minX, minY, maxX - minX, maxY - minY);
    }

    /// <summary>
    /// Picks an 0..7 phase that samples away from EPX block edges for this angle
    /// (minimal squared color distance to immediate neighbors at sample sites).
    /// </summary>
    private static (int OffsetX, int OffsetY) FindBestOffset(
        Color[] scaled,
        int width,
        int height,
        int outW,
        int outH,
        float minX,
        float minY,
        float sin,
        float cos
    )
    {
        var bestScore = long.MaxValue;
        var bestX = 0;
        var bestY = 0;

        for (var oy = 0; oy < ScaleFactor; ++oy)
        {
            for (var ox = 0; ox < ScaleFactor; ++ox)
            {
                long score = 0;
                for (var y = 0; y < outH; ++y)
                {
                    for (var x = 0; x < outW; ++x)
                    {
                        if (!TryMapSource(x, y, ox, oy, minX, minY, sin, cos, width, height, out var sx, out var sy))
                            continue;

                        var c = scaled[sy * width + sx];
                        score += NeighborDiff(scaled, width, height, c, sx - 1, sy);
                        score += NeighborDiff(scaled, width, height, c, sx + 1, sy);
                        score += NeighborDiff(scaled, width, height, c, sx, sy - 1);
                        score += NeighborDiff(scaled, width, height, c, sx, sy + 1);
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
        int width,
        int height,
        int outW,
        int outH,
        float minX,
        float minY,
        float sin,
        float cos,
        int offsetX,
        int offsetY,
        Color empty
    )
    {
        var output = new Color[outW * outH];
        Array.Fill(output, empty);

        for (var y = 0; y < outH; ++y)
        {
            for (var x = 0; x < outW; ++x)
            {
                if (
                    TryMapSource(
                        x,
                        y,
                        offsetX,
                        offsetY,
                        minX,
                        minY,
                        sin,
                        cos,
                        width,
                        height,
                        out var sx,
                        out var sy
                    )
                )
                    output[y * outW + x] = scaled[sy * width + sx];
            }
        }

        return output;
    }

    private static bool TryMapSource(
        int outX,
        int outY,
        int offsetX,
        int offsetY,
        float minX,
        float minY,
        float sin,
        float cos,
        int width,
        int height,
        out int sx,
        out int sy
    )
    {
        var hx = outX * ScaleFactor + offsetX + 0.5f;
        var hy = outY * ScaleFactor + offsetY + 0.5f;
        var wx = hx + minX;
        var wy = hy + minY;
        var srcX = wx * cos + wy * sin;
        var srcY = -wx * sin + wy * cos;
        sx = (int)MathF.Floor(srcX);
        sy = (int)MathF.Floor(srcY);
        return (uint)sx < (uint)width && (uint)sy < (uint)height;
    }

    private static long NeighborDiff(Color[] buf, int width, int height, Color center, int nx, int ny)
    {
        if ((uint)nx >= (uint)width || (uint)ny >= (uint)height)
            return 0;
        return SquaredDistance(center, buf[ny * width + nx]);
    }

    private static long SquaredDistance(Color a, Color b)
    {
        long dr = a.Red - b.Red;
        long dg = a.Green - b.Green;
        long db = a.Blue - b.Blue;
        long da = a.Alpha - b.Alpha;
        return dr * dr + dg * dg + db * db + da * da;
    }

    private static (float X, float Y) RotatePoint(float x, float y, float sin, float cos) =>
        (x * cos - y * sin, x * sin + y * cos);

    private static (int Width, int Height, Color[] Pixels) Downscale(
        Color[] buf,
        int width,
        int height,
        int factor,
        Color empty
    )
    {
        var newW = Math.Max(1, width / factor);
        var newH = Math.Max(1, height / factor);
        var scaled = new Color[newW * newH];
        Array.Fill(scaled, empty);
        for (var y = 0; y < newH; ++y)
        {
            var srcY = y * factor;
            for (var x = 0; x < newW; ++x)
                scaled[y * newW + x] = buf[srcY * width + x * factor];
        }

        return (newW, newH, scaled);
    }

    private static (int Width, int Height, Color[] Pixels) Rotate90(Color[] buf, int width, int height)
    {
        var rotated = new Color[buf.Length];
        for (var y = 0; y < height; ++y)
        {
            for (var x = 0; x < width; ++x)
                rotated[x * height + (height - 1 - y)] = buf[y * width + x];
        }

        return (height, width, rotated);
    }

    private static (int Width, int Height, Color[] Pixels) Rotate180(Color[] buf, int width, int height)
    {
        var rotated = new Color[buf.Length];
        for (var i = 0; i < buf.Length; ++i)
            rotated[buf.Length - 1 - i] = buf[i];
        return (width, height, rotated);
    }

    private static (int Width, int Height, Color[] Pixels) Rotate270(Color[] buf, int width, int height)
    {
        var (w, h, r90) = Rotate90(buf, width, height);
        return Rotate180(r90, w, h);
    }

    private static Color[] CopyTight(Image image)
    {
        var buf = new Color[image.Width * image.Height];
        for (var y = 0; y < image.Height; ++y)
            image.GetRowSpan(y).CopyTo(buf.AsSpan(y * image.Width, image.Width));
        return buf;
    }
}
