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
using ChromaFx.Core.Exceptions;
using Xunit;

namespace ChromaFx.Tests;

/// <summary>
/// ICO decode/encode and EXIF orientation baked into pixels.
/// </summary>
public class IcoAndExifTests
{
    [Fact]
    public void IcoRoundTrip_PreservesAlpha()
    {
        var src = new Image(17, 3);
        src.GetRowSpan(0)[0] = new Color(10, 20, 30, 40);
        src.GetRowSpan(2)[16] = new Color(1, 2, 3, 255);
        using var stream = new MemoryStream();
        src.Save(stream, new IcoEncoder());
        var decoded = Image.Load(stream.ToArray());
        Assert.Equal(src.Width, decoded.Width);
        Assert.Equal(src.Height, decoded.Height);
        Assert.Equal(src[0, 0], decoded[0, 0]);
        Assert.Equal(src[16, 2], decoded[16, 2]);
    }

    [Fact]
    public void IcoDecodes32BitDibAndMask()
    {
        var opaque = Dib32(1, 1, [255, 0, 0, 0], [0, 0, 0, 0]);
        var image = Image.Load(Ico(opaque, 1, 1));
        Assert.Equal(new Color(0, 0, 255, 255), image[0, 0]);

        var masked = Dib32(1, 1, [255, 0, 0, 0], [0x80, 0, 0, 0]);
        Assert.Equal(new Color(0, 0, 255, 0), Image.Load(Ico(masked, 1, 1))[0, 0]);
    }

    [Fact]
    public void IcoDecodes8BitPalette()
    {
        var dib = new byte[52];
        BinaryPrimitives.WriteInt32LittleEndian(dib.AsSpan(0), 40);
        BinaryPrimitives.WriteInt32LittleEndian(dib.AsSpan(4), 1);
        BinaryPrimitives.WriteInt32LittleEndian(dib.AsSpan(8), 2);
        BinaryPrimitives.WriteUInt16LittleEndian(dib.AsSpan(12), 1);
        BinaryPrimitives.WriteUInt16LittleEndian(dib.AsSpan(14), 8);
        BinaryPrimitives.WriteUInt32LittleEndian(dib.AsSpan(32), 1);
        dib[40] = 0;
        dib[41] = 0;
        dib[42] = 255;
        var image = Image.Load(Ico(dib, 1, 1));
        Assert.Equal(new Color(255, 0, 0, 255), image[0, 0]);
    }

    [Fact]
    public void IcoDecodeUsesTheLargestImage()
    {
        var small = Dib32(1, 1, [0, 0, 255, 255], [0, 0, 0, 0]);
        var large = Dib32(2, 1, [0, 255, 0, 255, 255, 0, 0, 255], [0, 0, 0, 0]);
        var image = Image.Load(Ico([small, large], [1, 2], [1, 1]));
        Assert.Equal(2, image.Width);
        Assert.Equal(new Color(0, 255, 0, 255), image[0, 0]);
        Assert.Equal(new Color(0, 0, 255, 255), image[1, 0]);
    }

    [Fact]
    public void IcoEncodeRejectsImagesLargerThan256()
    {
        var image = new Image(257, 1);
        Assert.Throws<ImageException>(() => image.Save(new MemoryStream(), FileFormats.Ico));
    }

    [Fact]
    public void JpegExifOrientationFlipsHorizontally()
    {
        var src = new Image(32, 8);
        for (var y = 0; y < src.Height; y++)
        {
            var row = src.GetRowSpan(y);
            for (var x = 0; x < src.Width; x++)
                row[x] = x < 16 ? new Color(220, 0, 0) : new Color(0, 0, 220);
        }

        using var encoded = new MemoryStream();
        src.Save(encoded, new JpegEncoder { Quality = 95 });
        var jpeg = InsertJpegExif(encoded.ToArray(), 2);
        var image = Image.Load(jpeg);
        Assert.True(image[0, 0].Blue > image[0, 0].Red);
        Assert.True(image[image.Width - 1, 0].Red > image[image.Width - 1, 0].Blue);
    }

