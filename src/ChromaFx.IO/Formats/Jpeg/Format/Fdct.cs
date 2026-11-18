/*
 * Copyright 2017–2020 JaCraig
 * Modifications Copyright 2023–2026 Ho Tzin Mein
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

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace ChromaFx.IO.Formats.Jpeg.Format;

/// <summary>
/// Forward Discrete Cosine Transform
/// </summary>
internal class ForwardDiscreteCosineTransform
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
        ref var d = ref MemoryMarshal.GetArrayDataReference(b.Data);

        // Pass 1: process rows (scalar — each row mixes eight contiguous samples).
        for (var y = 0; y < 8; y++)
        {
            var y8 = y * 8;

            var x0 = Unsafe.Add(ref d, y8 + 0);
            var x1 = Unsafe.Add(ref d, y8 + 1);
            var x2 = Unsafe.Add(ref d, y8 + 2);
            var x3 = Unsafe.Add(ref d, y8 + 3);
            var x4 = Unsafe.Add(ref d, y8 + 4);
            var x5 = Unsafe.Add(ref d, y8 + 5);
            var x6 = Unsafe.Add(ref d, y8 + 6);
            var x7 = Unsafe.Add(ref d, y8 + 7);

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

            Unsafe.Add(ref d, y8) = (tmp10 + tmp11 - 8 * CenterJSample) << Pass1Bits;
            Unsafe.Add(ref d, y8 + 4) = (tmp10 - tmp11) << Pass1Bits;
            var z1 = (tmp12 + tmp13) * Fix0541196100;
            z1 += 1 << (ConstBits - Pass1Bits - 1);
            Unsafe.Add(ref d, y8 + 2) = (z1 + tmp12 * Fix0765366865) >> (ConstBits - Pass1Bits);
            Unsafe.Add(ref d, y8 + 6) = (z1 - tmp13 * Fix1847759065) >> (ConstBits - Pass1Bits);

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
            Unsafe.Add(ref d, y8 + 1) = (tmp0 + tmp10 + tmp12) >> (ConstBits - Pass1Bits);
            Unsafe.Add(ref d, y8 + 3) = (tmp1 + tmp11 + tmp13) >> (ConstBits - Pass1Bits);
            Unsafe.Add(ref d, y8 + 5) = (tmp2 + tmp11 + tmp12) >> (ConstBits - Pass1Bits);
            Unsafe.Add(ref d, y8 + 7) = (tmp3 + tmp10 + tmp13) >> (ConstBits - Pass1Bits);
        }

        // Pass 2: process columns — four at a time when Vector128 is available.
        if (Vector128.IsHardwareAccelerated)
        {
            TransformColumns(ref d, 0);
            TransformColumns(ref d, 4);
            return;
        }

        for (var x = 0; x < 8; x++)
            TransformColumnScalar(ref d, x);
    }

    private static void TransformColumnScalar(ref int d, int x)
    {
        var tmp0 = Unsafe.Add(ref d, x) + Unsafe.Add(ref d, 56 + x);
        var tmp1 = Unsafe.Add(ref d, 8 + x) + Unsafe.Add(ref d, 48 + x);
        var tmp2 = Unsafe.Add(ref d, 16 + x) + Unsafe.Add(ref d, 40 + x);
        var tmp3 = Unsafe.Add(ref d, 24 + x) + Unsafe.Add(ref d, 32 + x);

        var tmp10 = tmp0 + tmp3 + (1 << (Pass1Bits - 1));
        var tmp12 = tmp0 - tmp3;
        var tmp11 = tmp1 + tmp2;
        var tmp13 = tmp1 - tmp2;

        tmp0 = Unsafe.Add(ref d, x) - Unsafe.Add(ref d, 56 + x);
        tmp1 = Unsafe.Add(ref d, 8 + x) - Unsafe.Add(ref d, 48 + x);
        tmp2 = Unsafe.Add(ref d, 16 + x) - Unsafe.Add(ref d, 40 + x);
        tmp3 = Unsafe.Add(ref d, 24 + x) - Unsafe.Add(ref d, 32 + x);

        Unsafe.Add(ref d, x) = (tmp10 + tmp11) >> Pass1Bits;
        Unsafe.Add(ref d, 32 + x) = (tmp10 - tmp11) >> Pass1Bits;

        var z1 = (tmp12 + tmp13) * Fix0541196100;
        z1 += 1 << (ConstBits + Pass1Bits - 1);
        Unsafe.Add(ref d, 16 + x) = (z1 + tmp12 * Fix0765366865) >> (ConstBits + Pass1Bits);
        Unsafe.Add(ref d, 48 + x) = (z1 - tmp13 * Fix1847759065) >> (ConstBits + Pass1Bits);

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
        Unsafe.Add(ref d, 8 + x) = (tmp0 + tmp10 + tmp12) >> (ConstBits + Pass1Bits);
        Unsafe.Add(ref d, 24 + x) = (tmp1 + tmp11 + tmp13) >> (ConstBits + Pass1Bits);
        Unsafe.Add(ref d, 40 + x) = (tmp2 + tmp11 + tmp12) >> (ConstBits + Pass1Bits);
        Unsafe.Add(ref d, 56 + x) = (tmp3 + tmp10 + tmp13) >> (ConstBits + Pass1Bits);
    }

    // Column pass for four columns at once; same integer arithmetic as the scalar loop.
    private static void TransformColumns(ref int d, int x)
    {
        var o = (nuint)x;
        var tmp0 = Vector128.LoadUnsafe(ref d, o) + Vector128.LoadUnsafe(ref d, o + 56);
        var tmp1 = Vector128.LoadUnsafe(ref d, o + 8) + Vector128.LoadUnsafe(ref d, o + 48);
        var tmp2 = Vector128.LoadUnsafe(ref d, o + 16) + Vector128.LoadUnsafe(ref d, o + 40);
        var tmp3 = Vector128.LoadUnsafe(ref d, o + 24) + Vector128.LoadUnsafe(ref d, o + 32);

        var pass1Round = Vector128.Create(1 << (Pass1Bits - 1));
        var tmp10 = tmp0 + tmp3 + pass1Round;
        var tmp12 = tmp0 - tmp3;
        var tmp11 = tmp1 + tmp2;
        var tmp13 = tmp1 - tmp2;

        tmp0 = Vector128.LoadUnsafe(ref d, o) - Vector128.LoadUnsafe(ref d, o + 56);
        tmp1 = Vector128.LoadUnsafe(ref d, o + 8) - Vector128.LoadUnsafe(ref d, o + 48);
        tmp2 = Vector128.LoadUnsafe(ref d, o + 16) - Vector128.LoadUnsafe(ref d, o + 40);
        tmp3 = Vector128.LoadUnsafe(ref d, o + 24) - Vector128.LoadUnsafe(ref d, o + 32);

        ((tmp10 + tmp11) >> Pass1Bits).StoreUnsafe(ref d, o);
        ((tmp10 - tmp11) >> Pass1Bits).StoreUnsafe(ref d, o + 32);

        var constRound = Vector128.Create(1 << (ConstBits + Pass1Bits - 1));
        var z1 = (tmp12 + tmp13) * Fix0541196100 + constRound;
        ((z1 + tmp12 * Fix0765366865) >> (ConstBits + Pass1Bits)).StoreUnsafe(ref d, o + 16);
        ((z1 - tmp13 * Fix1847759065) >> (ConstBits + Pass1Bits)).StoreUnsafe(ref d, o + 48);

        tmp10 = tmp0 + tmp3;
        tmp11 = tmp1 + tmp2;
        tmp12 = tmp0 + tmp2;
        tmp13 = tmp1 + tmp3;
        z1 = (tmp12 + tmp13) * Fix1175875602 + constRound;
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
        ((tmp0 + tmp10 + tmp12) >> (ConstBits + Pass1Bits)).StoreUnsafe(ref d, o + 8);
        ((tmp1 + tmp11 + tmp13) >> (ConstBits + Pass1Bits)).StoreUnsafe(ref d, o + 24);
        ((tmp2 + tmp11 + tmp12) >> (ConstBits + Pass1Bits)).StoreUnsafe(ref d, o + 40);
        ((tmp3 + tmp10 + tmp13) >> (ConstBits + Pass1Bits)).StoreUnsafe(ref d, o + 56);
    }
}
