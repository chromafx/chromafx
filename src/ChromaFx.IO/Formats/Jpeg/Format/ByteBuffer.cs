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

using ChromaFx.Core.Exceptions;
using ChromaFx.IO.Formats.Jpeg.Format.Exceptions;

namespace ChromaFx.IO.Formats.Jpeg.Format;

/// <summary>
/// Byte based buffer class
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ByteBuffer" /> class.
/// </remarks>
/// <param name="stream">The stream.</param>
internal class ByteBuffer(Stream stream)
{
    // Entropy-coded bits, most significant first. Refills never consume a marker; once one is
    // reached, zero bits are appended as padding and consuming padding throws MissingFf00.
    private ulong _accumulator;
    private int _bitCount;
    private int _paddingBits;
    private bool _atMarker;

    public byte[] Buffer { get; set; } = new byte[4096];

    public int I { get; set; } = 0;

    public int J { get; set; } = 0;

    public Stream Stream { get; set; } = stream;

    public void ResetBits()
    {
        _accumulator = 0;
        _bitCount = 0;
        _paddingBits = 0;
        _atMarker = false;
    }

    public bool DecodeBit()
    {
        if (_bitCount < 1)
            Refill(1);
        _bitCount--;
        CheckPadding();
        return ((_accumulator >> _bitCount) & 1) != 0;
    }

    public uint DecodeBits(int n)
    {
        if (_bitCount < n)
            Refill(n);
        _bitCount -= n;
        CheckPadding();
        return (uint)(_accumulator >> _bitCount) & (uint)((1 << n) - 1);
    }

    public int PeekByte()
    {
        if (_bitCount < 8)
            Refill(8);
        return (int)(_accumulator >> (_bitCount - 8)) & 0xff;
    }

    public void SkipBits(int n)
    {
        _bitCount -= n;
        CheckPadding();
    }

    public int ReceiveExtend(byte t)
    {
        if (_bitCount < t)
            Refill(t);
        _bitCount -= t;
        CheckPadding();
        var s = 1 << t;
        var x = (int)((_accumulator >> _bitCount) & (ulong)(s - 1));
        if (x < s >> 1)
            x += (-1 << t) + 1;
        return x;
    }

    public void Fill()
    {
        if (I != J)
            throw new ImageException("jpeg: fill called when unread bytes exist");
        if (J > 2)
        {
            Buffer[0] = Buffer[J - 2];
            Buffer[1] = Buffer[J - 1];
            I = 2;
            J = 2;
        }
        var n = Stream.Read(Buffer, J, Buffer.Length - J);
        if (n == 0)
            throw new Eof();
        J += n;
    }

    public void Ignore(int n)
    {
        ResetBits();
        while (true)
        {
            var m = J - I;
            if (m > n)
                m = n;
            I += m;
            n -= m;
            if (n == 0)
                break;
            Fill();
        }
    }

    public byte ReadByte()
    {
        while (I == J)
            Fill();
        var x = Buffer[I];
        I++;
        return x;
    }

    public void ReadFull(byte[] data, int offset, int len)
    {
        ResetBits();
        while (len > 0)
        {
            if (J - I >= len)
            {
                Array.Copy(Buffer, I, data, offset, len);
                I += len;
                len = 0;
            }
            else
            {
                Array.Copy(Buffer, I, data, offset, J - I);
                offset += J - I;
                len -= J - I;
                I += J - I;
                Fill();
            }
        }
    }

    private void CheckPadding()
    {
        if (_bitCount < _paddingBits)
            throw new MissingFf00();
    }

    // Buffered bytes are taken greedily; the stream is only read when n bits are still missing.
    private void Refill(int n)
    {
        while (_bitCount <= 56)
        {
            if (_atMarker)
            {
                if (_bitCount >= n)
                    return;
                _accumulator <<= 8;
                _bitCount += 8;
                _paddingBits += 8;
                continue;
            }

            byte x;
            if (I + 2 <= J)
            {
                x = Buffer[I];
                if (x != 0xff)
                {
                    I++;
                }
                else if (Buffer[I + 1] == 0x00)
                {
                    I += 2;
                }
                else
                {
                    _atMarker = true;
                    continue;
                }
            }
            else
            {
                if (_bitCount >= n)
                    return;
                x = ReadByte();
                if (x == 0xff && ReadByte() != 0x00)
                {
                    I -= 2;
                    _atMarker = true;
                    continue;
                }
            }

            _accumulator = (_accumulator << 8) | x;
            _bitCount += 8;
        }
    }
}
