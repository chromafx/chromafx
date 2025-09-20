/*
Copyright 2025 Ho Tzin Mein

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

namespace ChromaFx.Formats.Jpeg.Format.HelperClasses;

/// <summary>
/// Forward Discrete Cosine Transform
/// </summary>
public class ForwardDiscreteCosineTransform
{
    private const int CenterJSample = 128;

    private const int ConstBits = 13;

    private const int Fix0298631336 = 2446;

    private const int Fix0390180644 = 3196;
    private const int Fix0541196100 = 4433;
    private const int Fix0765366865 = 6270;
    private const int Fix0899976223 = 7373;
    private const int Fix1175875602 = 9633;
    private const int Fix1501321110 = 12299;
    private const int Fix1847759065 = 15137;
    private const int Fix1961570560 = 16069;
    private const int Fix2053119869 = 16819;
    private const int Fix2562915447 = 20995;
    private const int Fix3072711026 = 25172;
    private const int Pass1Bits = 2;

    /// <summary>
    /// Transforms the specified block using a forward discrete cosine transform.
    /// </summary>
    /// <param name="b">The block</param>
    public static void Transform(Block b)
    {
        // Pass 1: process rows.
        for (var y = 0; y < 8; y++)
        {
            var y8 = y * 8;

            var x0 = b[y8 + 0];
            var x1 = b[y8 + 1];
            var x2 = b[y8 + 2];
            var x3 = b[y8 + 3];
            var x4 = b[y8 + 4];
            var x5 = b[y8 + 5];
            var x6 = b[y8 + 6];
            var x7 = b[y8 + 7];

            var tmp0 = x0 + x7;
            var tmp1 = x1 + x6;
            var tmp2 = x2 + x5;
            var tmp3 = x3 + x4;

            var tmp10 = tmp0 + tmp3;
            var tmp12 = tmp0 - tmp3;
            var tmp11 = tmp1 + tmp2;
            var tmp13 = tmp1 - tmp2;

            tmp0 = x0 - x7;
            tmp1 = x1 - x6;
            tmp2 = x2 - x5;
            tmp3 = x3 - x4;

            b[y8] = (tmp10 + tmp11 - 8 * CenterJSample) << Pass1Bits;
            b[y8 + 4] = (tmp10 - tmp11) << Pass1Bits;
            var z1 = (tmp12 + tmp13) * Fix0541196100;
            z1 += 1 << (ConstBits - Pass1Bits - 1);
            b[y8 + 2] = (z1 + tmp12 * Fix0765366865) >> (ConstBits - Pass1Bits);
            b[y8 + 6] = (z1 - tmp13 * Fix1847759065) >> (ConstBits - Pass1Bits);

            tmp10 = tmp0 + tmp3;
            tmp11 = tmp1 + tmp2;
            tmp12 = tmp0 + tmp2;
            tmp13 = tmp1 + tmp3;
            z1 = (tmp12 + tmp13) * Fix1175875602;
            z1 += 1 << (ConstBits - Pass1Bits - 1);
            tmp0 *= Fix1501321110;
            tmp1 *= Fix3072711026;
            tmp2 *= Fix2053119869;
            tmp3 *= Fix0298631336;
            tmp10 *= -Fix0899976223;
            tmp11 *= -Fix2562915447;
            tmp12 *= -Fix0390180644;
            tmp13 *= -Fix1961570560;

            tmp12 += z1;
            tmp13 += z1;
            b[y8 + 1] = (tmp0 + tmp10 + tmp12) >> (ConstBits - Pass1Bits);
            b[y8 + 3] = (tmp1 + tmp11 + tmp13) >> (ConstBits - Pass1Bits);
            b[y8 + 5] = (tmp2 + tmp11 + tmp12) >> (ConstBits - Pass1Bits);
            b[y8 + 7] = (tmp3 + tmp10 + tmp13) >> (ConstBits - Pass1Bits);
        }

        // Pass 2: process columns.
        // We remove pass1Bits scaling, but leave results scaled up by an overall factor of 8.
        for (var x = 0; x < 8; x++)
        {
            var tmp0 = b[x] + b[56 + x];
            var tmp1 = b[8 + x] + b[48 + x];
            var tmp2 = b[16 + x] + b[40 + x];
            var tmp3 = b[24 + x] + b[32 + x];

            var tmp10 = tmp0 + tmp3 + (1 << (Pass1Bits - 1));
            var tmp12 = tmp0 - tmp3;
            var tmp11 = tmp1 + tmp2;
            var tmp13 = tmp1 - tmp2;

            tmp0 = b[x] - b[56 + x];
            tmp1 = b[8 + x] - b[48 + x];
            tmp2 = b[16 + x] - b[40 + x];
            tmp3 = b[24 + x] - b[32 + x];

            b[x] = (tmp10 + tmp11) >> Pass1Bits;
            b[32 + x] = (tmp10 - tmp11) >> Pass1Bits;

            var z1 = (tmp12 + tmp13) * Fix0541196100;
            z1 += 1 << (ConstBits + Pass1Bits - 1);
            b[16 + x] = (z1 + tmp12 * Fix0765366865) >> (ConstBits + Pass1Bits);
            b[48 + x] = (z1 - tmp13 * Fix1847759065) >> (ConstBits + Pass1Bits);

            tmp10 = tmp0 + tmp3;
            tmp11 = tmp1 + tmp2;
            tmp12 = tmp0 + tmp2;
            tmp13 = tmp1 + tmp3;
            z1 = (tmp12 + tmp13) * Fix1175875602;
            z1 += 1 << (ConstBits + Pass1Bits - 1);
            tmp0 *= Fix1501321110;
            tmp1 *= Fix3072711026;
            tmp2 *= Fix2053119869;
            tmp3 *= Fix0298631336;
            tmp10 *= -Fix0899976223;
            tmp11 *= -Fix2562915447;
            tmp12 *= -Fix0390180644;
            tmp13 *= -Fix1961570560;

            tmp12 += z1;
            tmp13 += z1;
            b[8 + x] = (tmp0 + tmp10 + tmp12) >> (ConstBits + Pass1Bits);
            b[24 + x] = (tmp1 + tmp11 + tmp13) >> (ConstBits + Pass1Bits);
            b[40 + x] = (tmp2 + tmp11 + tmp12) >> (ConstBits + Pass1Bits);
            b[56 + x] = (tmp3 + tmp10 + tmp13) >> (ConstBits + Pass1Bits);
        }
    }
}