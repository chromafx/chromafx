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

using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.InteropServices;

namespace ChromaFx.IO.Formats.Bmp.Format.PixelFormats;

/// <summary>
/// Pixel format base class
/// </summary>
/// <seealso cref="IPixelFormat"/>
internal abstract class PixelFormatBase : IPixelFormat
{
    /// <summary>
    /// The bytes per pixel for this format.
    /// </summary>
    /// <value>The BPP.</value>
    public abstract double Bpp { get; }

    /// <summary>
    /// Decodes the specified data.
    /// </summary>
    /// <param name="header">The header.</param>
    /// <param name="data">The data.</param>
    /// <param name="palette">The palette.</param>
    /// <returns>The decoded data</returns>
    public abstract byte[] Decode(Header header, byte[] data, Palette palette);

    /// <summary>
    /// Encodes the specified data.
    /// </summary>
    /// <param name="header">The header.</param>
    /// <param name="data">The data.</param>
    /// <param name="palette">The palette.</param>
    /// <returns>The encoded data</returns>
    public abstract byte[] Encode(Header header, byte[] data, Palette palette);

    /// <summary>
    /// Reads the byte array from the stream
    /// </summary>
    /// <param name="header">The header.</param>
    /// <param name="stream">The stream.</param>
    /// <returns>The byte array of the data</returns>
    public byte[] Read(Header header, Stream stream)
    {
        var bits = BitsPerPixel;
        var stride = Stride(header.Width, bits);
        switch (header.Compression)
        {
            case Compression.Rgb or Compression.Bitfields:
            {
                // biSizeImage is often 0 or wrong, so size the read from the row stride.
                var data = new byte[stride * header.Height];
                stream.ReadExactly(data);
                return data;
            }
            case Compression.Rle8 when bits == 8:
                return ReadRle(header, stream, stride, false);
            case Compression.Rle4 when bits == 4:
                return ReadRle(header, stream, stride, true);
        }

        return [];
    }

    protected int BitsPerPixel => (int)Math.Round(Bpp * 8);

    protected static int Stride(int width, int bitsPerPixel) => (width * bitsPerPixel + 31) / 32 * 4;

    protected static int SourceRow(Header header, int destinationRow) =>
        header.TopDown ? destinationRow : header.Height - destinationRow - 1;

    /// <summary>
    /// Decodes 1/4/8-bit palette indices. Palette entries are BGR plus a reserved byte, so pixels are opaque.
    /// </summary>
    protected static byte[] DecodeIndexed(Header header, byte[] data, Palette palette, int bits)
    {
        var width = header.Width;
        var height = header.Height;
        var stride = Stride(width, bits);
        var colors = new uint[1 << bits];
        Array.Fill(colors, 0xFF00_0000u);
        var count = Math.Min(colors.Length, palette.NumberOfColors);
        for (var i = 0; i < count; i++)
        {
            var o = i * 4;
            colors[i] =
                palette.Data[o + 2]
                | ((uint)palette.Data[o + 1] << 8)
                | ((uint)palette.Data[o] << 16)
                | 0xFF00_0000u;
        }

        var result = new byte[width * height * 4];
        var mask = (1 << bits) - 1;
        var perByte = 8 / bits;
        Parallel.For(
            0,
            height,
            y =>
            {
                var sourceOffset = SourceRow(header, y) * stride;
                if (sourceOffset + stride > data.Length)
                    return;
                var source = data.AsSpan(sourceOffset, stride);
                var dest = MemoryMarshal.Cast<byte, uint>(result.AsSpan(y * width * 4, width * 4));
                if (bits == 8)
                {
                    for (var x = 0; x < width; x++)
                        dest[x] = colors[source[x]];
                    return;
                }

                for (var x = 0; x < width; x++)
                {
                    var shift = 8 - bits * (x % perByte + 1);
                    dest[x] = colors[(source[x / perByte] >> shift) & mask];
                }
            }
        );
        return result;
    }

