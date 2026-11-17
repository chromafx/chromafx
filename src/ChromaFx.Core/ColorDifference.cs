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
/// CIE color-difference helpers (Lab). Companion to <see cref="ImageSimilarity"/>.
/// </summary>
public static class ColorDifference
{
    // Matches Xyz / CieLab conversion (D65, sRGB); LUT avoids Math.Pow per channel.
    private const double Kappa = 903.3;
    private const double XyzEpsilon = 0.008856;
    private const double WhiteX = 95.047;
    private const double WhiteY = 100.0;
    private const double WhiteZ = 108.883;

    private static readonly double[] SrgbToLinear = BuildSrgbLut();

    /// <summary>
    /// CIE76 ΔE — Euclidean distance in CIELAB. Identical colors → 0.
    /// </summary>
    public static double Cie76(Color a, Color b)
    {
        ToLab(a, out var l0, out var a0, out var b0);
        ToLab(b, out var l1, out var a1, out var b1);
        var dl = l0 - l1;
        var da = a0 - a1;
        var db = b0 - b1;
        return Math.Sqrt(dl * dl + da * da + db * db);
    }

    /// <summary>
    /// CIE76 ΔE between two Lab colors.
    /// </summary>
    public static double Cie76(CieLab a, CieLab b)
    {
        var dl = a.L - b.L;
        var da = a.A - b.A;
        var db = a.B - b.B;
        return Math.Sqrt(dl * dl + da * da + db * db);
    }

    /// <summary>
    /// Mean CIE76 ΔE over RGB pixels (alpha ignored). Same-size images only.
    /// </summary>
    public static double MeanCie76(Image a, Image b)
    {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);
        if (a.Width != b.Width || a.Height != b.Height)
            throw new ArgumentException("Images must have the same width and height.");

        var pixels = (long)a.Width * a.Height;
        if (pixels == 0)
            return 0.0;

        double sum = 0;
        for (var y = 0; y < a.Height; y++)
        {
            var ra = a.GetRowSpan(y);
            var rb = b.GetRowSpan(y);
            for (var x = 0; x < a.Width; x++)
                sum += Cie76(ra[x], rb[x]);
        }

        return sum / pixels;
    }

    private static void ToLab(Color color, out double l, out double a, out double b)
    {
        var r = SrgbToLinear[color.Red];
        var g = SrgbToLinear[color.Green];
        var bl = SrgbToLinear[color.Blue];
        var x = r * 0.4124 + g * 0.3576 + bl * 0.1805;
        var y = r * 0.2126 + g * 0.7152 + bl * 0.0722;
        var z = r * 0.0193 + g * 0.1192 + bl * 0.9505;

        x = PivotXyz(x / WhiteX);
        y = PivotXyz(y / WhiteY);
        z = PivotXyz(z / WhiteZ);

        l = Math.Clamp(Math.Max(0, 116 * y - 16), 0, 100);
        a = Math.Clamp(500 * (x - y), -100, 100);
        b = Math.Clamp(200 * (y - z), -100, 100);
    }

    private static double PivotXyz(double n) =>
        n > XyzEpsilon ? n.CubicRoot() : (Kappa * n + 16) / 116;

    private static double[] BuildSrgbLut()
    {
        var lut = new double[256];
        for (var i = 0; i < 256; i++)
        {
            var n = i / 255.0;
            lut[i] = (n > 0.04045 ? Math.Pow((n + 0.055) / 1.055, 2.4) : n / 12.92) * 100.0;
        }

        return lut;
    }
}
