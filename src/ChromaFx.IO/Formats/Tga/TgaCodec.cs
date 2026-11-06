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
using ChromaFx.Core;
using ChromaFx.Core.Colors;
using ChromaFx.Core.Exceptions;

namespace ChromaFx.IO.Formats.Tga;

internal static class TgaCodec
{
    private const int MaxPixels = 32_000_000;

    internal static bool LooksLike(ReadOnlySpan<byte> header)
    {
        if (header.Length < 18)
            return false;

        var colorMapType = header[1];
        var imageType = header[2];
        var depth = header[16];
        var descriptor = header[17];
        if ((descriptor & 0xC0) != 0)
            return false;

        var width = BinaryPrimitives.ReadUInt16LittleEndian(header[12..]);
        var height = BinaryPrimitives.ReadUInt16LittleEndian(header[14..]);
        if (width == 0 || height == 0)
            return false;

        var mapped = imageType is 1 or 9;
        var trueColor = imageType is 2 or 10;
        var grey = imageType is 3 or 11;
        if (!mapped && !trueColor && !grey)
            return false;

        if (mapped)
        {
            if (colorMapType != 1 || depth != 8)
                return false;
            var mapDepth = header[7];
            if (mapDepth is not (15 or 16 or 24 or 32))
                return false;
            if (BinaryPrimitives.ReadUInt16LittleEndian(header[5..]) == 0)
                return false;
            return true;
        }

        if (colorMapType != 0)
            return false;
        if (grey)
            return depth == 8;
        return depth is 15 or 16 or 24 or 32;
    }

