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

namespace ChromaFx.IO.Formats;

/// <summary>
/// Extracts and writes EXIF / XMP blobs for JPEG and PNG.
/// </summary>
internal static class ImageMetadataIO
{
    private static readonly byte[] ExifHeader = [0x45, 0x78, 0x69, 0x66, 0x00, 0x00];
    private static readonly byte[] XmpJpegHeader =
        "http://ns.adobe.com/xap/1.0/\0"u8.ToArray();
    private const string XmpKeyword = "XML:com.adobe.xmp";

    /// <summary>
    /// One header walk: capture EXIF/XMP and orientation (tag 274). Restores stream position.
    /// </summary>
    internal static ImageMetadata ReadJpeg(Stream stream, out int orientation)
    {
        var meta = new ImageMetadata();
        orientation = 1;
        if (stream is not { CanSeek: true })
            return meta;

        var start = stream.Position;
        try
        {
            Span<byte> marker = stackalloc byte[2];
            if (!TryFill(stream, marker) || marker[0] != 0xFF || marker[1] != 0xD8)
                return meta;

            while (TryReadMarker(stream, out var kind))
            {
                if (kind is 0xD8 or 0xD9 or 0xDA)
                    break;
                if (kind == 0x01 || kind is >= 0xD0 and <= 0xD7)
                    continue;
                if (!TryFill(stream, marker))
                    break;

                var length = (marker[0] << 8) | marker[1];
                if (length < 2)
                    break;
                var payload = length - 2;
                if (kind != 0xE1 || payload is < 6 or > 16 * 1024 * 1024)
                {
                    stream.Seek(payload, SeekOrigin.Current);
                    continue;
                }

                var data = new byte[payload];
                stream.ReadExactly(data);
                if (StartsWith(data, ExifHeader))
                {
                    if (meta.Exif is null)
                        meta.Exif = data.AsSpan(6).ToArray();
                    if (orientation == 1)
                    {
                        var o = ExifOrientation.ReadExifBlob(data);
                        if (o != 0)
                            orientation = o;
                    }
                }
                else if (StartsWith(data, XmpJpegHeader) && string.IsNullOrEmpty(meta.Xmp))
                {
                    meta.Xmp = Encoding.UTF8.GetString(
                        data,
                        XmpJpegHeader.Length,
                        data.Length - XmpJpegHeader.Length
                    );
                }
            }

            return meta;
        }
        catch (EndOfStreamException)
        {
            return meta;
        }
        finally
        {
            stream.Position = start;
        }
    }

