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

using System.Buffers.Binary;
using System.IO.Compression;
using ChromaFx.Core;
using ChromaFx.Core.Colors;
using ChromaFx.Core.Exceptions;
using ChromaFx.IO.Formats;

namespace ChromaFx.IO.Formats.Tiff;

internal static class TiffCodec
{
    private const int MaxPixels = 32_000_000;
    private const int CompressionNone = 1;
    private const int CompressionLzw = 5;
    private const int CompressionDeflate = 8;
    private const int CompressionPackBits = 32773;
    private const int CompressionDeflateOld = 32946;

    internal static bool LooksLike(ReadOnlySpan<byte> header)
    {
        if (header.Length < 4)
            return false;
        var little = header[0] == (byte)'I' && header[1] == (byte)'I';
        var big = header[0] == (byte)'M' && header[1] == (byte)'M';
        if (!little && !big)
            return false;
        var magic = little
            ? BinaryPrimitives.ReadUInt16LittleEndian(header[2..])
            : BinaryPrimitives.ReadUInt16BigEndian(header[2..]);
        return magic is 42 or 43;
    }

    internal static Image Decode(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        return new Reader(ReadAll(stream)).Decode();
    }

    internal static void Encode(BinaryWriter writer, Image image)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(image);
        if (image.Width <= 0 || image.Height <= 0 || (long)image.Width * image.Height > MaxPixels)
            throw new ImageException("TIFF image is too large.");

        const int tagCount = 11;
        var bitsAt = 8 + 2 + tagCount * 12 + 4;
        var dataAt = bitsAt + 8;
        var byteCount = image.Width * image.Height * 4;

        Span<byte> header = stackalloc byte[8];
        header[0] = (byte)'I';
        header[1] = (byte)'I';
        BinaryPrimitives.WriteUInt16LittleEndian(header[2..], 42);
        BinaryPrimitives.WriteUInt32LittleEndian(header[4..], 8);
        writer.Write(header);
        writer.Write((ushort)tagCount);
        WriteLong(writer, 256, image.Width);
        WriteLong(writer, 257, image.Height);
        WriteField(writer, 258, 3, 4, bitsAt);
        WriteShort(writer, 259, 1);
        WriteShort(writer, 262, 2);
        WriteLong(writer, 273, dataAt);
        WriteShort(writer, 277, 4);
        WriteLong(writer, 278, image.Height);
        WriteLong(writer, 279, byteCount);
        WriteShort(writer, 284, 1);
        WriteShort(writer, 338, 2);
        writer.Write(0);
        writer.Write((ushort)8);
        writer.Write((ushort)8);
        writer.Write((ushort)8);
        writer.Write((ushort)8);