    internal static Image Decode(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        var data = ReadAll(stream);
        if (!LooksLike(data))
            throw new ImageException("Not a TGA image.");

        var idLength = data[0];
        var imageType = data[2];
        var mapStart = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(3));
        var mapLength = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(5));
        var mapDepth = data[7];
        var width = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(12));
        var height = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(14));
        var depth = data[16];
        var descriptor = data[17];
        if ((long)width * height > MaxPixels)
            throw new ImageException("TGA image is too large.");

        var mapped = imageType is 1 or 9;
        var rle = imageType is 9 or 10 or 11;
        var offset = 18 + idLength;
        if (mapped)
            offset += mapLength * BytesPerPixel(mapDepth);
        if (offset > data.Length)
            throw new ImageException("Truncated TGA image.");

        var pixelCount = width * height;
        var tight = new byte[pixelCount * 4];
        var cursor = offset;
        var written = 0;
        while (written < pixelCount)
        {
            var count = 1;
            var run = false;
            if (rle)
            {
                if (cursor >= data.Length)
                    throw new ImageException("Truncated TGA image.");
                var packet = data[cursor++];
                count = (packet & 0x7F) + 1;
                run = (packet & 0x80) != 0;
            }

            if (written + count > pixelCount)
                throw new ImageException("TGA image data exceeds its dimensions.");

            if (run)
            {
                ReadPixel(data, ref cursor, depth, descriptor, mapped, mapStart, mapLength, mapDepth, out var r, out var g, out var b, out var a);
                for (var i = 0; i < count; i++)
                    WritePixel(tight, written++, r, g, b, a);
            }
            else
            {
                for (var i = 0; i < count; i++)
                {
                    ReadPixel(data, ref cursor, depth, descriptor, mapped, mapStart, mapLength, mapDepth, out var r, out var g, out var b, out var a);
                    WritePixel(tight, written++, r, g, b, a);
                }
            }
        }

        var top = (descriptor & 0x20) != 0;
        var right = (descriptor & 0x10) != 0;
        var image = new Image(width, height);
        for (var y = 0; y < height; y++)
        {
            var srcY = top ? y : height - 1 - y;
            var row = image.GetRowSpan(y);
            for (var x = 0; x < width; x++)
            {
                var srcX = right ? width - 1 - x : x;
                var i = (srcY * width + srcX) * 4;
                row[x] = new Color(tight[i], tight[i + 1], tight[i + 2], tight[i + 3]);
            }
        }

        return image;
    }

    internal static void Encode(BinaryWriter writer, Image image)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(image);
        if (image.Width is <= 0 or > ushort.MaxValue || image.Height is <= 0 or > ushort.MaxValue)
            throw new ImageException("TGA dimensions must fit in 16 bits.");

        Span<byte> header = stackalloc byte[18];
        header[2] = 2;
        BinaryPrimitives.WriteUInt16LittleEndian(header[12..], (ushort)image.Width);
        BinaryPrimitives.WriteUInt16LittleEndian(header[14..], (ushort)image.Height);
        header[16] = 32;
        header[17] = 0x28;
        writer.Write(header);

        Span<byte> pixel = stackalloc byte[4];
        for (var y = 0; y < image.Height; y++)
        {
            var row = image.GetRowSpan(y);
            for (var x = 0; x < image.Width; x++)
            {
                var color = row[x];
                pixel[0] = color.Blue;
                pixel[1] = color.Green;
                pixel[2] = color.Red;
                pixel[3] = color.Alpha;
                writer.Write(pixel);
            }
        }

        Span<byte> footer = stackalloc byte[26];
        "TRUEVISION-XFILE.\0"u8.CopyTo(footer[8..]);
        writer.Write(footer);
    }

    private static void ReadPixel(
        byte[] data,
        ref int cursor,
        int depth,
        int descriptor,
        bool mapped,
        int mapStart,
        int mapLength,
        int mapDepth,
        out byte r,
        out byte g,
        out byte b,
        out byte a
    )
    {
        if (!mapped)
        {
            ReadStored(data, ref cursor, depth, descriptor, out r, out g, out b, out a);
            return;
        }

        if (cursor >= data.Length)
            throw new ImageException("Truncated TGA image.");
        var index = data[cursor++];
        var slot = index - mapStart;
        if (slot < 0 || slot >= mapLength)
            throw new ImageException("TGA color index is outside the palette.");
        var mapBytes = BytesPerPixel(mapDepth);
        var mapOffset = 18 + data[0] + slot * mapBytes;
        var entry = mapOffset;
        ReadStored(data, ref entry, mapDepth, descriptor, out r, out g, out b, out a);
    }

    private static void ReadStored(
        byte[] data,
        ref int cursor,
        int depth,
        int descriptor,
        out byte r,
        out byte g,
        out byte b,
        out byte a
    )
    {
        var size = BytesPerPixel(depth);
        if (cursor < 0 || size < 0 || (long)cursor + size > data.Length)
            throw new ImageException("Truncated TGA image.");

        switch (depth)
        {
            case 8:
                r = g = b = data[cursor];
                a = 255;
                break;
            case 15:
            case 16:
                var packed = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(cursor));
                b = Expand5(packed & 0x1F);
                g = Expand5((packed >> 5) & 0x1F);
                r = Expand5((packed >> 10) & 0x1F);
                var alphaBits = descriptor & 0x0F;
                a = depth == 16 && alphaBits > 0 ? (byte)((packed & 0x8000) != 0 ? 255 : 0) : (byte)255;
                break;
            case 24:
                b = data[cursor];
                g = data[cursor + 1];
                r = data[cursor + 2];
                a = 255;
                break;
            default:
                b = data[cursor];
                g = data[cursor + 1];
                r = data[cursor + 2];
                a = data[cursor + 3];
                break;
        }

        cursor += size;
    }

    private static void WritePixel(byte[] tight, int index, byte r, byte g, byte b, byte a)
    {
        var o = index * 4;
        tight[o] = r;
        tight[o + 1] = g;
        tight[o + 2] = b;
        tight[o + 3] = a;
    }

    private static int BytesPerPixel(int depth) =>
        depth switch
        {
            8 => 1,
            15 or 16 => 2,
            24 => 3,
            32 => 4,
            _ => throw new ImageException($"TGA pixel depth {depth} is not supported."),
        };

    private static byte Expand5(int value) => (byte)((value << 3) | (value >> 2));

    private static byte[] ReadAll(Stream stream)
    {
        if (stream.CanSeek)
        {
            stream.Position = 0;
            if (stream.Length > int.MaxValue)
                throw new ImageException("TGA image is too large.");
            var buffer = new byte[(int)stream.Length];
            stream.ReadExactly(buffer);
            return buffer;
        }

        using var copy = new MemoryStream();
        stream.CopyTo(copy);
        return copy.ToArray();
    }
}
