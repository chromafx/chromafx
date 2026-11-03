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

using ChromaFx.Core.Colors;

namespace ChromaFx.PixelArt;

/// <summary>Shared AABB / cardinal helpers for pixel-art rotation filters.</summary>
internal static class PixelArtRotation
{
    public static float NormalizeDegrees(float degrees)
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

    public static bool IsCardinal(float angle) => angle is 90f or 180f or 270f;

    public static (int Width, int Height, Color[] Pixels) ApplyCardinal(
        Color[] source,
        int width,
        int height,
        float angle
    ) =>
        angle switch
        {
            90f => Rotate90(source, width, height),
            180f => Rotate180(source, width, height),
            270f => Rotate270(source, width, height),
            _ => (width, height, source),
        };

    /// <summary>
    /// Axis-aligned bounds of the source rectangle rotated about its center (forward angle).
    /// </summary>
    public static (float MinX, float MinY, int OutW, int OutH) ComputeRotatedBounds(
        int width,
        int height,
        float sin,
        float cos
    )
    {
        var cx = width / 2f;
        var cy = height / 2f;
        Span<(float X, float Y)> corners =
        [
            RotateAbout(0, 0, cx, cy, sin, cos),
            RotateAbout(width, 0, cx, cy, sin, cos),
            RotateAbout(0, height, cx, cy, sin, cos),
            RotateAbout(width, height, cx, cy, sin, cos),
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

        var outW = Math.Max(1, (int)MathF.Ceiling(maxX - minX));
        var outH = Math.Max(1, (int)MathF.Ceiling(maxY - minY));
        return (minX, minY, outW, outH);
    }

    /// <summary>Inverse rotation: sample with −angle so positive degrees are CCW.</summary>
    public static void InverseCosSin(float radians, out double cos, out double sin)
    {
        cos = Math.Cos(-radians);
        sin = Math.Sin(-radians);
    }

    /// <summary>
    /// Maps output pixel (x,y) in the AABB canvas back to continuous source coordinates
    /// (pixel centers at integers when using corner-based dest mapping without +0.5).
    /// </summary>
    public static void MapToSource(
        int x,
        int y,
        float minX,
        float minY,
        double srcCx,
        double srcCy,
        double cos,
        double sin,
        out double sx,
        out double sy
    )
    {
        var wx = x + minX;
        var wy = y + minY;
        var relX = wx - srcCx;
        var relY = wy - srcCy;
        sx = relX * cos - relY * sin + srcCx;
        sy = relX * sin + relY * cos + srcCy;
    }

    /// <summary>
    /// Same as <see cref="MapToSource"/> but samples at dest pixel centers (+0.5),
    /// matching shader UV conventions (pixel edges at integers).
    /// </summary>
    public static void MapToSourceCentered(
        int x,
        int y,
        float minX,
        float minY,
        double srcCx,
        double srcCy,
        double cos,
        double sin,
        out double sx,
        out double sy
    )
    {
        var wx = x + 0.5 + minX;
        var wy = y + 0.5 + minY;
        var relX = wx - srcCx;
        var relY = wy - srcCy;
        sx = relX * cos - relY * sin + srcCx;
        sy = relX * sin + relY * cos + srcCy;
    }

    private static (float X, float Y) RotateAbout(
        float x,
        float y,
        float cx,
        float cy,
        float sin,
        float cos
    )
    {
        var dx = x - cx;
        var dy = y - cy;
        return (dx * cos - dy * sin + cx, dx * sin + dy * cos + cy);
    }

    private static (int Width, int Height, Color[] Pixels) Rotate90(
        Color[] buf,
        int width,
        int height
    )
    {
        var rotated = new Color[buf.Length];
        for (var y = 0; y < height; ++y)
        {
            for (var x = 0; x < width; ++x)
                rotated[x * height + (height - 1 - y)] = buf[y * width + x];
        }

        return (height, width, rotated);
    }

    private static (int Width, int Height, Color[] Pixels) Rotate180(
        Color[] buf,
        int width,
        int height
    )
    {
        var rotated = new Color[buf.Length];
        for (var i = 0; i < buf.Length; ++i)
            rotated[buf.Length - 1 - i] = buf[i];
        return (width, height, rotated);
    }

    private static (int Width, int Height, Color[] Pixels) Rotate270(
        Color[] buf,
        int width,
        int height
    )
    {
        var (w, h, r90) = Rotate90(buf, width, height);
        return Rotate180(r90, w, h);
    }
}
