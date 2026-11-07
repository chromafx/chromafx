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
using System.Text;
using ChromaFx.Core;
using ChromaFx.Core.Colors;
using ChromaFx.Core.Exceptions;
using ChromaFx.IO.Formats.Png;

namespace ChromaFx.IO.Formats.Ico;

internal static class IcoCodec
{
    private const int MaxPixels = 32_000_000;

    internal static bool LooksLike(ReadOnlySpan<byte> header)
    {
        if (header.Length < 6)
            return false;
        if (BinaryPrimitives.ReadUInt16LittleEndian(header) != 0)
            return false;
        if (BinaryPrimitives.ReadUInt16LittleEndian(header[2..]) != 1)
            return false;
        var count = BinaryPrimitives.ReadUInt16LittleEndian(header[4..]);
        if (count is < 1 or > 256)
            return false;
        if (header.Length < 22)
            return true;

        var bytes = BinaryPrimitives.ReadInt32LittleEndian(header[14..]);
        var offset = BinaryPrimitives.ReadInt32LittleEndian(header[18..]);
        return bytes > 0 && offset >= 6 + count * 16;
    }

    internal static Image Decode(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanSeek)
        {
            using var copy = new MemoryStream();
            stream.CopyTo(copy);
            copy.Position = 0;
            return Decode(copy);
        }

        var file = new byte[stream.Length - stream.Position];
        stream.ReadExactly(file);
        if (!LooksLike(file))
            throw new ImageException("Not an ICO image.");

        var count = BinaryPrimitives.ReadUInt16LittleEndian(file.AsSpan(4));
        var best = -1;
        var bestArea = -1;
        var bestBits = -1;
        for (var i = 0; i < count; i++)
        {
            var entry = 6 + i * 16;
            if (entry + 16 > file.Length)
                throw new ImageException("Truncated ICO directory.");
            var width = file[entry] == 0 ? 256 : file[entry];
            var height = file[entry + 1] == 0 ? 256 : file[entry + 1];
            var bits = BinaryPrimitives.ReadUInt16LittleEndian(file.AsSpan(entry + 6));
            var area = width * height;
            if (area > bestArea || (area == bestArea && bits > bestBits))
            {
                best = i;
                bestArea = area;
                bestBits = bits;
            }
        }

        var chosen = 6 + best * 16;
        var size = BinaryPrimitives.ReadInt32LittleEndian(file.AsSpan(chosen + 8));
        var offset = BinaryPrimitives.ReadInt32LittleEndian(file.AsSpan(chosen + 12));
        if (size <= 0 || offset < 0 || (long)offset + size > file.Length)
            throw new ImageException("ICO image is truncated.");

        var payload = file.AsSpan(offset, size);
        if (
            payload.Length >= 8
            && payload[0] == 0x89
            && payload[1] == 0x50
            && payload[2] == 0x4E
            && payload[3] == 0x47
        )
        {
            using var png = new MemoryStream(payload.ToArray(), writable: false);
            return new PngFormat().Decode(png);
        }

