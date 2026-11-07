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

namespace ChromaFx.IO.Formats;

/// <summary>
/// Reads TIFF orientation tag 274 and bakes it into pixels.
/// </summary>
internal static class ExifOrientation
{
    internal static int ReadJpeg(Stream stream)
    {
        if (stream is not { CanSeek: true })
            return 1;

        var start = stream.Position;
        try
        {
            Span<byte> marker = stackalloc byte[2];
            if (!TryFill(stream, marker) || marker[0] != 0xFF || marker[1] != 0xD8)
                return 1;

            while (TryReadMarker(stream, out var kind))
            {
                if (kind is 0xD8 or 0xD9 or 0xDA)
                    return 1;
                if (kind == 0x01 || kind is >= 0xD0 and <= 0xD7)
                    continue;
                if (!TryFill(stream, marker))
                    return 1;

                var length = (marker[0] << 8) | marker[1];
                if (length < 2)
                    return 1;
                var payload = length - 2;
                if (kind != 0xE1)
                {
                    stream.Seek(payload, SeekOrigin.Current);
                    continue;
                }

                if (payload is < 14 or > 1024 * 1024)
                {
                    stream.Seek(payload, SeekOrigin.Current);
                    continue;
                }

                var data = new byte[payload];
                stream.ReadExactly(data);
                var orientation = ReadExifBlob(data);
                if (orientation != 0)
                    return orientation;
            }

            return 1;
        }
        catch (EndOfStreamException)
        {
            return 1;
        }
        finally
        {
            stream.Position = start;
        }
    }

    internal static int ReadPng(Stream stream)
    {
        if (stream is not { CanSeek: true })
            return 1;

        var start = stream.Position;
        try
        {
            Span<byte> signature = stackalloc byte[8];
            if (
                !TryFill(stream, signature)
                || signature[0] != 0x89
                || signature[1] != 0x50
                || signature[2] != 0x4E
                || signature[3] != 0x47
            )
                return 1;

            Span<byte> header = stackalloc byte[8];
            while (TryFill(stream, header))
            {
                var length = BinaryPrimitives.ReadInt32BigEndian(header);
                if (length < 0 || length > 16 * 1024 * 1024)
                    return 1;
                var exif =
                    header[4] == (byte)'e'
                    && header[5] == (byte)'X'
                    && header[6] == (byte)'I'
                    && header[7] == (byte)'f';
                var end =
                    header[4] == (byte)'I'
                    && header[5] == (byte)'E'
                    && header[6] == (byte)'N'
                    && header[7] == (byte)'D';
                if (end)
                    return 1;
                if (!exif)
                {
                    stream.Seek(length + 4, SeekOrigin.Current);
                    continue;
                }

                var data = new byte[length];
                stream.ReadExactly(data);
                stream.Seek(4, SeekOrigin.Current);
                var orientation = ReadExifBlob(data);
                return orientation == 0 ? 1 : orientation;
            }

            return 1;
        }
        catch (EndOfStreamException)
        {
            return 1;
        }
        finally
        {
            stream.Position = start;
        }
    }

    internal static int ReadWebP(ReadOnlySpan<byte> data)
    {
        if (data.Length < 12 || data[0] != (byte)'R' || data[8] != (byte)'W' || data[11] != (byte)'P')
            return 1;

        var offset = 12;
        while (offset + 8 <= data.Length)
        {
            var size = BinaryPrimitives.ReadInt32LittleEndian(data[(offset + 4)..]);
            if (size < 0 || offset + 8L + size > data.Length)
                return 1;
            var exif =
                data[offset] == (byte)'E'
                && data[offset + 1] == (byte)'X'
                && data[offset + 2] == (byte)'I'
                && data[offset + 3] == (byte)'F';
            if (exif)
            {
                var orientation = ReadExifBlob(data.Slice(offset + 8, size));
                return orientation == 0 ? 1 : orientation;
            }

            offset += 8 + size + (size & 1);
        }

        return 1;
    }

