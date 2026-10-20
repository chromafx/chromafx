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

using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;

namespace ChromaFx.Core.Simd;

/// <summary>
/// Vectorized byte-span helpers (wrapping and saturating arithmetic).
/// </summary>
internal static class ByteSimd
{
    /// <summary>
    /// Wrapping byte subtract: <c>dst[i] = left[i] - right[i]</c>.
    /// </summary>
    internal static void SubtractWrapping(
        ReadOnlySpan<byte> left,
        ReadOnlySpan<byte> right,
        Span<byte> destination
    )
    {
        int length = left.Length;
        int i = 0;
        int simdLength = Vector<byte>.Count;

        if (Vector.IsHardwareAccelerated)
        {
            for (; i <= length - simdLength; i += simdLength)
            {
                var vLeft = new Vector<byte>(left.Slice(i));
                var vRight = new Vector<byte>(right.Slice(i));
                (vLeft - vRight).CopyTo(destination.Slice(i));
            }
        }

        for (; i < length; i++)
            destination[i] = (byte)(left[i] - right[i]);
    }

    /// <summary>
    /// Wrapping byte add: <c>dst[i] = left[i] + right[i]</c>.
    /// </summary>
    internal static void AddWrapping(
        ReadOnlySpan<byte> left,
        ReadOnlySpan<byte> right,
        Span<byte> destination
    )
    {
        int length = left.Length;
        int i = 0;
        int simdLength = Vector<byte>.Count;

        if (Vector.IsHardwareAccelerated)
        {
            for (; i <= length - simdLength; i += simdLength)
            {
                var vLeft = new Vector<byte>(left.Slice(i));
                var vRight = new Vector<byte>(right.Slice(i));
                (vLeft + vRight).CopyTo(destination.Slice(i));
            }
        }

        for (; i < length; i++)
            destination[i] = (byte)(left[i] + right[i]);
    }

    /// <summary>
    /// Saturating byte add clamped to 0–255.
    /// </summary>
    internal static void AddSaturating(
        ReadOnlySpan<byte> left,
        ReadOnlySpan<byte> right,
        Span<byte> destination
    )
    {
        int length = left.Length;
        int i = 0;

        if (Avx2.IsSupported)
        {
            int step = Vector256<byte>.Count;
            for (; i <= length - step; i += step)
            {
                var vLeft = Vector256.Create(left.Slice(i));
                var vRight = Vector256.Create(right.Slice(i));
                Avx2.AddSaturate(vLeft, vRight).CopyTo(destination.Slice(i));
            }
        }
        else if (Sse2.IsSupported)
        {
            int step = Vector128<byte>.Count;
            for (; i <= length - step; i += step)
            {
                var vLeft = Vector128.Create(left.Slice(i));
                var vRight = Vector128.Create(right.Slice(i));
                Sse2.AddSaturate(vLeft, vRight).CopyTo(destination.Slice(i));
            }
        }
        else if (AdvSimd.IsSupported)
        {
            int step = Vector128<byte>.Count;
            for (; i <= length - step; i += step)
            {
                var vLeft = Vector128.Create(left.Slice(i));
                var vRight = Vector128.Create(right.Slice(i));
                AdvSimd.AddSaturate(vLeft, vRight).CopyTo(destination.Slice(i));
            }
        }
        else if (Vector.IsHardwareAccelerated)
        {
            int step = Vector<byte>.Count;
            var max = new Vector<ushort>(255);
            for (; i <= length - step; i += step)
            {
                var vLeft = new Vector<byte>(left.Slice(i));
                var vRight = new Vector<byte>(right.Slice(i));
                Vector.Widen(vLeft, out Vector<ushort> leftLo, out Vector<ushort> leftHi);
                Vector.Widen(vRight, out Vector<ushort> rightLo, out Vector<ushort> rightHi);
                var sumLo = Vector.Min(leftLo + rightLo, max);
                var sumHi = Vector.Min(leftHi + rightHi, max);
                Vector.Narrow(sumLo, sumHi).CopyTo(destination.Slice(i));
            }
        }

        for (; i < length; i++)
        {
            int sum = left[i] + right[i];
            destination[i] = (byte)(sum > 255 ? 255 : sum);
        }
    }