    /// <summary>
    /// One PNG walk: capture eXIf/iTXt XMP and orientation. Restores stream position.
    /// </summary>
    internal static ImageMetadata ReadPng(Stream stream, out int orientation)
    {
        var meta = new ImageMetadata();
        orientation = 1;
        if (stream is not { CanSeek: true })
            return meta;

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
                return meta;

            Span<byte> header = stackalloc byte[8];
            while (TryFill(stream, header))
            {
                var length = BinaryPrimitives.ReadInt32BigEndian(header);
                if (length < 0 || length > 16 * 1024 * 1024)
                    break;
                var type0 = header[4];
                var type1 = header[5];
                var type2 = header[6];
                var type3 = header[7];
                var end = type0 == (byte)'I' && type1 == (byte)'E' && type2 == (byte)'N' && type3 == (byte)'D';
                if (end)
                    break;

                var data = new byte[length];
                if (length > 0)
                    stream.ReadExactly(data);
                stream.Seek(4, SeekOrigin.Current);

                if (
                    type0 == (byte)'e'
                    && type1 == (byte)'X'
                    && type2 == (byte)'I'
                    && type3 == (byte)'f'
                    && length >= 8
                )
                {
                    if (meta.Exif is null)
                        meta.Exif = data;
                    if (orientation == 1)
                    {
                        var o = ExifOrientation.ReadExifBlob(data);
                        if (o != 0)
                            orientation = o;
                    }
                }
                else if (
                    type0 == (byte)'i'
                    && type1 == (byte)'T'
                    && type2 == (byte)'X'
                    && type3 == (byte)'t'
                    && string.IsNullOrEmpty(meta.Xmp)
                    && TryParseItxtXmp(data, out var xmp)
                )
                {
                    meta.Xmp = xmp;
                }
            }

            return meta;
        }
        catch (EndOfStreamException)
        {
            return meta;
        }
        finally
        {
            stream.Position = start;
        }
    }

    internal static void Attach(Image image, ImageMetadata meta)
    {
        if (image is null || meta is null || meta.IsEmpty)
            return;
        image.Metadata = meta;
    }

    internal static void WriteJpeg(BinaryWriter writer, ImageMetadata meta)
    {
        if (meta is null || meta.IsEmpty)
            return;

        if (meta.Exif is { Length: > 0 })
        {
            var tiff = WithOrientationOne((byte[])meta.Exif.Clone());
            var payloadLen = 6 + tiff.Length;
            writer.Write((byte)0xFF);
            writer.Write((byte)0xE1);
            writer.Write((byte)((payloadLen + 2) >> 8));
            writer.Write((byte)((payloadLen + 2) & 0xFF));
            writer.Write(ExifHeader);
            writer.Write(tiff);
        }

        if (!string.IsNullOrEmpty(meta.Xmp))
        {
            var xmpBytes = Encoding.UTF8.GetBytes(meta.Xmp);
            var payloadLen = XmpJpegHeader.Length + xmpBytes.Length;
            writer.Write((byte)0xFF);
            writer.Write((byte)0xE1);
            writer.Write((byte)((payloadLen + 2) >> 8));
            writer.Write((byte)((payloadLen + 2) & 0xFF));
            writer.Write(XmpJpegHeader);
            writer.Write(xmpBytes);
        }
    }

    internal static void WritePngAncillary(BinaryWriter writer, ImageMetadata meta)
    {
        if (meta is null || meta.IsEmpty)
            return;

        if (meta.Exif is { Length: > 0 })
        {
            var tiff = WithOrientationOne((byte[])meta.Exif.Clone());
            WritePngChunk(writer, "eXIf", tiff);
        }

        if (!string.IsNullOrEmpty(meta.Xmp))
        {
            var keyword = Encoding.ASCII.GetBytes(XmpKeyword);
            var text = Encoding.UTF8.GetBytes(meta.Xmp);
            var data = new byte[keyword.Length + 5 + text.Length];
            keyword.CopyTo(data, 0);
            var i = keyword.Length;
            data[i++] = 0; // null
            data[i++] = 0; // compression flag
            data[i++] = 0; // compression method
            data[i++] = 0; // language tag null
            data[i++] = 0; // translated keyword null
            text.CopyTo(data.AsSpan(i));
            WritePngChunk(writer, "iTXt", data);
        }
    }

    private static void WritePngChunk(BinaryWriter writer, string type, byte[] data)
    {
        Span<byte> len = stackalloc byte[4];
        BinaryPrimitives.WriteInt32BigEndian(len, data.Length);
        writer.Write(len);
        var typeBytes = Encoding.ASCII.GetBytes(type);
        writer.Write(typeBytes);
        writer.Write(data);
        var crc = new Png.Format.Helpers.Crc32();
        crc.Update(typeBytes);
        var value = (uint)crc.Update(data);
        Span<byte> crcBytes = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(crcBytes, value);
        writer.Write(crcBytes);
    }

    private static byte[] WithOrientationOne(byte[] tiff)
    {
        if (tiff.Length < 8)
            return tiff;
        if (!(tiff[0] == (byte)'I' && tiff[1] == (byte)'I') && !(tiff[0] == (byte)'M' && tiff[1] == (byte)'M'))
            return tiff;

        var little = tiff[0] == (byte)'I';
        if (ReadU16(tiff, 2, little) != 42)
            return tiff;
        var ifd = (int)ReadU32(tiff, 4, little);
        if ((uint)ifd + 2 > (uint)tiff.Length)
            return tiff;
        var count = ReadU16(tiff, ifd, little);
        for (var i = 0; i < count; i++)
        {
            var entry = ifd + 2 + i * 12;
            if ((uint)entry + 12 > (uint)tiff.Length)
                break;
            if (ReadU16(tiff, entry, little) != 274)
                continue;

            var type = ReadU16(tiff, entry + 2, little);
            if (type == 3)
                WriteU16(tiff, entry + 8, little, 1);
            else if (type == 4)
                WriteU32(tiff, entry + 8, little, 1);
            break;
        }

        return tiff;
    }

    private static bool TryParseItxtXmp(ReadOnlySpan<byte> data, out string xmp)
    {
        xmp = null;
        var keywordBytes = Encoding.ASCII.GetBytes(XmpKeyword);
        if (data.Length < keywordBytes.Length + 5)
            return false;
        if (!data[..keywordBytes.Length].SequenceEqual(keywordBytes) || data[keywordBytes.Length] != 0)
            return false;

        var i = keywordBytes.Length + 1;
        var compression = data[i++];
        i++; // method
        // language tag
        while (i < data.Length && data[i] != 0)
            i++;
        if (i >= data.Length)
            return false;
        i++;
        // translated keyword
        while (i < data.Length && data[i] != 0)
            i++;
        if (i >= data.Length)
            return false;
        i++;
        if (compression != 0)
            return false;

        xmp = Encoding.UTF8.GetString(data[i..]);
        return true;
    }

    private static bool StartsWith(ReadOnlySpan<byte> data, ReadOnlySpan<byte> prefix) =>
        data.Length >= prefix.Length && data[..prefix.Length].SequenceEqual(prefix);

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

    private static void WriteU16(Span<byte> data, int offset, bool little, ushort value)
    {
        if (little)
            BinaryPrimitives.WriteUInt16LittleEndian(data[offset..], value);
        else
            BinaryPrimitives.WriteUInt16BigEndian(data[offset..], value);
    }

    private static void WriteU32(Span<byte> data, int offset, bool little, uint value)
    {
        if (little)
            BinaryPrimitives.WriteUInt32LittleEndian(data[offset..], value);
        else
            BinaryPrimitives.WriteUInt32BigEndian(data[offset..], value);
    }
}