    [Fact]
    public void PngExifOrientationRotatesClockwise()
    {
        var src = new Image(2, 1);
        src[0, 0] = new Color(255, 0, 0);
        src[1, 0] = new Color(0, 0, 255);
        using var encoded = new MemoryStream();
        src.Save(encoded, FileFormats.Png);
        var image = Image.Load(InsertPngExif(encoded.ToArray(), 6));
        Assert.Equal(1, image.Width);
        Assert.Equal(2, image.Height);
        Assert.Equal(new Color(255, 0, 0), image[0, 0]);
        Assert.Equal(new Color(0, 0, 255), image[0, 1]);
    }

    [Fact]
    public void ExifOrientationApplyCoversTheEightValues()
    {
        var src = new Image(2, 2);
        src[0, 0] = new Color(1, 0, 0);
        src[1, 0] = new Color(2, 0, 0);
        src[0, 1] = new Color(3, 0, 0);
        src[1, 1] = new Color(4, 0, 0);

        Assert.Equal(new Color(2, 0, 0), ExifOrientation.Apply(src, 2)[0, 0]);
        Assert.Equal(new Color(4, 0, 0), ExifOrientation.Apply(src, 3)[0, 0]);
        Assert.Equal(new Color(3, 0, 0), ExifOrientation.Apply(src, 6)[0, 0]);
        Assert.Equal(new Color(2, 0, 0), ExifOrientation.Apply(src, 8)[0, 0]);
        Assert.Same(src, ExifOrientation.Apply(src, 1));
    }

    [Fact]
    public void WebPExifChunkReportsOrientation()
    {
        var tiff = TiffOrientation(6);
        var payload = new byte[6 + tiff.Length];
        payload[0] = (byte)'E';
        payload[1] = (byte)'x';
        payload[2] = (byte)'i';
        payload[3] = (byte)'f';
        tiff.CopyTo(payload.AsSpan(6));
        var file = new byte[12 + 8 + payload.Length];
        file[0] = (byte)'R';
        file[1] = (byte)'I';
        file[2] = (byte)'F';
        file[3] = (byte)'F';
        file[8] = (byte)'W';
        file[9] = (byte)'E';
        file[10] = (byte)'B';
        file[11] = (byte)'P';
        file[12] = (byte)'E';
        file[13] = (byte)'X';
        file[14] = (byte)'I';
        file[15] = (byte)'F';
        BinaryPrimitives.WriteInt32LittleEndian(file.AsSpan(16), payload.Length);
        payload.CopyTo(file.AsSpan(20));
        Assert.Equal(6, ExifOrientation.ReadWebP(file));
    }

    private static byte[] Dib32(int width, int height, byte[] bgraBottomUp, byte[] andMask)
    {
        var dib = new byte[40 + bgraBottomUp.Length + andMask.Length];
        BinaryPrimitives.WriteInt32LittleEndian(dib.AsSpan(0), 40);
        BinaryPrimitives.WriteInt32LittleEndian(dib.AsSpan(4), width);
        BinaryPrimitives.WriteInt32LittleEndian(dib.AsSpan(8), height * 2);
        BinaryPrimitives.WriteUInt16LittleEndian(dib.AsSpan(12), 1);
        BinaryPrimitives.WriteUInt16LittleEndian(dib.AsSpan(14), 32);
        bgraBottomUp.CopyTo(dib.AsSpan(40));
        andMask.CopyTo(dib.AsSpan(40 + bgraBottomUp.Length));
        return dib;
    }

    private static byte[] Ico(byte[] dib, int width, int height) => Ico([dib], [width], [height]);