    /// <summary>
    /// Saturating byte subtract clamped to 0–255.
    /// </summary>
    internal static void SubtractSaturating(
        ReadOnlySpan<byte> left,
        ReadOnlySpan<byte> right,
        Span<byte> destination
    )
    {
        int length = left.Length;
        int i = 0;

        if (Avx2.IsSupported)
        {
            int step = Vector256<byte>.Count;
            for (; i <= length - step; i += step)
            {
                var vLeft = Vector256.Create(left.Slice(i));
                var vRight = Vector256.Create(right.Slice(i));
                Avx2.SubtractSaturate(vLeft, vRight).CopyTo(destination.Slice(i));
            }
        }
        else if (Sse2.IsSupported)
        {
            int step = Vector128<byte>.Count;
            for (; i <= length - step; i += step)
            {
                var vLeft = Vector128.Create(left.Slice(i));
                var vRight = Vector128.Create(right.Slice(i));
                Sse2.SubtractSaturate(vLeft, vRight).CopyTo(destination.Slice(i));
            }
        }
        else if (AdvSimd.IsSupported)
        {
            int step = Vector128<byte>.Count;
            for (; i <= length - step; i += step)
            {
                var vLeft = Vector128.Create(left.Slice(i));
                var vRight = Vector128.Create(right.Slice(i));
                AdvSimd.SubtractSaturate(vLeft, vRight).CopyTo(destination.Slice(i));
            }
        }
        else if (Vector.IsHardwareAccelerated)
        {
            int step = Vector<byte>.Count;
            for (; i <= length - step; i += step)
            {
                var vLeft = new Vector<byte>(left.Slice(i));
                var vRight = new Vector<byte>(right.Slice(i));
                Vector.Widen(vLeft, out Vector<ushort> leftLo, out Vector<ushort> leftHi);
                Vector.Widen(vRight, out Vector<ushort> rightLo, out Vector<ushort> rightHi);
                var diffLo = Vector.ConditionalSelect(
                    Vector.GreaterThanOrEqual(leftLo, rightLo),
                    leftLo - rightLo,
                    Vector<ushort>.Zero
                );
                var diffHi = Vector.ConditionalSelect(
                    Vector.GreaterThanOrEqual(leftHi, rightHi),
                    leftHi - rightHi,
                    Vector<ushort>.Zero
                );
                Vector.Narrow(diffLo, diffHi).CopyTo(destination.Slice(i));
            }
        }

        for (; i < length; i++)
        {
            int diff = left[i] - right[i];
            destination[i] = (byte)(diff < 0 ? 0 : diff);
        }
    }

    /// <summary>
    /// In-place bitwise AND of two equal-length spans into <paramref name="left"/>.
    /// </summary>
    internal static void AndInPlace(Span<byte> left, ReadOnlySpan<byte> right)
    {
        int length = left.Length;
        int i = 0;
        int simdLength = Vector<byte>.Count;

        if (Vector.IsHardwareAccelerated)
        {
            for (; i <= length - simdLength; i += simdLength)
            {
                var vLeft = new Vector<byte>(left.Slice(i));
                var vRight = new Vector<byte>(right.Slice(i));
                (vLeft & vRight).CopyTo(left.Slice(i));
            }
        }

        for (; i < length; i++)
            left[i] &= right[i];
    }

    /// <summary>
    /// In-place bitwise OR of two equal-length spans into <paramref name="left"/>.
    /// </summary>
    internal static void OrInPlace(Span<byte> left, ReadOnlySpan<byte> right)
    {
        int length = left.Length;
        int i = 0;
        int simdLength = Vector<byte>.Count;

        if (Vector.IsHardwareAccelerated)
        {
            for (; i <= length - simdLength; i += simdLength)
            {
                var vLeft = new Vector<byte>(left.Slice(i));
                var vRight = new Vector<byte>(right.Slice(i));
                (vLeft | vRight).CopyTo(left.Slice(i));
            }
        }

        for (; i < length; i++)
            left[i] |= right[i];
    }

    /// <summary>
    /// In-place bitwise XOR of two equal-length spans into <paramref name="left"/>.
    /// </summary>
    internal static void XorInPlace(Span<byte> left, ReadOnlySpan<byte> right)
    {
        int length = left.Length;
        int i = 0;
        int simdLength = Vector<byte>.Count;

        if (Vector.IsHardwareAccelerated)
        {
            for (; i <= length - simdLength; i += simdLength)
            {
                var vLeft = new Vector<byte>(left.Slice(i));
                var vRight = new Vector<byte>(right.Slice(i));
                (vLeft ^ vRight).CopyTo(left.Slice(i));
            }
        }

        for (; i < length; i++)
            left[i] ^= right[i];
    }

    /// <summary>
    /// PNG Average encode predictor: <c>dst[i] = cur[i] - ((left + up) / 2)</c> where
    /// <c>left</c> is 0 for the first <paramref name="bytesPerPixel"/> samples, then <c>cur[i - bpp]</c>.
    /// No dependency on <paramref name="destination"/>.
    /// </summary>
    internal static void AveragePredictSubtract(
        ReadOnlySpan<byte> current,
        ReadOnlySpan<byte> previous,
        Span<byte> destination,
        int bytesPerPixel
    )
    {
        int length = current.Length;
        int i = 0;
        int bpp = bytesPerPixel;

        for (; i < bpp && i < length; i++)
            destination[i] = (byte)(current[i] - (previous[i] >> 1));

        int simdLength = Vector<byte>.Count;
        if (Vector.IsHardwareAccelerated)
        {
            var two = new Vector<ushort>(2);
            for (; i <= length - simdLength; i += simdLength)
            {
                var vCur = new Vector<byte>(current.Slice(i));
                var vLeft = new Vector<byte>(current.Slice(i - bpp));
                var vUp = new Vector<byte>(previous.Slice(i));
                Vector.Widen(vLeft, out Vector<ushort> leftLo, out Vector<ushort> leftHi);
                Vector.Widen(vUp, out Vector<ushort> upLo, out Vector<ushort> upHi);
                var avgLo = (leftLo + upLo) / two;
                var avgHi = (leftHi + upHi) / two;
                var predictor = Vector.Narrow(avgLo, avgHi);
                (vCur - predictor).CopyTo(destination.Slice(i));
            }
        }

        for (; i < length; i++)
        {
            byte left = current[i - bpp];
            byte up = previous[i];
            destination[i] = (byte)(current[i] - ((left + up) >> 1));
        }
    }