        return DecodeDib(payload);
    }

    internal static void Encode(BinaryWriter writer, Image image)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(image);
        if (image.Width is < 1 or > 256 || image.Height is < 1 or > 256)
            throw new ImageException("ICO images must be between 1×1 and 256×256.");

        using var png = new MemoryStream();
        using (var pngWriter = new BinaryWriter(png, Encoding.UTF8, leaveOpen: true))
            new PngFormat().Encode(pngWriter, image);
        var bytes = png.ToArray();

        writer.Write((ushort)0);
        writer.Write((ushort)1);
        writer.Write((ushort)1);
        writer.Write((byte)(image.Width == 256 ? 0 : image.Width));
        writer.Write((byte)(image.Height == 256 ? 0 : image.Height));
        writer.Write((byte)0);
        writer.Write((byte)0);
        writer.Write((ushort)1);
        writer.Write((ushort)32);
        writer.Write(bytes.Length);
        writer.Write(22);
        writer.Write(bytes);
    }

    private static Image DecodeDib(ReadOnlySpan<byte> payload)
    {
        if (payload.Length < 40)
            throw new ImageException("ICO bitmap header is truncated.");
        var headerSize = BinaryPrimitives.ReadInt32LittleEndian(payload);
        if (headerSize is < 40 || headerSize > payload.Length)
            throw new ImageException("ICO bitmap header is not supported.");

        var width = BinaryPrimitives.ReadInt32LittleEndian(payload[4..]);
        var heightAndMask = BinaryPrimitives.ReadInt32LittleEndian(payload[8..]);
        var planes = BinaryPrimitives.ReadUInt16LittleEndian(payload[12..]);
        var bitCount = BinaryPrimitives.ReadUInt16LittleEndian(payload[14..]);
        var compression = BinaryPrimitives.ReadUInt32LittleEndian(payload[16..]);
        if (width <= 0 || heightAndMask <= 1 || heightAndMask % 2 != 0 || planes != 1)
            throw new ImageException("ICO bitmap size is not supported.");
        if (compression != 0 || bitCount is not (1 or 4 or 8 or 24 or 32))
            throw new ImageException($"ICO bitmap format {bitCount}/{compression} is not supported.");

        var height = heightAndMask / 2;
        if ((long)width * height > MaxPixels)
            throw new ImageException("ICO image is too large.");

        var colors = 0;
        if (bitCount <= 8)
        {
            var used = BinaryPrimitives.ReadUInt32LittleEndian(payload[32..]);
            var max = 1 << bitCount;
            colors = used is >= 1 && used <= max ? (int)used : max;
        }

        var xorOffset = headerSize + colors * 4;
        var xorStride = RowStride(width, bitCount);
        var andStride = RowStride(width, 1);
        var andOffset = xorOffset + xorStride * height;
        if ((long)andOffset + (long)andStride * height > payload.Length)
            throw new ImageException("ICO bitmap is truncated.");

        Span<uint> palette = stackalloc uint[256];
        palette = palette[..colors];
        for (var i = 0; i < colors; i++)
        {
            var p = headerSize + i * 4;
            palette[i] = new Color(payload[p + 2], payload[p + 1], payload[p], 255).IntData;
        }

        var image = new Image(width, height);
        var anyAlpha = false;
        for (var y = 0; y < height; y++)
        {
            var sourceRow = height - 1 - y;
            var rowStart = xorOffset + sourceRow * xorStride;
            var dest = image.GetRowSpan(y);
            for (var x = 0; x < width; x++)
            {
                dest[x] = bitCount switch
                {
                    32 => Read32(payload, rowStart + x * 4),
                    24 => Read24(payload, rowStart + x * 3),
                    _ => FromPalette(payload, palette, rowStart, x, bitCount),
                };
                if (bitCount == 32 && dest[x].Alpha != 0)
                    anyAlpha = true;
            }
        }

        if (bitCount != 32 || !anyAlpha)
            ApplyMask(image, payload, andOffset, andStride, forceOpaque: bitCount == 32);
        return image;
    }

    private static Color Read32(ReadOnlySpan<byte> payload, int offset) =>
        new(payload[offset + 2], payload[offset + 1], payload[offset], payload[offset + 3]);

    private static Color Read24(ReadOnlySpan<byte> payload, int offset) =>
        new(payload[offset + 2], payload[offset + 1], payload[offset]);

    private static Color FromPalette(
        ReadOnlySpan<byte> payload,
        ReadOnlySpan<uint> palette,
        int rowStart,
        int x,
        int bitCount
    )
    {
        var index = bitCount switch
        {
            8 => payload[rowStart + x],
            4 => (payload[rowStart + (x >> 1)] >> ((1 - (x & 1)) * 4)) & 0xF,
            _ => (payload[rowStart + (x >> 3)] >> (7 - (x & 7))) & 1,
        };
        if ((uint)index >= (uint)palette.Length)
            throw new ImageException("ICO palette index is out of range.");
        var color = new Color { IntData = palette[index] };
        return color;
    }

    private static void ApplyMask(
        Image image,
        ReadOnlySpan<byte> payload,
        int andOffset,
        int andStride,
        bool forceOpaque
    )
    {
        for (var y = 0; y < image.Height; y++)
        {
            var sourceRow = image.Height - 1 - y;
            var dest = image.GetRowSpan(y);
            for (var x = 0; x < image.Width; x++)
            {
                var mask = payload[andOffset + sourceRow * andStride + (x >> 3)];
                var transparent = ((mask >> (7 - (x & 7))) & 1) != 0;
                if (transparent)
                    dest[x].Alpha = 0;
                else if (forceOpaque)
                    dest[x].Alpha = 255;
            }
        }
    }

    private static int RowStride(int width, int bits) =>
        (int)(((long)width * bits + 31) / 32 * 4);
}