    private static byte[] Ico(byte[][] images, int[] widths, int[] heights)
    {
        var directory = 6 + images.Length * 16;
        var size = directory;
        foreach (var image in images)
            size += image.Length;
        var file = new byte[size];
        file[2] = 1;
        file[4] = (byte)images.Length;
        var cursor = directory;
        for (var i = 0; i < images.Length; i++)
        {
            var entry = 6 + i * 16;
            file[entry] = (byte)widths[i];
            file[entry + 1] = (byte)heights[i];
            BinaryPrimitives.WriteUInt16LittleEndian(file.AsSpan(entry + 6), 32);
            BinaryPrimitives.WriteInt32LittleEndian(file.AsSpan(entry + 8), images[i].Length);
            BinaryPrimitives.WriteInt32LittleEndian(file.AsSpan(entry + 12), cursor);
            images[i].CopyTo(file.AsSpan(cursor));
            cursor += images[i].Length;
        }

        return file;
    }

    private static byte[] InsertJpegExif(byte[] jpeg, int orientation)
    {
        var app1 = App1(orientation);
        var wrapped = new byte[jpeg.Length + app1.Length];
        wrapped[0] = 0xFF;
        wrapped[1] = 0xD8;
        app1.CopyTo(wrapped.AsSpan(2));
        jpeg.AsSpan(2).CopyTo(wrapped.AsSpan(2 + app1.Length));
        return wrapped;
    }

    private static byte[] InsertPngExif(byte[] png, int orientation)
    {
        var tiff = TiffOrientation(orientation);
        var chunk = new byte[12 + tiff.Length];
        BinaryPrimitives.WriteInt32BigEndian(chunk.AsSpan(0), tiff.Length);
        chunk[4] = (byte)'e';
        chunk[5] = (byte)'X';
        chunk[6] = (byte)'I';
        chunk[7] = (byte)'f';
        tiff.CopyTo(chunk.AsSpan(8));
        BinaryPrimitives.WriteUInt32BigEndian(chunk.AsSpan(8 + tiff.Length), Crc32(chunk.AsSpan(4, 4 + tiff.Length)));
        var output = new byte[png.Length + chunk.Length];
        png.AsSpan(0, png.Length - 12).CopyTo(output);
        chunk.CopyTo(output.AsSpan(png.Length - 12));
        png.AsSpan(png.Length - 12).CopyTo(output.AsSpan(png.Length - 12 + chunk.Length));
        return output;
    }

    private static byte[] App1(int orientation)
    {
        var tiff = TiffOrientation(orientation);
        var app1 = new byte[4 + 6 + tiff.Length];
        app1[0] = 0xFF;
        app1[1] = 0xE1;
        BinaryPrimitives.WriteUInt16BigEndian(app1.AsSpan(2), (ushort)(app1.Length - 2));
        app1[4] = (byte)'E';
        app1[5] = (byte)'x';
        app1[6] = (byte)'i';
        app1[7] = (byte)'f';
        tiff.CopyTo(app1.AsSpan(10));
        return app1;
    }

    private static byte[] TiffOrientation(int orientation)
    {
        var tiff = new byte[26];
        tiff[0] = (byte)'I';
        tiff[1] = (byte)'I';
        BinaryPrimitives.WriteUInt16LittleEndian(tiff.AsSpan(2), 42);
        BinaryPrimitives.WriteUInt32LittleEndian(tiff.AsSpan(4), 8);
        BinaryPrimitives.WriteUInt16LittleEndian(tiff.AsSpan(8), 1);
        BinaryPrimitives.WriteUInt16LittleEndian(tiff.AsSpan(10), 274);
        BinaryPrimitives.WriteUInt16LittleEndian(tiff.AsSpan(12), 3);
        BinaryPrimitives.WriteUInt32LittleEndian(tiff.AsSpan(14), 1);
        BinaryPrimitives.WriteUInt16LittleEndian(tiff.AsSpan(18), (ushort)orientation);
        return tiff;
    }

    private static uint Crc32(ReadOnlySpan<byte> data)
    {
        var crc = 0xFFFFFFFFu;
        foreach (var value in data)
        {
            crc ^= value;
            for (var bit = 0; bit < 8; bit++)
                crc = (crc & 1) != 0 ? (crc >> 1) ^ 0xEDB88320u : crc >> 1;
        }

        return ~crc;
    }
}