        Span<byte> pixel = stackalloc byte[4];
        for (var y = 0; y < image.Height; y++)
        {
            var row = image.GetRowSpan(y);
            for (var x = 0; x < image.Width; x++)
            {
                var color = row[x];
                pixel[0] = color.Red;
                pixel[1] = color.Green;
                pixel[2] = color.Blue;
                pixel[3] = color.Alpha;
                writer.Write(pixel);
            }
        }
    }

    private static void WriteShort(BinaryWriter writer, ushort tag, ushort value)
    {
        writer.Write(tag);
        writer.Write((ushort)3);
        writer.Write(1u);
        writer.Write(value);
        writer.Write((ushort)0);
    }

    private static void WriteLong(BinaryWriter writer, ushort tag, int value) =>
        WriteField(writer, tag, 4, 1, value);

    private static void WriteField(BinaryWriter writer, ushort tag, ushort type, uint count, int value)
    {
        writer.Write(tag);
        writer.Write(type);
        writer.Write(count);
        writer.Write(value);
    }

    private static byte[] ReadAll(Stream stream)
    {
        if (stream.CanSeek)
        {
            stream.Position = 0;
            if (stream.Length > int.MaxValue)
                throw new ImageException("TIFF image is too large.");
            var buffer = new byte[(int)stream.Length];
            stream.ReadExactly(buffer);
            return buffer;
        }

        using var copy = new MemoryStream();
        stream.CopyTo(copy);
        return copy.ToArray();
    }

    private readonly struct Field(int type, int count, int valueAt)
    {
        public int Type { get; } = type;
        public int Count { get; } = count;
        public int ValueAt { get; } = valueAt;
    }

    private sealed class Reader(byte[] data)
    {
        private readonly Dictionary<int, Field> _fields = [];
        private readonly bool _little =
            data.Length >= 2 && data[0] == (byte)'I' && data[1] == (byte)'I';
        private int _width;
        private int _height;
        private int _samples;
        private int _bits;
        private int _photometric;
        private Color[] _palette;

        public Image Decode()
        {
            if (!LooksLike(data))
                throw new ImageException("Not a TIFF image.");
            var magic = ReadU16(2);
            if (magic == 43)
                throw new ImageException("BigTIFF is not supported.");
            if (magic != 42)
                throw new ImageException("Not a TIFF image.");

            var ifd = ReadOffset(4);
            var count = ReadU16(ifd);
            if (count > 4096)
                throw new ImageException("TIFF directory is too large.");
            for (var i = 0; i < count; i++)
            {
                var entry = ifd + 2 + i * 12;
                Need(entry, 12);
                var countValue = ReadU32(entry + 4);
                if (countValue > int.MaxValue)
                    throw new ImageException("TIFF field is too large.");
                _fields[ReadU16(entry)] = new Field(ReadU16(entry + 2), (int)countValue, entry + 8);
            }

            _width = ReadDimension(256);
            _height = ReadDimension(257);
            if ((long)_width * _height > MaxPixels)
                throw new ImageException("TIFF image is too large.");

            _photometric = (int)Require(262);
            _samples = (int)Optional(277, 1);
            _bits = ReadBits();
            if (Optional(284, 1) != 1)
                throw new ImageException("TIFF planar configuration 2 is not supported.");
            var orientation = (int)Optional(274, 1);
            if (orientation is < 1 or > 8)
                orientation = 1;
            if (Optional(266, 1) != 1)
                throw new ImageException("TIFF fill order 2 is not supported.");
            RejectSampleFormat();

            var predictor = (int)Optional(317, 1);
            if (predictor is not (1 or 2))
                throw new ImageException($"TIFF predictor {predictor} is not supported.");

            var compression = (int)Optional(259, CompressionNone);
            if (
                compression
                is not (
                    CompressionNone
                    or CompressionLzw
                    or CompressionDeflate
                    or CompressionPackBits
                    or CompressionDeflateOld
                )
            )
                throw new ImageException($"TIFF compression {compression} is not supported.");
            if (
                predictor == 2
                && compression is not (CompressionLzw or CompressionDeflate or CompressionDeflateOld)
            )
                throw new ImageException("TIFF predictor 2 requires LZW or Deflate.");

            ValidatePhotometric();
            if (_photometric == 3)
                _palette = LoadPalette();

            var image = new Image(_width, _height);
            if (_fields.ContainsKey(322))
                DecodeTiles(image, compression, predictor);
            else
                DecodeStrips(image, compression, predictor);
            return ExifOrientation.Apply(image, orientation);
        }

        private void DecodeStrips(Image image, int compression, int predictor)
        {
            var rowsPerStrip = Optional(278, (uint)_height);
            if (rowsPerStrip == 0)
                throw new ImageException("TIFF rows per strip is zero.");
            var offsets = ReadArray(273);
            var counts = ReadArray(279);
            var stripCount = (int)((_height + (long)rowsPerStrip - 1) / rowsPerStrip);
            if (offsets.Length < stripCount || counts.Length < stripCount)
                throw new ImageException("TIFF strip table is too short.");

            var rowBytes = RowBytes(_width);
            for (var i = 0; i < stripCount; i++)
            {
                var row0 = (int)Math.Min(i * (long)rowsPerStrip, _height);
                var rows = (int)Math.Min(rowsPerStrip, (uint)(_height - row0));
                Blit(image, InflateRegion(offsets[i], counts[i], rows, rowBytes, compression, predictor), rows, rowBytes, 0, row0, _width);
            }
        }

        private void DecodeTiles(Image image, int compression, int predictor)
        {
            var tileWidth = ReadDimension(322);
            var tileHeight = ReadDimension(323);
            if ((long)tileWidth * tileHeight > MaxPixels)
                throw new ImageException("TIFF tile is too large.");
            var across = (_width + tileWidth - 1) / tileWidth;
            var down = (_height + tileHeight - 1) / tileHeight;
            var offsets = ReadArray(324);
            var counts = ReadArray(325);
            if (offsets.Length < across * down || counts.Length < across * down)
                throw new ImageException("TIFF tile table is too short.");

            var rowBytes = RowBytes(tileWidth);
            for (var ty = 0; ty < down; ty++)
            {
                for (var tx = 0; tx < across; tx++)
                {
                    var index = ty * across + tx;
                    var raw = InflateRegion(offsets[index], counts[index], tileHeight, rowBytes, compression, predictor);
                    var originX = tx * tileWidth;
                    var originY = ty * tileHeight;
                    Blit(
                        image,
                        raw,
                        Math.Min(tileHeight, _height - originY),
                        rowBytes,
                        originX,
                        originY,
                        Math.Min(tileWidth, _width - originX)
                    );
                }
            }
        }

        private byte[] InflateRegion(int offset, int length, int rows, int rowBytes, int compression, int predictor)
        {
            var expected = checked(rows * rowBytes);
            var raw = compression switch
            {
                CompressionNone => CopyRaw(offset, length, expected),
                CompressionPackBits => UnpackPackBits(offset, length, rows, rowBytes),
                CompressionLzw => Lzw(offset, length, expected),
                _ => Inflate(offset, length, expected),
            };
            if (predictor == 2)
                UndoPredictor(raw, rows, rowBytes, _samples);
            return raw;
        }

        private byte[] CopyRaw(int offset, int length, int expected)
        {
            if (length < expected)
                throw new ImageException("Truncated TIFF strip.");
            Need(offset, expected);
            var output = new byte[expected];
            data.AsSpan(offset, expected).CopyTo(output);
            return output;
        }

        private byte[] Inflate(int offset, int length, int expected)
        {
            Need(offset, length);
            using var input = new MemoryStream(data, offset, length, writable: false);
            using var zlib = new ZLibStream(input, CompressionMode.Decompress);
            var output = new byte[expected];
            var read = 0;
            while (read < expected)
            {
                var n = zlib.Read(output.AsSpan(read));
                if (n == 0)
                    throw new ImageException("Truncated TIFF Deflate data.");
                read += n;
            }

            return output;
        }

        private byte[] UnpackPackBits(int offset, int length, int rows, int rowBytes)
        {
            Need(offset, length);
            var src = data.AsSpan(offset, length);
            var output = new byte[rows * rowBytes];
            var ip = 0;
            var op = 0;
            for (var row = 0; row < rows; row++)
            {
                var rowEnd = op + rowBytes;
                var nops = 0;
                while (op < rowEnd)
                {
                    if (ip >= src.Length)
                        throw new ImageException("Truncated TIFF PackBits data.");
                    var n = src[ip++];
                    if (n == 128)
                    {
                        if (++nops > rowBytes + 8)
                            throw new ImageException("TIFF PackBits data is corrupt.");
                        continue;
                    }

                    if (n < 128)
                    {
                        var count = n + 1;
                        if (op + count > rowEnd || ip + count > src.Length)
                            throw new ImageException("Truncated TIFF PackBits data.");
                        src.Slice(ip, count).CopyTo(output.AsSpan(op));
                        ip += count;
                        op += count;
                    }
                    else
                    {
                        var count = 257 - n;
                        if (ip >= src.Length || op + count > rowEnd)
                            throw new ImageException("Truncated TIFF PackBits data.");
                        output.AsSpan(op, count).Fill(src[ip++]);
                        op += count;
                    }
                }
            }

            return output;
        }

        private byte[] Lzw(int offset, int length, int expected)
        {
            Need(offset, length);
            var end = offset + length;
            var output = new byte[expected];
            var prefix = new int[4096];
            var suffix = new byte[4096];
            var bitPos = 0;
            var codeSize = 9;
            var next = 258;
            var old = -1;
            for (var i = 0; i < 256; i++)
                suffix[i] = (byte)i;

            int ReadCode()
            {
                var code = 0;
                for (var i = 0; i < codeSize; i++)
                {
                    var byteIndex = offset + (bitPos >> 3);
                    if (byteIndex >= end)
                        throw new ImageException("Truncated TIFF LZW data.");
                    var bit = (data[byteIndex] >> (7 - (bitPos & 7))) & 1;
                    code = (code << 1) | bit;
                    bitPos++;
                }

                return code;
            }

            void Reset()
            {
                codeSize = 9;
                next = 258;
                old = -1;
            }

            var outPos = 0;
            Span<byte> reverse = stackalloc byte[4097];
            while (outPos < expected)
            {
                var code = ReadCode();
                if (code == 257)
                    break;
                if (code == 256)
                {
                    Reset();
                    continue;
                }

                if (old < 0)
                {
                    if (code > 255)
                        throw new ImageException("TIFF LZW data is corrupt.");
                    output[outPos++] = (byte)code;
                    old = code;
                    continue;
                }

                if (code > next)
                    throw new ImageException("TIFF LZW data is corrupt.");

                byte first;
                int spelled;
                if (code == next)
                {
                    spelled = Spell(old, reverse);
                    first = reverse[spelled - 1];
                    for (var i = spelled; i > 0; i--)
                        reverse[i] = reverse[i - 1];
                    reverse[0] = first;
                    spelled++;
                }
                else
                {
                    spelled = Spell(code, reverse);
                    first = reverse[spelled - 1];
                }

                if (outPos + spelled > expected)
                    throw new ImageException("TIFF LZW data exceeds its strip.");
                for (var i = spelled - 1; i >= 0; i--)
                    output[outPos++] = reverse[i];

                if (next < 4096)
                {
                    prefix[next] = old;
                    suffix[next] = first;
                    next++;
                    if (codeSize < 12 && next > (1 << codeSize) - 1)
                        codeSize++;
                }

                old = code;
            }

            if (outPos != expected)
                throw new ImageException("Truncated TIFF LZW data.");
            return output;

            int Spell(int code, Span<byte> stack)
            {
                var n = 0;
                var guard = 0;
                while (code >= 256)
                {
                    if ((uint)code >= 4096u || ++guard > 4096)
                        throw new ImageException("TIFF LZW data is corrupt.");
                    stack[n++] = suffix[code];
                    code = prefix[code];
                }

                stack[n++] = (byte)code;
                return n;
            }
        }

        private void Blit(Image image, byte[] raw, int rows, int rowBytes, int originX, int originY, int copyWidth)
        {
            for (var row = 0; row < rows; row++)
            {
                var y = originY + row;
                if ((uint)y >= (uint)_height)
                    continue;
                var src = raw.AsSpan(row * rowBytes, rowBytes);
                var dest = image.GetRowSpan(y);
                for (var x = 0; x < copyWidth; x++)
                {
                    var dx = originX + x;
                    if ((uint)dx >= (uint)_width)
                        continue;
                    dest[dx] = ReadColor(src, x);
                }
            }
        }

        private Color ReadColor(ReadOnlySpan<byte> row, int x)
        {
            if (_photometric == 3)
            {
                var index = Sample(row, x, 0);
                if (_palette == null || (uint)index >= (uint)_palette.Length)
                    throw new ImageException("TIFF palette index is out of range.");
                return _palette[index];
            }

            if (_photometric is 0 or 1)
            {
                var gray = Scale(Sample(row, x, 0));
                if (_photometric == 0)
                    gray = (byte)(255 - gray);
                var alpha = _samples == 2 ? Scale(Sample(row, x, 1)) : (byte)255;
                return new Color(gray, gray, gray, alpha);
            }

            return new Color(
                Scale(Sample(row, x, 0)),
                Scale(Sample(row, x, 1)),
                Scale(Sample(row, x, 2)),
                _samples >= 4 ? Scale(Sample(row, x, 3)) : (byte)255
            );
        }

        private int Sample(ReadOnlySpan<byte> row, int pixel, int sample)
        {
            var bitIndex = (pixel * _samples + sample) * _bits;
            var byteIndex = bitIndex >> 3;
            if ((uint)byteIndex >= (uint)row.Length)
                throw new ImageException("Truncated TIFF pixel data.");
            if (_bits == 8)
                return row[byteIndex];
            if (_bits == 4)
                return ((bitIndex >> 2) & 1) == 0 ? row[byteIndex] >> 4 : row[byteIndex] & 0x0F;
            return (row[byteIndex] >> (7 - (bitIndex & 7))) & 1;
        }

        private byte Scale(int value) =>
            _bits switch
            {
                1 => (byte)(value == 0 ? 0 : 255),
                4 => (byte)(value * 17),
                _ => (byte)value,
            };

        private void ValidatePhotometric()
        {
            switch (_photometric)
            {
                case 0:
                case 1:
                    if (_samples is not (1 or 2))
                        throw new ImageException("TIFF greyscale sample count is not supported.");
                    return;
                case 2:
                    if (_samples is not (3 or 4))
                        throw new ImageException("TIFF RGB sample count is not supported.");
                    return;
                case 3:
                    if (_samples != 1)
                        throw new ImageException("TIFF palette sample count is not supported.");
                    return;
                default:
                    throw new ImageException($"TIFF photometric {_photometric} is not supported.");
            }
        }

        private int ReadBits()
        {
            if (!_fields.TryGetValue(258, out var field) || field.Count < 1)
                throw new ImageException("TIFF is missing bits per sample.");
            var first = (int)ReadNumber(field, 0);
            if (first is not (1 or 4 or 8))
                throw new ImageException($"TIFF bits per sample {first} is not supported.");
            if (field.Count != 1 && field.Count != _samples)
                throw new ImageException("TIFF bits per sample does not match the sample count.");
            for (var i = 0; i < field.Count; i++)
            {
                if (ReadNumber(field, i) != (uint)first)
                    throw new ImageException("TIFF bits per sample must be equal across channels.");
            }

            return first;
        }

        private Color[] LoadPalette()
        {
            var colors = 1 << _bits;
            if (!_fields.TryGetValue(320, out var field) || field.Count < colors * 3)
                throw new ImageException("TIFF palette image is missing a color map.");
            var start = ValueStart(field);
            var palette = new Color[colors];
            for (var i = 0; i < colors; i++)
            {
                var red = ReadU16(start + i * 2) >> 8;
                var green = ReadU16(start + (colors + i) * 2) >> 8;
                var blue = ReadU16(start + (colors * 2 + i) * 2) >> 8;
                palette[i] = new Color((byte)red, (byte)green, (byte)blue, 255);
            }

            return palette;
        }

        private void RejectSampleFormat()
        {
            if (!_fields.TryGetValue(339, out var field))
                return;
            for (var i = 0; i < field.Count; i++)
            {
                var format = ReadNumber(field, i);
                if (format != 1)
                    throw new ImageException($"TIFF sample format {format} is not supported.");
            }
        }

        private int ReadDimension(int tag)
        {
            var value = Require(tag);
            if (value is 0 or > int.MaxValue)
                throw new ImageException("TIFF dimensions are invalid.");
            return (int)value;
        }

        private uint Require(int tag)
        {
            if (!_fields.TryGetValue(tag, out var field) || field.Count < 1)
                throw new ImageException($"TIFF is missing tag {tag}.");
            return ReadNumber(field, 0);
        }

        private uint Optional(int tag, uint fallback) =>
            _fields.TryGetValue(tag, out var field) && field.Count > 0 ? ReadNumber(field, 0) : fallback;

        private int[] ReadArray(int tag)
        {
            if (!_fields.TryGetValue(tag, out var field) || field.Count is <= 0 or > 1_000_000)
                throw new ImageException($"TIFF tag {tag} is missing or too large.");
            var values = new int[field.Count];
            for (var i = 0; i < field.Count; i++)
            {
                var number = ReadNumber(field, i);
                if (number > int.MaxValue)
                    throw new ImageException("TIFF offset is too large.");
                values[i] = (int)number;
            }

            return values;
        }

        private uint ReadNumber(Field field, int index)
        {
            var size = TypeSize(field.Type);
            var start = ValueStart(field);
            var at = checked(start + index * size);
            return field.Type switch
            {
                1 or 6 or 7 => data[Need(at, 1)],
                3 or 8 => ReadU16(at),
                4 or 9 => ReadU32(at),
                _ => throw new ImageException($"TIFF field type {field.Type} is not supported."),
            };
        }

        private int ValueStart(Field field)
        {
            var size = TypeSize(field.Type);
            var bytes = (long)field.Count * size;
            if (bytes <= 4)
                return field.ValueAt;
            return ReadOffset(field.ValueAt);
        }

        private static int TypeSize(int type) =>
            type switch
            {
                1 or 2 or 6 or 7 => 1,
                3 or 8 => 2,
                4 or 9 or 11 => 4,
                5 or 10 or 12 => 8,
                _ => throw new ImageException($"TIFF field type {type} is not supported."),
            };

        private static void UndoPredictor(byte[] raw, int rows, int rowBytes, int samples)
        {
            for (var row = 0; row < rows; row++)
            {
                var start = row * rowBytes;
                for (var i = samples; i < rowBytes; i++)
                    raw[start + i] = (byte)(raw[start + i] + raw[start + i - samples]);
            }
        }

        private static int RowBytes(int pixelWidth, int samples, int bits) =>
            (int)(((long)pixelWidth * samples * bits + 7) / 8);

        private int RowBytes(int pixelWidth) => RowBytes(pixelWidth, _samples, _bits);

        private int ReadOffset(int offset)
        {
            var value = ReadU32(offset);
            if (value > int.MaxValue)
                throw new ImageException("TIFF offset is too large.");
            return (int)value;
        }

        private ushort ReadU16(int offset)
        {
            Need(offset, 2);
            return _little
                ? BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(offset))
                : BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(offset));
        }

        private uint ReadU32(int offset)
        {
            Need(offset, 4);
            return _little
                ? BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(offset))
                : BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(offset));
        }

        private int Need(int offset, int size)
        {
            if (offset < 0 || size < 0 || (long)offset + size > data.Length)
                throw new ImageException("Truncated TIFF image.");
            return offset;
        }
    }
}
