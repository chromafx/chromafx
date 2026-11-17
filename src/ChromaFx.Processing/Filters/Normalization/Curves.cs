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
using ChromaFx.Processing.Numerics;
using ChromaFx.Processing.Simd;

namespace ChromaFx.Processing.Filters.Normalization;

/// <summary>
/// Master RGB tone curve via piecewise-linear control points (0–1). Alpha is preserved.
/// The three-parameter ctor uses points (0, black), (0.5, mid), (1, white).
/// </summary>
public sealed class Curves : IFilter
{
    private readonly byte[] _lut = new byte[256];

    /// <summary>
    /// Three-point master curve: black / mid / white output levels (0–1).
    /// </summary>
    public Curves(float black = 0f, float mid = 0.5f, float white = 1f)
        : this([(0f, Math.Clamp(black, 0f, 1f)), (0.5f, Math.Clamp(mid, 0f, 1f)), (1f, Math.Clamp(white, 0f, 1f))])
    {
        Black = Math.Clamp(black, 0f, 1f);
        Mid = Math.Clamp(mid, 0f, 1f);
        White = Math.Clamp(white, 0f, 1f);
    }

    /// <summary>
    /// Builds a curve from sorted-or-unsorted control points in 0–1 (X is clamped and sorted).
    /// Endpoints at X=0 and X=1 are added when missing.
    /// </summary>
    public Curves(IReadOnlyList<(float X, float Y)> points)
    {
        ArgumentNullException.ThrowIfNull(points);
        var pts = NormalizePoints(points);
        for (var i = 0; i < 256; i++)
        {
            var x = i / 255f;
            _lut[i] = (byte)Math.Clamp((int)(Eval(pts, x) * 255f + 0.5f), 0, 255);
        }

        Black = pts[0].Y;
        Mid = Eval(pts, 0.5f);
        White = pts[^1].Y;
    }

    /// <summary>Gets the black output level (or Y at X=0 for custom points).</summary>
    public float Black { get; }

    /// <summary>Gets the mid output level (or evaluated Y at X=0.5).</summary>
    public float Mid { get; }

    /// <summary>Gets the white output level (or Y at X=1 for custom points).</summary>
    public float White { get; }

    /// <inheritdoc />
    public Image Apply(Image image, Rectangle targetLocation = default)
    {
        targetLocation = targetLocation.Normalize(image);
        int left = targetLocation.Left;
        int rowWidth = targetLocation.Right - left;
        var lut = _lut;

        FilterParallelism.ForRows(
            targetLocation,
            y =>
            {
                PixelSimd.ApplyRgbLut(image.GetRowSpan(y).Slice(left, rowWidth), lut);
            }
        );

        return image;
    }

    private static List<(float X, float Y)> NormalizePoints(IReadOnlyList<(float X, float Y)> points)
    {
        var list = new List<(float X, float Y)>(points.Count + 2);
        foreach (var (x, y) in points)
            list.Add((Math.Clamp(x, 0f, 1f), Math.Clamp(y, 0f, 1f)));

        list.Sort((a, b) => a.X.CompareTo(b.X));

        // Deduplicate X (keep last)
        for (var i = list.Count - 1; i > 0; i--)
        {
            if (Math.Abs(list[i].X - list[i - 1].X) < 1e-6f)
                list.RemoveAt(i - 1);
        }

        if (list.Count == 0 || list[0].X > 0f)
            list.Insert(0, (0f, 0f));
        if (list[^1].X < 1f)
            list.Add((1f, 1f));

        return list;
    }

    private static float Eval(IReadOnlyList<(float X, float Y)> pts, float x)
    {
        if (x <= pts[0].X)
            return pts[0].Y;
        if (x >= pts[^1].X)
            return pts[^1].Y;

        for (var i = 1; i < pts.Count; i++)
        {
            if (x > pts[i].X)
                continue;
            var (x0, y0) = pts[i - 1];
            var (x1, y1) = pts[i];
            var t = (x - x0) / (x1 - x0);
            return y0 + (y1 - y0) * t;
        }

        return pts[^1].Y;
    }
}
