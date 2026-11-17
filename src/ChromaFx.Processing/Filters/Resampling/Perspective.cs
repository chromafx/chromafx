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
using ChromaFx.Processing.Numerics;

namespace ChromaFx.Processing.Filters.Resampling;

/// <summary>
/// Perspective-style trapezoid warp. Positive <paramref name="horizontal"/> insets the top edge;
/// positive <paramref name="vertical"/> insets the left edge (fractions of width/height, ±0.45).
/// Alpha outside the source is cleared. Uses bilinear sampling.
/// </summary>
public sealed class Perspective(float horizontal = 0f, float vertical = 0f) : IFilter
{
    /// <summary>Gets the horizontal perspective amount.</summary>
    public float Horizontal { get; } = Math.Clamp(horizontal, -0.45f, 0.45f);

    /// <summary>Gets the vertical perspective amount.</summary>
    public float Vertical { get; } = Math.Clamp(vertical, -0.45f, 0.45f);

    /// <inheritdoc />
    public Image Apply(Image image, Rectangle targetLocation = default)
    {
        targetLocation = targetLocation.Normalize(image);
        var w = image.Width;
        var h = image.Height;
        if (w < 2 || h < 2 || (Horizontal == 0f && Vertical == 0f))
            return image;

        // Destination rectangle corners → source trapezoid (forward), then invert for sampling.
        var hx = Horizontal * w;
        var vy = Vertical * h;
        // dest corners (TL, TR, BR, BL) in image space
        Span<(float X, float Y)> dest =
        [
            (targetLocation.Left, targetLocation.Top),
            (targetLocation.Right - 1, targetLocation.Top),
            (targetLocation.Right - 1, targetLocation.Bottom - 1),
            (targetLocation.Left, targetLocation.Bottom - 1),
        ];
        // source sampling corners: pinch top by hx, left by vy
        Span<(float X, float Y)> src =
        [
            (targetLocation.Left + Math.Max(0, hx), targetLocation.Top + Math.Max(0, vy)),
            (targetLocation.Right - 1 - Math.Max(0, hx), targetLocation.Top + Math.Max(0, -vy)),
            (
                targetLocation.Right - 1 - Math.Max(0, -hx),
                targetLocation.Bottom - 1 - Math.Max(0, -vy)
            ),
            (targetLocation.Left + Math.Max(0, -hx), targetLocation.Bottom - 1 - Math.Max(0, vy)),
        ];

        // Homography maps dest → src for inverse sampling (stack; no per-Apply heap matrix).
        Span<float> hMat = stackalloc float[9];
        Homography.FromQuads(dest, src, hMat);
        // Copy for the parallel closure (stackalloc cannot be captured).
        var h0 = hMat[0];
        var h1 = hMat[1];
        var h2 = hMat[2];
        var h3 = hMat[3];
        var h4 = hMat[4];
        var h5 = hMat[5];
        var h6 = hMat[6];
        var h7 = hMat[7];
        var h8 = hMat[8];

        var copy = FilterBufferPool.RentCopy(image.PixelSpan);
        var stride = image.Stride;
        try
        {
            FilterParallelism.ForRows(
                targetLocation,
                y =>
                {
                    var destRow = image.GetRowSpan(y);
                    for (var x = targetLocation.Left; x < targetLocation.Right; x++)
                    {
                        var ww = h6 * x + h7 * y + h8;
                        float sx;
                        float sy;
                        if (Math.Abs(ww) < 1e-8f)
                        {
                            sx = -1;
                            sy = -1;
                        }
                        else
                        {
                            sx = (h0 * x + h1 * y + h2) / ww;
                            sy = (h3 * x + h4 * y + h5) / ww;
                        }

                        destRow[x] = SampleBilinear(copy, stride, w, h, sx, sy);
                    }
                }
            );
        }
        finally
        {
            FilterBufferPool.Return(copy);
        }

        return image;
    }

