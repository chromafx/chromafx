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

namespace ChromaFx.Core;

/// <summary>
/// Classical image-quality metrics and diff helpers (RGB channels; alpha ignored).
/// Same-size images only — not an <c>IFilter</c>.
/// </summary>
public static class ImageSimilarity
{
    private const double Peak = 255.0;
    private const int SsimWindow = 8;
    private const double SsimK1 = 0.01;
    private const double SsimK2 = 0.03;

    /// <summary>
    /// Mean squared error over RGB channels (per-channel mean).
    /// </summary>
    public static double Mse(Image a, Image b)
    {
        EnsureSameSize(a, b);
        AccumulateRgb(a, b, out var sumSq, out _, out var channelCount, out _);
        return channelCount == 0 ? 0.0 : sumSq / channelCount;
    }

    /// <summary>
    /// Root mean squared error over RGB channels.
    /// </summary>
    public static double Rmse(Image a, Image b) => Math.Sqrt(Mse(a, b));

    /// <summary>
    /// Mean absolute error over RGB channels.
    /// </summary>
    public static double Mae(Image a, Image b)
    {
        EnsureSameSize(a, b);
        AccumulateRgb(a, b, out _, out var sumAbs, out var channelCount, out _);
        return channelCount == 0 ? 0.0 : sumAbs / channelCount;
    }

    /// <summary>
    /// Peak signal-to-noise ratio for peak 255. Returns <see cref="double.PositiveInfinity"/> when MSE is 0.
    /// </summary>
    public static double Psnr(Image a, Image b)
    {
        var mse = Mse(a, b);
        if (mse <= 0.0)
            return double.PositiveInfinity;
        return 10.0 * Math.Log10((Peak * Peak) / mse);
    }

    /// <summary>
    /// Mean SSIM on Rec.601 luminance over 8×8 windows (stride 1). Identical images → 1.
    /// </summary>
    public static double Ssim(Image a, Image b)
    {
        EnsureSameSize(a, b);
        var w = a.Width;
        var h = a.Height;
        if (w == 0 || h == 0)
            return 1.0;

        var lumaA = BuildLuma(a);
        var lumaB = BuildLuma(b);

        var win = Math.Min(SsimWindow, Math.Min(w, h));
        if (win < 1)
            return 1.0;

        var c1 = (SsimK1 * Peak) * (SsimK1 * Peak);
        var c2 = (SsimK2 * Peak) * (SsimK2 * Peak);

        double sum = 0;
        var windows = 0;
        var maxY = h - win;
        var maxX = w - win;
        for (var y = 0; y <= maxY; y++)
        {
            for (var x = 0; x <= maxX; x++)
            {
                sum += WindowSsim(lumaA, lumaB, w, x, y, win, c1, c2);
                windows++;
            }
        }

        return windows == 0 ? 1.0 : sum / windows;
    }

    /// <summary>
    /// Counts pixels whose RGB differs (alpha ignored).
    /// </summary>
    public static int CountDifferingPixels(Image a, Image b)
    {
        EnsureSameSize(a, b);
        AccumulateRgb(a, b, out _, out _, out _, out var differing);
        return differing;
    }

    /// <summary>
    /// Absolute per-channel RGB difference as a new opaque image (alpha 255).
    /// </summary>
    public static Image CreateDiffImage(Image a, Image b)
    {
        EnsureSameSize(a, b);
        var result = new Image(a.Width, a.Height);
        for (var y = 0; y < a.Height; y++)
        {
            var ra = a.GetRowSpan(y);
            var rb = b.GetRowSpan(y);
            var dest = result.GetRowSpan(y);
            for (var x = 0; x < a.Width; x++)
            {
                var ca = ra[x];
                var cb = rb[x];
                dest[x] = new Color(
                    (byte)Math.Abs(ca.Red - cb.Red),
                    (byte)Math.Abs(ca.Green - cb.Green),
                    (byte)Math.Abs(ca.Blue - cb.Blue),
                    255);
            }
        }

        return result;
    }

    private static void EnsureSameSize(Image a, Image b)
    {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);
        if (a.Width != b.Width || a.Height != b.Height)
            throw new ArgumentException("Images must have the same width and height.");
    }

    private static void AccumulateRgb(
        Image a,
        Image b,
        out double sumSq,
        out double sumAbs,
        out long channelCount,
        out int differingPixels)
    {
        sumSq = 0;
        sumAbs = 0;
        differingPixels = 0;
        channelCount = (long)a.Width * a.Height * 3;
        for (var y = 0; y < a.Height; y++)
        {
            var ra = a.GetRowSpan(y);
            var rb = b.GetRowSpan(y);
            for (var x = 0; x < a.Width; x++)
            {
                var ca = ra[x];
                var cb = rb[x];
                var dr = ca.Red - cb.Red;
                var dg = ca.Green - cb.Green;
                var db = ca.Blue - cb.Blue;
                sumSq += (double)dr * dr + (double)dg * dg + (double)db * db;
                sumAbs += Math.Abs(dr) + Math.Abs(dg) + Math.Abs(db);
                if (dr != 0 || dg != 0 || db != 0)
                    differingPixels++;
            }
        }
    }

    private static float[] BuildLuma(Image image)
    {
        var luma = new float[image.Width * image.Height];
        for (var y = 0; y < image.Height; y++)
        {
            var row = image.GetRowSpan(y);
            var rowBase = y * image.Width;
            for (var x = 0; x < image.Width; x++)
            {
                var c = row[x];
                luma[rowBase + x] = 0.299f * c.Red + 0.587f * c.Green + 0.114f * c.Blue;
            }
        }

        return luma;
    }

    private static double WindowSsim(
        float[] a,
        float[] b,
        int width,
        int originX,
        int originY,
        int win,
        double c1,
        double c2)
    {
        var n = win * win;
        double sumA = 0, sumB = 0, sumAA = 0, sumBB = 0, sumAB = 0;
        for (var dy = 0; dy < win; dy++)
        {
            var row = (originY + dy) * width + originX;
            for (var dx = 0; dx < win; dx++)
            {
                var va = a[row + dx];
                var vb = b[row + dx];
                sumA += va;
                sumB += vb;
                sumAA += va * va;
                sumBB += vb * vb;
                sumAB += va * vb;
            }
        }

        var meanA = sumA / n;
        var meanB = sumB / n;
        var varA = sumAA / n - meanA * meanA;
        var varB = sumBB / n - meanB * meanB;
        var cov = sumAB / n - meanA * meanB;

        var numerator = (2 * meanA * meanB + c1) * (2 * cov + c2);
        var denominator = (meanA * meanA + meanB * meanB + c1) * (varA + varB + c2);
        return denominator <= 0 ? 1.0 : numerator / denominator;
    }
}
