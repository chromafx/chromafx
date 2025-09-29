/*
 * Copyright 2017–2020 JaCraig
 * Modifications Copyright 2023–2025 Ho Tzin Mein
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

namespace ChromaFx.IO.Formats.Jpeg.Format.HelperClasses;

/// <summary>
/// Inverse discrete cosine transform
/// </summary>
public static class Idct
{
    private const int R2 = 181;
    private const int W1 = 2841;
    private const int W1Mw7 = 2276;
    private const int W1Pw7 = 3406;
    private const int W2 = 2676;
    private const int W2Mw6 = 1568;
    private const int W2Pw6 = 3784;
    private const int W3 = 2408;
    private const int W3Mw5 = 799;
    private const int W3Pw5 = 4017;
    private const int W5 = 1609;
    private const int W6 = 1108;
    private const int W7 = 565;

    /// <summary>
    /// Transforms the specified source.
    /// </summary>
    /// <param name="src">The source.</param>
    public static void Transform(Block src)
    {
        for (var y = 0; y < 8; y++)
        {
            var y8 = y * 8;

            if (src[y8 + 1] == 0 && src[y8 + 2] == 0 && src[y8 + 3] == 0 &&
                src[y8 + 4] == 0 && src[y8 + 5] == 0 && src[y8 + 6] == 0 && src[y8 + 7] == 0)
            {
                var dc = src[y8 + 0] << 3;
                src[y8 + 0] = dc;
                src[y8 + 1] = dc;
                src[y8 + 2] = dc;
                src[y8 + 3] = dc;
                src[y8 + 4] = dc;
                src[y8 + 5] = dc;
                src[y8 + 6] = dc;
                src[y8 + 7] = dc;
                continue;
            }

            var x0 = (src[y8 + 0] << 11) + 128;
            var x1 = src[y8 + 4] << 11;
            var x2 = src[y8 + 6];
            var x3 = src[y8 + 2];
            var x4 = src[y8 + 1];
            var x5 = src[y8 + 7];
            var x6 = src[y8 + 5];
            var x7 = src[y8 + 3];

            var x8 = W7 * (x4 + x5);
            x4 = x8 + W1Mw7 * x4;
            x5 = x8 - W1Pw7 * x5;
            x8 = W3 * (x6 + x7);
            x6 = x8 - W3Mw5 * x6;
            x7 = x8 - W3Pw5 * x7;

            x8 = x0 + x1;
            x0 -= x1;
            x1 = W6 * (x3 + x2);
            x2 = x1 - W2Pw6 * x2;
            x3 = x1 + W2Mw6 * x3;
            x1 = x4 + x6;
            x4 -= x6;
            x6 = x5 + x7;
            x5 -= x7;

            x7 = x8 + x3;
            x8 -= x3;
            x3 = x0 + x2;
            x0 -= x2;
            x2 = (R2 * (x4 + x5) + 128) >> 8;
            x4 = (R2 * (x4 - x5) + 128) >> 8;

            src[y8 + 0] = (x7 + x1) >> 8;
            src[y8 + 1] = (x3 + x2) >> 8;
            src[y8 + 2] = (x0 + x4) >> 8;
            src[y8 + 3] = (x8 + x6) >> 8;
            src[y8 + 4] = (x8 - x6) >> 8;
            src[y8 + 5] = (x0 - x4) >> 8;
            src[y8 + 6] = (x3 - x2) >> 8;
            src[y8 + 7] = (x7 - x1) >> 8;
        }

        for (var x = 0; x < 8; x++)
        {
            var y0 = (src[x] << 8) + 8192;
            var y1 = src[32 + x] << 8;
            var y2 = src[48 + x];
            var y3 = src[16 + x];
            var y4 = src[8 + x];
            var y5 = src[56 + x];
            var y6 = src[40 + x];
            var y7 = src[24 + x];

            var y8 = W7 * (y4 + y5) + 4;
            y4 = (y8 + W1Mw7 * y4) >> 3;
            y5 = (y8 - W1Pw7 * y5) >> 3;
            y8 = W3 * (y6 + y7) + 4;
            y6 = (y8 - W3Mw5 * y6) >> 3;
            y7 = (y8 - W3Pw5 * y7) >> 3;

            y8 = y0 + y1;
            y0 -= y1;
            y1 = W6 * (y3 + y2) + 4;
            y2 = (y1 - W2Pw6 * y2) >> 3;
            y3 = (y1 + W2Mw6 * y3) >> 3;
            y1 = y4 + y6;
            y4 -= y6;
            y6 = y5 + y7;
            y5 -= y7;

            y7 = y8 + y3;
            y8 -= y3;
            y3 = y0 + y2;
            y0 -= y2;
            y2 = (R2 * (y4 + y5) + 128) >> 8;
            y4 = (R2 * (y4 - y5) + 128) >> 8;

            src[x] = (y7 + y1) >> 14;
            src[8 + x] = (y3 + y2) >> 14;
            src[16 + x] = (y0 + y4) >> 14;
            src[24 + x] = (y8 + y6) >> 14;
            src[32 + x] = (y8 - y6) >> 14;
            src[40 + x] = (y0 - y4) >> 14;
            src[48 + x] = (y3 - y2) >> 14;
            src[56 + x] = (y7 - y1) >> 14;
        }
    }
}