    private static Color SampleBilinear(
        Color[] src,
        int stride,
        int width,
        int height,
        float x,
        float y
    )
    {
        if (x < -0.5f || y < -0.5f || x >= width - 0.5f || y >= height - 0.5f)
            return default;

        var x0 = (int)MathF.Floor(x);
        var y0 = (int)MathF.Floor(y);
        var x1 = x0 + 1;
        var y1 = y0 + 1;
        var fx = x - x0;
        var fy = y - y0;

        var c00 = Peek(src, stride, width, height, x0, y0);
        var c10 = Peek(src, stride, width, height, x1, y0);
        var c01 = Peek(src, stride, width, height, x0, y1);
        var c11 = Peek(src, stride, width, height, x1, y1);

        return Lerp(Lerp(c00, c10, fx), Lerp(c01, c11, fx), fy);
    }

    private static Color Peek(Color[] src, int stride, int width, int height, int x, int y)
    {
        if ((uint)x >= (uint)width || (uint)y >= (uint)height)
            return default;
        return src[y * stride + x];
    }

    private static Color Lerp(Color a, Color b, float t) =>
        new(
            (byte)(a.Red + (b.Red - a.Red) * t + 0.5f),
            (byte)(a.Green + (b.Green - a.Green) * t + 0.5f),
            (byte)(a.Blue + (b.Blue - a.Blue) * t + 0.5f),
            (byte)(a.Alpha + (b.Alpha - a.Alpha) * t + 0.5f)
        );
}

/// <summary>
/// 3×3 homography helpers for perspective sampling.
/// </summary>
file static class Homography
{
    public static void FromQuads(
        ReadOnlySpan<(float X, float Y)> dest,
        ReadOnlySpan<(float X, float Y)> src,
        Span<float> hOut
    )
    {
        // Solve for H mapping dest→src using DLT (8 equations, h33=1).
        Span<float> a = stackalloc float[8 * 8];
        Span<float> b = stackalloc float[8];
        for (var i = 0; i < 4; i++)
        {
            var (x, y) = dest[i];
            var (xp, yp) = src[i];
            var r0 = i * 2 * 8;
            var r1 = (i * 2 + 1) * 8;
            a[r0 + 0] = x;
            a[r0 + 1] = y;
            a[r0 + 2] = 1;
            a[r0 + 6] = -x * xp;
            a[r0 + 7] = -y * xp;
            b[i * 2] = xp;

            a[r1 + 3] = x;
            a[r1 + 4] = y;
            a[r1 + 5] = 1;
            a[r1 + 6] = -x * yp;
            a[r1 + 7] = -y * yp;
            b[i * 2 + 1] = yp;
        }

        Span<float> h8 = stackalloc float[8];
        Solve8(a, b, h8);
        hOut[0] = h8[0];
        hOut[1] = h8[1];
        hOut[2] = h8[2];
        hOut[3] = h8[3];
        hOut[4] = h8[4];
        hOut[5] = h8[5];
        hOut[6] = h8[6];
        hOut[7] = h8[7];
        hOut[8] = 1f;
    }

    private static void Solve8(Span<float> a, Span<float> b, Span<float> x)
    {
        // Gaussian elimination with partial pivoting (8×8).
        const int n = 8;
        for (var col = 0; col < n; col++)
        {
            var pivot = col;
            var best = Math.Abs(a[col * n + col]);
            for (var row = col + 1; row < n; row++)
            {
                var v = Math.Abs(a[row * n + col]);
                if (v > best)
                {
                    best = v;
                    pivot = row;
                }
            }

            if (pivot != col)
            {
                for (var k = 0; k < n; k++)
                    (a[col * n + k], a[pivot * n + k]) = (a[pivot * n + k], a[col * n + k]);
                (b[col], b[pivot]) = (b[pivot], b[col]);
            }

            var diag = a[col * n + col];
            if (Math.Abs(diag) < 1e-12f)
                continue;

            for (var row = col + 1; row < n; row++)
            {
                var f = a[row * n + col] / diag;
                b[row] -= f * b[col];
                for (var k = col; k < n; k++)
                    a[row * n + k] -= f * a[col * n + k];
            }
        }

        for (var i = n - 1; i >= 0; i--)
        {
            var sum = b[i];
            for (var k = i + 1; k < n; k++)
                sum -= a[i * n + k] * x[k];
            var diag = a[i * n + i];
            x[i] = Math.Abs(diag) < 1e-12f ? 0 : sum / diag;
        }
    }
}
