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

using ChromaFx.Exceptions;
using ChromaFx.Formats.Jpeg.Format.HelperClasses.Exceptions;

namespace ChromaFx.Formats.Jpeg.Format.HelperClasses;

/// <summary>
/// Byte based buffer class
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ByteBuffer" /> class.
/// </remarks>
/// <param name="stream">The stream.</param>
public class ByteBuffer(Stream stream)
{

    /// <summary>
    /// Gets or sets the Bits.
    /// </summary>
    /// <value>
    /// The Bits.
    /// </value>
    public BitsBuffer Bits { get; set; } = new BitsBuffer();

    /// <summary>
    /// Gets or sets the buffer.
    /// </summary>
    /// <value>
    /// The buffer.
    /// </value>
    public byte[] Buffer { get; set; } = new byte[4096];

    /// <summary>
    /// Gets or sets the i.
    /// </summary>
    /// <value>
    /// The i.
    /// </value>
    public int I { get; set; } = 0;

    /// <summary>
    /// Gets or sets the j.
    /// </summary>
    /// <value>
    /// The j.
    /// </value>
    public int J { get; set; } = 0;

    /// <summary>
    /// Gets or sets the stream.
    /// </summary>
    /// <value>
    /// The stream.
    /// </value>
    public Stream Stream { get; set; } = stream;

    /// <summary>
    /// The unreadable bytes
    /// </summary>
    public int UnreadableBytes = 0;

    /// <summary>
    /// Decodes the bit.
    /// </summary>
    /// <returns>The value.</returns>
    public bool DecodeBit()
    {
        if (Bits.NumberOfUnreadBits == 0)
            EnsureNBits(1);

        var ret = (Bits.Accumulator & Bits.Mask) != 0;
        Bits.NumberOfUnreadBits--;
        Bits.Mask >>= 1;
        return ret;
    }

    /// <summary>
    /// Decodes the bits.
    /// </summary>
    /// <param name="n">The n.</param>
    /// <returns>The value.</returns>
    public uint DecodeBits(int n)
    {
        if (Bits.NumberOfUnreadBits < n)
            EnsureNBits(n);

        var ret = Bits.Accumulator >> (Bits.NumberOfUnreadBits - n);
        ret = (uint)(ret & ((1 << n) - 1));
        Bits.NumberOfUnreadBits -= n;
        Bits.Mask >>= n;
        return ret;
    }

    /// <summary>
    /// Ensures the n bits.
    /// </summary>
    /// <param name="n">The n.</param>
    public void EnsureNBits(int n)
    {
        while (true)
        {
            var c = ReadByteStuffedByte();
            Bits.Accumulator = (Bits.Accumulator << 8) | c;
            Bits.NumberOfUnreadBits += 8;
            if (Bits.Mask == 0)
                Bits.Mask = 1 << 7;
            else
                Bits.Mask <<= 8;
            if (Bits.NumberOfUnreadBits >= n)
                break;
        }
    }

    /// <summary>
    /// Fills this instance.
    /// </summary>
    /// <exception cref="ImageException">jpeg: fill called when unread bytes exist</exception>
    /// <exception cref="Eof">End of file reached</exception>
    public void Fill()
    {
        if (I != J)
            throw new ImageException("jpeg: fill called when unread bytes exist");

        // Move the last 2 bytes to the start of the buffer, in case we need
        // to call unreadByteStuffedByte.
        if (J > 2)
        {
            Buffer[0] = Buffer[J - 2];
            Buffer[1] = Buffer[J - 1];
            I = 2;
            J = 2;
        }

        // Fill in the rest of the buffer.
        var n = Stream.Read(Buffer, J, Buffer.Length - J);
        if (n == 0)
            throw new Eof();
        J += n;
    }

    /// <summary>
    /// Ignores the specified n.
    /// </summary>
    /// <param name="n">The n.</param>
    public void Ignore(int n)
    {
        // Unread the overshot bytes, if any.
        if (UnreadableBytes != 0)
        {
            if (Bits.NumberOfUnreadBits >= 8)
                UnreadByteStuffedByte();
            UnreadableBytes = 0;
        }

        while (true)
        {
            var m = J - I;
            if (m > n) m = n;
            I += m;
            n -= m;
            if (n == 0)
                break;
            Fill();
        }
    }

    /// <summary>
    /// Reads the byte.
    /// </summary>
    /// <returns>The byte</returns>
    public byte ReadByte()
    {
        while (I == J)
            Fill();
        var x = Buffer[I];
        I++;
        UnreadableBytes = 0;
        return x;
    }

    /// <summary>
    /// Reads the byte stuffed byte.
    /// </summary>
    /// <returns>The byte</returns>
    /// <exception cref="MissingFf00">
    /// </exception>
    public byte ReadByteStuffedByte()
    {
        byte x;

        if (I + 2 <= J)
        {
            x = Buffer[I];
            I++;
            UnreadableBytes = 1;
            if (x != 0xff)
                return x;
            if (Buffer[I] != 0x00)
                throw new MissingFf00();
            I++;
            UnreadableBytes = 2;
            return 0xff;
        }

        UnreadableBytes = 0;

        x = ReadByte();
        UnreadableBytes = 1;
        if (x != 0xff)
            return x;
        x = ReadByte();
        UnreadableBytes = 2;
        if (x != 0x00)
            throw new MissingFf00();
        return 0xff;
    }

    /// <summary>
    /// Reads the full.
    /// </summary>
    /// <param name="data">The data.</param>
    /// <param name="offset">The offset.</param>
    /// <param name="len">The length.</param>
    public void ReadFull(byte[] data, int offset, int len)
    {
        // Unread the overshot bytes, if any.
        if (UnreadableBytes != 0)
        {
            if (Bits.NumberOfUnreadBits >= 8)
                UnreadByteStuffedByte();
            UnreadableBytes = 0;
        }

        while (len > 0)
        {
            if (J - I >= len)
            {
                Array.Copy(Buffer, I, data, offset, len);
                I += len;
                len -= len;
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

    /// <summary>
    /// Receives the extend.
    /// </summary>
    /// <param name="t">The t.</param>
    /// <returns></returns>
    public int ReceiveExtend(byte t)
    {
        if (Bits.NumberOfUnreadBits < t)
            EnsureNBits(t);

        Bits.NumberOfUnreadBits -= t;
        Bits.Mask >>= t;
        var s = 1 << t;
        var x = (int)((Bits.Accumulator >> Bits.NumberOfUnreadBits) & (s - 1));
        if (x < s >> 1)
            x += (-1 << t) + 1;
        return x;
    }

    /// <summary>
    /// Unreads the byte stuffed byte.
    /// </summary>
    public void UnreadByteStuffedByte()
    {
        I -= UnreadableBytes;
        UnreadableBytes = 0;
        if (Bits.NumberOfUnreadBits >= 8)
        {
            Bits.Accumulator >>= 8;
            Bits.NumberOfUnreadBits -= 8;
            Bits.Mask >>= 8;
        }
    }
}