    /// <summary>
    /// Decodes 16/32-bit pixels using the header bitfield masks, or the BI_RGB defaults when absent.
    /// </summary>
    protected static byte[] DecodeMasked(
        Header header,
        byte[] data,
        int bits,
        uint defaultRed,
        uint defaultGreen,
        uint defaultBlue
    )
    {
        var useMasks =
            header.Compression == Compression.Bitfields
            && (header.RedMask | header.GreenMask | header.BlueMask) != 0;
        var red = new ChannelMask(useMasks ? (uint)header.RedMask : defaultRed);
        var green = new ChannelMask(useMasks ? (uint)header.GreenMask : defaultGreen);
        var blue = new ChannelMask(useMasks ? (uint)header.BlueMask : defaultBlue);
        var alpha = new ChannelMask(useMasks ? (uint)header.AlphaMask : 0);
        var width = header.Width;
        var height = header.Height;
        var stride = Stride(width, bits);
        var result = new byte[width * height * 4];
        Parallel.For(
            0,
            height,
            y =>
            {
                var sourceOffset = SourceRow(header, y) * stride;
                if (sourceOffset + stride > data.Length)
                    return;
                var source = data.AsSpan(sourceOffset, stride);
                var dest = MemoryMarshal.Cast<byte, uint>(result.AsSpan(y * width * 4, width * 4));
                for (var x = 0; x < width; x++)
                {
                    var pixel =
                        bits == 32
                            ? BinaryPrimitives.ReadUInt32LittleEndian(source[(x * 4)..])
                            : BinaryPrimitives.ReadUInt16LittleEndian(source[(x * 2)..]);
                    var a = alpha.IsEmpty ? (byte)255 : alpha.Extract(pixel);
                    dest[x] =
                        red.Extract(pixel)
                        | ((uint)green.Extract(pixel) << 8)
                        | ((uint)blue.Extract(pixel) << 16)
                        | ((uint)a << 24);
                }
            }
        );
        return result;
    }

    private readonly struct ChannelMask
    {
        private readonly uint _mask;
        private readonly int _shift;
        private readonly int _bits;
        private readonly uint _max;

        public ChannelMask(uint mask)
        {
            _mask = mask;
            _shift = mask == 0 ? 0 : BitOperations.TrailingZeroCount(mask);
            _bits = BitOperations.PopCount(mask);
            _max = _bits >= 32 ? uint.MaxValue : (1u << _bits) - 1;
        }

        public bool IsEmpty => _mask == 0;

        public byte Extract(uint pixel)
        {
            if (_mask == 0)
                return 0;
            var value = (pixel & _mask) >> _shift;
            return _bits >= 8 ? (byte)(value >> (_bits - 8)) : (byte)(value * 255 / _max);
        }
    }

    private static byte[] ReadRle(Header header, Stream stream, int stride, bool rle4)
    {
        using var buffer = new MemoryStream();
        stream.CopyTo(buffer);
        var src = buffer.GetBuffer();
        var length = (int)buffer.Length;
        var width = header.Width;
        var height = header.Height;
        var output = new byte[stride * height];
        int x = 0, y = 0, i = 0;

        void Put(int index)
        {
            if (x < width && y < height)
            {
                if (rle4)
                    output[y * stride + (x >> 1)] |= (byte)((x & 1) == 0 ? index << 4 : index);
                else
                    output[y * stride + x] = (byte)index;
            }
            x++;
        }

        while (i + 1 < length && y < height)
        {
            int count = src[i++];
            int value = src[i++];
            if (count > 0)
            {
                for (var k = 0; k < count; k++)
                    Put(rle4 ? ((k & 1) == 0 ? value >> 4 : value & 15) : value);
                continue;
            }

            switch (value)
            {
                case 0:
                    x = 0;
                    y++;
                    break;
                case 1:
                    return output;
                case 2:
                    if (i + 1 >= length)
                        return output;
                    x += src[i++];
                    y += src[i++];
                    break;
                default:
                    var bytes = rle4 ? (value + 1) / 2 : value;
                    for (var k = 0; k < value; k++)
                    {
                        var p = i + (rle4 ? k >> 1 : k);
                        if (p >= length)
                            break;
                        Put(rle4 ? ((k & 1) == 0 ? src[p] >> 4 : src[p] & 15) : src[p]);
                    }
                    i += bytes + (bytes & 1);
                    break;
            }
        }

        return output;
    }
}