    internal static Image Apply(Image image, int orientation)
    {
        if (image == null || orientation is < 2 or > 8)
            return image;

        var width = image.Width;
        var height = image.Height;
        var swapped = orientation is 5 or 6 or 7 or 8;
        var dest = new Image(swapped ? height : width, swapped ? width : height);
        for (var y = 0; y < height; y++)
        {
            var row = image.GetRowSpan(y);
            for (var x = 0; x < width; x++)
            {
                var (dx, dy) = Map(orientation, x, y, width, height);
                dest.GetRowSpan(dy)[dx] = row[x];
            }
        }

        return dest;
    }

    private static int ReadExifBlob(ReadOnlySpan<byte> data)
    {
        if (
            data.Length >= 6
            && data[0] == (byte)'E'
            && data[1] == (byte)'x'
            && data[2] == (byte)'i'
            && data[3] == (byte)'f'
            && data[4] == 0
            && data[5] == 0
        )
            data = data[6..];
        if (data.Length < 8)
            return 0;
        if (!(data[0] == (byte)'I' && data[1] == (byte)'I') && !(data[0] == (byte)'M' && data[1] == (byte)'M'))
            return 0;

        var little = data[0] == (byte)'I';
        if (ReadU16(data, 2, little) != 42)
            return 0;
        var ifd = ReadU32(data, 4, little);
        if (ifd > int.MaxValue)
            return 1;
        return ReadOrientation(data, (int)ifd, little);
    }

    private static int ReadOrientation(ReadOnlySpan<byte> data, int ifd, bool little)
    {
        if ((uint)ifd + 2 > (uint)data.Length)
            return 1;
        var count = ReadU16(data, ifd, little);
        if (count > 4096)
            return 1;
        for (var i = 0; i < count; i++)
        {
            var entry = ifd + 2 + i * 12;
            if ((uint)entry + 12 > (uint)data.Length)
                return 1;
            if (ReadU16(data, entry, little) != 274)
                continue;

            var type = ReadU16(data, entry + 2, little);
            if (ReadU32(data, entry + 4, little) < 1)
                return 1;
            var value = type switch
            {
                3 => ReadU16(data, entry + 8, little),
                4 => (int)ReadU32(data, entry + 8, little),
                _ => 1,
            };
            return value is >= 1 and <= 8 ? value : 1;
        }

        return 1;
    }

    private static (int X, int Y) Map(int orientation, int x, int y, int width, int height) =>
        orientation switch
        {
            2 => (width - 1 - x, y),
            3 => (width - 1 - x, height - 1 - y),
            4 => (x, height - 1 - y),
            5 => (y, x),
            6 => (height - 1 - y, x),
            7 => (height - 1 - y, width - 1 - x),
            8 => (y, width - 1 - x),
            _ => (x, y),
        };

    private static bool TryReadMarker(Stream stream, out int marker)
    {
        marker = 0;
        int value;
        do
        {
            value = stream.ReadByte();
            if (value < 0)
                return false;
        } while (value != 0xFF);

        do
        {
            value = stream.ReadByte();
            if (value < 0)
                return false;
        } while (value == 0xFF);

        marker = value;
        return true;
    }

    private static bool TryFill(Stream stream, Span<byte> buffer)
    {
        var read = 0;
        while (read < buffer.Length)
        {
            var n = stream.Read(buffer[read..]);
            if (n == 0)
                return false;
            read += n;
        }

        return true;
    }

    private static int ReadU16(ReadOnlySpan<byte> data, int offset, bool little) =>
        little
            ? BinaryPrimitives.ReadUInt16LittleEndian(data[offset..])
            : BinaryPrimitives.ReadUInt16BigEndian(data[offset..]);

    private static uint ReadU32(ReadOnlySpan<byte> data, int offset, bool little) =>
        little
            ? BinaryPrimitives.ReadUInt32LittleEndian(data[offset..])
            : BinaryPrimitives.ReadUInt32BigEndian(data[offset..]);
}
