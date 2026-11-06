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
using ChromaFx.Core.Exceptions;
using ChromaFx.IO.Formats.Tga;
using Xunit;

namespace ChromaFx.Tests;

/// <summary>
/// Managed TGA and baseline TIFF round-trips, including odd widths.
/// </summary>
public class ManagedCodecTests
{
    [Fact]
    public void TgaRoundTrip_PreservesOddWidthAndAlpha()
    {
        var src = Sample(17, 3, alpha: true);
        using var stream = new MemoryStream();
        src.Save(stream, new TgaEncoder());
        stream.Position = 0;
        var decoded = new TgaFormat().Decode(stream);
        AssertPixels(src, decoded);
        Assert.Equal(src[0, 0], Image.Load(stream.ToArray())[0, 0]);
    }

    [Fact]
    public void TgaDecodesRleTruecolor()
    {
        var bytes = new byte[18 + 4];
        bytes[2] = 10;
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(12), 2);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(14), 1);
        bytes[16] = 24;
        bytes[17] = 0x20;
        bytes[18] = 0x81;
        bytes[19] = 255;
        bytes[20] = 0;
        bytes[21] = 0;
        var image = Image.Load(bytes);
        Assert.Equal(new Color(0, 0, 255, 255), image[0, 0]);
        Assert.Equal(image[0, 0], image[1, 0]);
    }

    [Fact]
    public void TgaDecodesBottomOriginAnd16Bit()
    {
        var bottom = new byte[18 + 6];
        bottom[2] = 2;
        BinaryPrimitives.WriteUInt16LittleEndian(bottom.AsSpan(12), 1);
        BinaryPrimitives.WriteUInt16LittleEndian(bottom.AsSpan(14), 2);
        bottom[16] = 24;
        bottom[18] = 0;
        bottom[19] = 0;
        bottom[20] = 255;
        bottom[21] = 0;
        bottom[22] = 255;
        bottom[23] = 0;
        var image = Image.Load(bottom);
        Assert.Equal(new Color(0, 255, 0, 255), image[0, 0]);
        Assert.Equal(new Color(255, 0, 0, 255), image[0, 1]);

        var packed = new byte[18 + 2];
        packed[2] = 2;
        BinaryPrimitives.WriteUInt16LittleEndian(packed.AsSpan(12), 1);
        BinaryPrimitives.WriteUInt16LittleEndian(packed.AsSpan(14), 1);
        packed[16] = 16;
        packed[17] = 1;
        BinaryPrimitives.WriteUInt16LittleEndian(packed.AsSpan(18), 0x7C00);
        Assert.Equal(new Color(255, 0, 0, 0), Image.Load(packed)[0, 0]);
    }

    [Fact]
    public void TgaDecodesIndexedAndRejectsPngHeader()
    {
        var indexed = new byte[18 + 6 + 1];
        indexed[1] = 1;
        indexed[2] = 1;
        BinaryPrimitives.WriteUInt16LittleEndian(indexed.AsSpan(5), 2);
        indexed[7] = 24;
        BinaryPrimitives.WriteUInt16LittleEndian(indexed.AsSpan(12), 1);
        BinaryPrimitives.WriteUInt16LittleEndian(indexed.AsSpan(14), 1);
        indexed[16] = 8;
        indexed[21] = 0;
        indexed[22] = 0;
        indexed[23] = 255;
        indexed[24] = 1;
        Assert.Equal(new Color(255, 0, 0, 255), Image.Load(indexed)[0, 0]);

        var png = new byte[18];
        png[0] = 0x89;
        png[1] = 0x50;
        Assert.False(new TgaFormat().CanDecode(png));
    }

    [Fact]
    public void TiffRoundTrip_PreservesOddWidthAndAlpha()
    {
        var src = Sample(17, 3, alpha: true);
        using var stream = new MemoryStream();
        src.Save(stream, FileFormats.Tiff);
        var decoded = Image.Load(stream.ToArray());
        AssertPixels(src, decoded);
    }

    [Fact]
    public void TiffDecodesPackBitsDeflateAndLzw()
    {
        var pack = Tiff(
            width: 5,
            height: 1,
            photometric: 1,
            compression: 32773,
            strip: [0x00, 0x01, 0xFD, 0x02]
        );
        var packed = Image.Load(pack);
        Assert.Equal(new Color(1, 1, 1, 255), packed[0, 0]);
        Assert.Equal(new Color(2, 2, 2, 255), packed[4, 0]);

        var raw = new byte[] { 9, 8, 7, 6 };
        var deflated = Image.Load(
            Tiff(width: 1, height: 1, photometric: 2, samples: 4, compression: 8, strip: Zlib(raw))
        );
        Assert.Equal(new Color(9, 8, 7, 6), deflated[0, 0]);

        var lzw = Image.Load(
            Tiff(
                width: 1,
                height: 1,
                photometric: 1,
                compression: 5,
                strip: [0x80, 0x2A, 0xE0, 0x20]
            )
        );
        Assert.Equal(new Color(0xAB, 0xAB, 0xAB, 255), lzw[0, 0]);

        var deltas = LzwEncode([10, 10, 10]);
        var predicted = Image.Load(
            Tiff(width: 3, height: 1, photometric: 1, compression: 5, predictor: 2, strip: deltas)
        );
        Assert.Equal(new Color(10, 10, 10, 255), predicted[0, 0]);
        Assert.Equal(new Color(20, 20, 20, 255), predicted[1, 0]);
        Assert.Equal(new Color(30, 30, 30, 255), predicted[2, 0]);
    }

    [Fact]
    public void TiffDecodesPaletteBitsEndianAndTiles()
    {
        ushort[] map = [0, 0xFFFF, 0, 0, 0, 0];
        var palette = Image.Load(
            Tiff(width: 1, height: 1, photometric: 3, bits: 1, strip: [0x80], colorMap: map)
        );
        Assert.Equal(new Color(255, 0, 0, 255), palette[0, 0]);

        var nibbles = Image.Load(Tiff(width: 2, height: 1, photometric: 1, bits: 4, strip: [0x10]));
        Assert.Equal(new Color(17, 17, 17, 255), nibbles[0, 0]);
        Assert.Equal(new Color(0, 0, 0, 255), nibbles[1, 0]);

        var bilevel = Image.Load(Tiff(width: 8, height: 1, photometric: 0, bits: 1, strip: [0x80]));
        Assert.Equal(new Color(0, 0, 0, 255), bilevel[0, 0]);
        Assert.Equal(new Color(255, 255, 255, 255), bilevel[7, 0]);

        var big = Image.Load(
            Tiff(width: 1, height: 1, photometric: 1, strip: [200], little: false)
        );
        Assert.Equal(new Color(200, 200, 200, 255), big[0, 0]);

        var tiled = Image.Load(
            Tiff(
                width: 2,
                height: 2,
                photometric: 1,
                tiles: [[1, 2], [3, 4]],
                tileWidth: 2,
                tileHeight: 1
            )
        );
        Assert.Equal(new Color(1, 1, 1, 255), tiled[0, 0]);
        Assert.Equal(new Color(2, 2, 2, 255), tiled[1, 0]);
        Assert.Equal(new Color(4, 4, 4, 255), tiled[1, 1]);
    }

    [Fact]
    public void TiffRejectsJpegCompressionAndLaterIfdIsIgnored()
    {
        var jpeg = Tiff(width: 1, height: 1, photometric: 1, compression: 6, strip: [1]);
        Assert.Throws<ImageException>(() => Image.Load(jpeg));

        var first = Tiff(width: 1, height: 1, photometric: 1, strip: [40], nextIfd: 4);
        Assert.Equal(new Color(40, 40, 40, 255), Image.Load(first)[0, 0]);
    }

    private static Image Sample(int width, int height, bool alpha)
    {
        var image = new Image(width, height);
        for (var y = 0; y < height; y++)
        {
            var row = image.GetRowSpan(y);
            for (var x = 0; x < width; x++)
            {
                row[x] = new Color(
                    (byte)(20 + x * 3),
                    (byte)(30 + y * 5),
                    (byte)(200 - x),
                    alpha ? (byte)(10 + y) : (byte)255
                );
            }
        }

        return image;
    }

    private static void AssertPixels(Image expected, Image actual)
    {
        Assert.Equal(expected.Width, actual.Width);
        Assert.Equal(expected.Height, actual.Height);
        for (var y = 0; y < expected.Height; y++)
        {
            var exp = expected.GetRowSpan(y);
            var got = actual.GetRowSpan(y);
            for (var x = 0; x < expected.Width; x++)
                Assert.Equal(exp[x], got[x]);
        }
    }

    private static byte[] Zlib(byte[] raw)
    {
        using var ms = new MemoryStream();
        using (var zlib = new ZLibStream(ms, CompressionLevel.Fastest, leaveOpen: true))
            zlib.Write(raw);
        return ms.ToArray();
    }

    private static byte[] LzwEncode(ReadOnlySpan<byte> input)
    {
        var table = new Dictionary<string, int>();
        for (var i = 0; i < 256; i++)
            table[new string((char)i, 1)] = i;

        var packed = new List<byte>();
        var bitBuffer = 0;
        var bitCount = 0;
        var next = 258;
        var codeSize = 9;

        void Emit(int code)
        {
            for (var shift = codeSize - 1; shift >= 0; shift--)
            {
                bitBuffer = (bitBuffer << 1) | ((code >> shift) & 1);
                bitCount++;
                if (bitCount != 8)
                    continue;
                packed.Add((byte)bitBuffer);
                bitBuffer = 0;
                bitCount = 0;
            }
        }

        Emit(256);
        var w = new string((char)input[0], 1);
        for (var i = 1; i < input.Length; i++)
        {
            var key = w + (char)input[i];
            if (table.ContainsKey(key))
            {
                w = key;
                continue;
            }

            Emit(table[w]);
            if (next < 4096)
            {
                table[key] = next++;
                if (codeSize < 12 && next > (1 << codeSize) - 1)
                    codeSize++;
            }

            w = new string((char)input[i], 1);
        }

        Emit(table[w]);
        Emit(257);
        if (bitCount > 0)
            packed.Add((byte)(bitBuffer << (8 - bitCount)));
        return packed.ToArray();
    }

    private readonly record struct Field(ushort Tag, ushort Type, uint[] Values, int Blob);

    private static byte[] Tiff(
        int width,
        int height,
        int photometric,
        byte[] strip = null,
        byte[][] tiles = null,
        int bits = 8,
        int samples = 1,
        int compression = 1,
        int predictor = 1,
        bool little = true,
        ushort[] colorMap = null,
        int tileWidth = 0,
        int tileHeight = 0,
        int nextIfd = 0
    )
    {
        var blobs = new List<byte[]>();
        var fields = new List<Field>();
        int AddBlob(byte[] blob)
        {
            blobs.Add(blob);
            return blobs.Count - 1;
        }

        void Add(ushort tag, ushort type, params uint[] values) =>
            fields.Add(new Field(tag, type, values, -1));

        Add(256, 4, (uint)width);
        Add(257, 4, (uint)height);
        if (samples == 1)
            Add(258, 3, (uint)bits);
        else
        {
            var bps = new byte[samples * 2];
            for (var i = 0; i < samples; i++)
                Write16(bps.AsSpan(i * 2), (ushort)bits, little);
            fields.Add(new Field(258, 3, [], AddBlob(bps)));
        }

        Add(259, 3, (uint)compression);
        Add(262, 3, (uint)photometric);
        if (samples != 1)
            Add(277, 3, (uint)samples);
        if (predictor != 1)
            Add(317, 3, (uint)predictor);
        if (colorMap != null)
        {
            var map = new byte[colorMap.Length * 2];
            for (var i = 0; i < colorMap.Length; i++)
                Write16(map.AsSpan(i * 2), colorMap[i], little);
            fields.Add(new Field(320, 3, [], AddBlob(map)));
        }

        var payloadIndex = new List<int>();
        var offsetBlob = -1;
        if (tiles != null)
        {
            Add(322, 4, (uint)tileWidth);
            Add(323, 4, (uint)tileHeight);
            offsetBlob = AddBlob(new byte[tiles.Length * 4]);
            var counts = new byte[tiles.Length * 4];
            for (var i = 0; i < tiles.Length; i++)
            {
                payloadIndex.Add(AddBlob(tiles[i]));
                Write32(counts.AsSpan(i * 4), (uint)tiles[i].Length, little);
            }

            fields.Add(new Field(324, 4, [], offsetBlob));
            if (tiles.Length == 1)
                Add(325, 4, (uint)tiles[0].Length);
            else
                fields.Add(new Field(325, 4, [], AddBlob(counts)));
        }
        else
        {
            strip ??= [0];
            payloadIndex.Add(AddBlob(strip));
            fields.Add(new Field(273, 4, [uint.MaxValue], -1));
            Add(278, 4, (uint)height);
            Add(279, 4, (uint)strip.Length);
        }

        fields.Sort((a, b) => a.Tag.CompareTo(b.Tag));
        var cursor = 8 + 2 + fields.Count * 12 + 4;
        var blobAt = new int[blobs.Count];
        for (var i = 0; i < blobs.Count; i++)
        {
            blobAt[i] = cursor;
            cursor += blobs[i].Length;
        }

        if (offsetBlob >= 0)
        {
            var bytes = blobs[offsetBlob];
            for (var i = 0; i < payloadIndex.Count; i++)
                Write32(bytes.AsSpan(i * 4), (uint)blobAt[payloadIndex[i]], little);
        }

        var output = new byte[cursor];
        output[0] = output[1] = (byte)(little ? 'I' : 'M');
        Write16(output.AsSpan(2), 42, little);
        Write32(output.AsSpan(4), 8, little);
        Write16(output.AsSpan(8), (ushort)fields.Count, little);
        var entry = 10;
        foreach (var field in fields)
        {
            Write16(output.AsSpan(entry), field.Tag, little);
            Write16(output.AsSpan(entry + 2), field.Type, little);
            var count = field.Blob >= 0 ? (uint)(blobs[field.Blob].Length / TypeSize(field.Type)) : (uint)field.Values.Length;
            Write32(output.AsSpan(entry + 4), count, little);
            if (field.Tag == 273)
                Write32(output.AsSpan(entry + 8), (uint)blobAt[payloadIndex[0]], little);
            else if (field.Blob >= 0)
                Write32(output.AsSpan(entry + 8), (uint)blobAt[field.Blob], little);
            else if (field.Values.Length * TypeSize(field.Type) > 4)
                throw new InvalidOperationException($"TIFF test tag {field.Tag} does not fit inline.");
            else if (field.Type == 3)
                Write16(output.AsSpan(entry + 8), (ushort)field.Values[0], little);
            else
                Write32(output.AsSpan(entry + 8), field.Values[0], little);

            entry += 12;
        }

        Write32(output.AsSpan(entry), (uint)nextIfd, little);
        for (var i = 0; i < blobs.Count; i++)
            blobs[i].CopyTo(output.AsSpan(blobAt[i]));
        return output;
    }

    private static int TypeSize(ushort type) => type == 3 ? 2 : 4;

    private static void Write16(Span<byte> dest, ushort value, bool little)
    {
        if (little)
            BinaryPrimitives.WriteUInt16LittleEndian(dest, value);
        else
            BinaryPrimitives.WriteUInt16BigEndian(dest, value);
    }

    private static void Write32(Span<byte> dest, uint value, bool little)
    {
        if (little)
            BinaryPrimitives.WriteUInt32LittleEndian(dest, value);
        else
            BinaryPrimitives.WriteUInt32BigEndian(dest, value);
    }
}
