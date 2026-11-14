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
using ChromaFx.Processing.Filters.ColorMatrix.BaseClasses;
using ChromaFx.Processing.Filters.Interfaces;
using ChromaFx.Processing.Numerics;

namespace ChromaFx.Processing.Filters.ColorMatrix;

/// <summary>
/// Estimates per-channel gains from the image (Gray World or White Patch) and
/// applies them as a diagonal color matrix.
/// </summary>
public sealed class AutoWhiteBalance : IFilter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AutoWhiteBalance"/> class.
    /// </summary>
    /// <param name="method">Estimator to use.</param>
    /// <param name="whitePatchPercentile">
    /// For <see cref="AutoWhiteBalanceMethod.WhitePatch"/>, luminance percentile
    /// (0…100) used to pick the reference white (default 99).
    /// </param>
    public AutoWhiteBalance(
        AutoWhiteBalanceMethod method = AutoWhiteBalanceMethod.GrayWorld,
        float whitePatchPercentile = 99f
    )
    {
        Method = method;
        WhitePatchPercentile = Math.Clamp(whitePatchPercentile, 0f, 100f);
    }

    /// <summary>
    /// Gets the estimator method.
    /// </summary>
    public AutoWhiteBalanceMethod Method { get; }

    /// <summary>
    /// Gets the white-patch luminance percentile.
    /// </summary>
    public float WhitePatchPercentile { get; }

    /// <inheritdoc />
    public Image Apply(Image image, Rectangle targetLocation = default)
    {
        ArgumentNullException.ThrowIfNull(image);
        targetLocation = targetLocation.Normalize(image);
        if (image.Width <= 0 || image.Height <= 0)
            return image;

        var (gainR, gainG, gainB) = Method switch
        {
            AutoWhiteBalanceMethod.WhitePatch => EstimateWhitePatch(image, targetLocation),
            _ => EstimateGrayWorld(image, targetLocation),
        };

        var matrix = new ColorMatrix(
            new Matrix5X5(
                gainR,
                0,
                0,
                0,
                0,
                0,
                gainG,
                0,
                0,
                0,
                0,
                0,
                gainB,
                0,
                0,
                0,
                0,
                0,
                1,
                0,
                0,
                0,
                0,
                0,
                1
            )
        );
        return matrix.Apply(image, targetLocation);
    }

    private static (float R, float G, float B) EstimateGrayWorld(Image image, Rectangle bounds)
    {
        double sumR = 0,
            sumG = 0,
            sumB = 0;
        var count = 0;
        // Rectangle uses Bottom..Top (y increases downward; Bottom is the smaller y).
        for (var y = bounds.Bottom; y < bounds.Top; y++)
        {
            var row = image.GetRowSpan(y);
            for (var x = bounds.Left; x < bounds.Right; x++)
            {
                var c = row[x];
                if (c.Alpha == 0)
                    continue;
                sumR += c.Red;
                sumG += c.Green;
                sumB += c.Blue;
                count++;
            }
        }

        if (count == 0)
            return (1f, 1f, 1f);

        var avgR = sumR / count;
        var avgG = sumG / count;
        var avgB = sumB / count;
        var gray = (avgR + avgG + avgB) / 3.0;
        if (gray <= 1e-6)
            return (1f, 1f, 1f);

        return NormalizeGains((float)(gray / avgR), (float)(gray / avgG), (float)(gray / avgB));
    }

    private (float R, float G, float B) EstimateWhitePatch(Image image, Rectangle bounds)
    {
        // Collect luminance samples for percentile, then average RGB among bright pixels.
        var luminances = new List<byte>(Math.Max(64, bounds.Width * bounds.Height / 4));
        for (var y = bounds.Bottom; y < bounds.Top; y++)
        {
            var row = image.GetRowSpan(y);
            for (var x = bounds.Left; x < bounds.Right; x++)
            {
                var c = row[x];
                if (c.Alpha == 0)
                    continue;
                luminances.Add((byte)((77 * c.Red + 150 * c.Green + 29 * c.Blue) >> 8));
            }
        }

        if (luminances.Count == 0)
            return (1f, 1f, 1f);

        luminances.Sort();
        var index = Math.Clamp(
            (int)(luminances.Count * (WhitePatchPercentile / 100f)),
            0,
            luminances.Count - 1
        );
        var threshold = luminances[index];

        double sumR = 0,
            sumG = 0,
            sumB = 0;
        var count = 0;
        for (var y = bounds.Bottom; y < bounds.Top; y++)
        {
            var row = image.GetRowSpan(y);
            for (var x = bounds.Left; x < bounds.Right; x++)
            {
                var c = row[x];
                if (c.Alpha == 0)
                    continue;
                var lum = (byte)((77 * c.Red + 150 * c.Green + 29 * c.Blue) >> 8);
                if (lum < threshold)
                    continue;
                sumR += c.Red;
                sumG += c.Green;
                sumB += c.Blue;
                count++;
            }
        }

        if (count == 0)
            return (1f, 1f, 1f);

        var avgR = sumR / count;
        var avgG = sumG / count;
        var avgB = sumB / count;
        // Scale so the brightest channel average maps to 255.
        var max = Math.Max(avgR, Math.Max(avgG, avgB));
        if (max <= 1e-6)
            return (1f, 1f, 1f);

        return NormalizeGains((float)(255.0 / avgR), (float)(255.0 / avgG), (float)(255.0 / avgB));
    }

    private static (float R, float G, float B) NormalizeGains(float r, float g, float b)
    {
        // Avoid exploding near-zero channels; cap extreme gains.
        r = float.IsFinite(r) ? Math.Clamp(r, 0.05f, 8f) : 1f;
        g = float.IsFinite(g) ? Math.Clamp(g, 0.05f, 8f) : 1f;
        b = float.IsFinite(b) ? Math.Clamp(b, 0.05f, 8f) : 1f;
        var max = Math.Max(r, Math.Max(g, b));
        if (max > 1f)
        {
            r /= max;
            g /= max;
            b /= max;
        }

        return (r, g, b);
    }
}