    /// <summary>
    /// PNG Sub encode: <c>dst[i] = cur[i] - cur[i - bpp]</c> (left = 0 for the first bpp bytes).
    /// </summary>
    internal static void SubPredictSubtract(
        ReadOnlySpan<byte> current,
        Span<byte> destination,
        int bytesPerPixel
    )
    {
        int length = current.Length;
        int i = 0;
        int bpp = bytesPerPixel;

        for (; i < bpp && i < length; i++)
            destination[i] = current[i];

        int simdLength = Vector<byte>.Count;
        if (Vector.IsHardwareAccelerated)
        {
            for (; i <= length - simdLength; i += simdLength)
            {
                var vCur = new Vector<byte>(current.Slice(i));
                var vLeft = new Vector<byte>(current.Slice(i - bpp));
                (vCur - vLeft).CopyTo(destination.Slice(i));
            }
        }

        for (; i < length; i++)
            destination[i] = (byte)(current[i] - current[i - bpp]);
    }

    /// <summary>
    /// PNG Sub decode in place: <c>data[i] += data[i - bpp]</c> (left = 0 for the first bpp bytes).
    /// Left-dependency keeps this scalar.
    /// </summary>
    internal static void SubPredictAddInPlace(Span<byte> data, int bytesPerPixel)
    {
        int bpp = bytesPerPixel;
        for (int i = bpp; i < data.Length; i++)
            data[i] = (byte)(data[i] + data[i - bpp]);
    }

    /// <summary>
    /// PNG Average decode in place: <c>data[i] += (left + up) / 2</c>.
    /// Left-dependency keeps this scalar.
    /// </summary>
    internal static void AveragePredictAddInPlace(
        Span<byte> data,
        ReadOnlySpan<byte> previous,
        int bytesPerPixel
    )
    {
        int bpp = bytesPerPixel;
        int length = data.Length;
        int i = 0;

        for (; i < bpp && i < length; i++)
            data[i] = (byte)(data[i] + (previous[i] >> 1));

        for (; i < length; i++)
            data[i] = (byte)(data[i] + ((data[i - bpp] + previous[i]) >> 1));
    }

    /// <summary>
    /// PNG Paeth decode in place.
    /// </summary>
    internal static void PaethPredictAddInPlace(
        Span<byte> data,
        ReadOnlySpan<byte> previous,
        int bytesPerPixel
    )
    {
        int bpp = bytesPerPixel;
        int length = data.Length;
        int i = 0;

        for (; i < bpp && i < length; i++)
        {
            byte above = previous[i];
            data[i] = (byte)(data[i] + PaethPredictor(0, above, 0));
        }

        for (; i < length; i++)
        {
            byte left = data[i - bpp];
            byte above = previous[i];
            byte upperLeft = previous[i - bpp];
            data[i] = (byte)(data[i] + PaethPredictor(left, above, upperLeft));
        }
    }

    /// <summary>
    /// PNG Paeth encode: <c>dst[i] = cur[i] - Paeth(left, up, upLeft)</c>.
    /// Branchy; kept scalar.
    /// </summary>
    internal static void PaethPredictSubtract(
        ReadOnlySpan<byte> current,
        ReadOnlySpan<byte> previous,
        Span<byte> destination,
        int bytesPerPixel
    )
    {
        int bpp = bytesPerPixel;
        int length = current.Length;
        int i = 0;

        for (; i < bpp && i < length; i++)
            destination[i] = (byte)(current[i] - PaethPredictor(0, previous[i], 0));

        for (; i < length; i++)
        {
            byte left = current[i - bpp];
            byte above = previous[i];
            byte upperLeft = previous[i - bpp];
            destination[i] = (byte)(current[i] - PaethPredictor(left, above, upperLeft));
        }
    }

    private static byte PaethPredictor(byte left, byte above, byte upperLeft)
    {
        int p = left + above - upperLeft;
        int pa = Math.Abs(p - left);
        int pb = Math.Abs(p - above);
        int pc = Math.Abs(p - upperLeft);
        if (pa <= pb && pa <= pc)
            return left;
        return pb <= pc ? above : upperLeft;
    }

    /// <summary>
    /// Reinterprets a color pixel span as bytes (RGBA packed).
    /// </summary>
    internal static Span<byte> AsBytes<T>(Span<T> pixels)
        where T : unmanaged => MemoryMarshal.AsBytes(pixels);

    /// <summary>
    /// Reinterprets a color pixel span as <see cref="uint"/> values.
    /// </summary>
    internal static Span<uint> AsUInts<T>(Span<T> pixels)
        where T : unmanaged => MemoryMarshal.Cast<T, uint>(pixels);
}
