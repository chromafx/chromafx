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
using ChromaFx.IO;
using ChromaFx.IO.Encoders;
using Xunit;

namespace ChromaFx.Tests;

public class ImageMetadataPreserveTests
{
    [Fact]
    public void Jpeg_RoundTripsExifAndXmp()
    {
        var image = Solid(16, 16, new Color(10, 20, 30, 255));
        image.Metadata.Exif = TiffOrientation(1);
        image.Metadata.Xmp = "<x:xmpmeta xmlns:x=\"adobe:ns:meta/\"><rdf:RDF/></x:xmpmeta>";

        using var ms = new MemoryStream();
        image.Save(ms, new JpegEncoder { Quality = 90 });
        var loaded = Image.Load(ms.ToArray());

        Assert.NotNull(loaded.Metadata.Exif);
        Assert.True(loaded.Metadata.Exif!.Length >= 8);
        Assert.Equal(image.Metadata.Xmp, loaded.Metadata.Xmp);
        Assert.Equal(1, ReadOrientation(loaded.Metadata.Exif));
    }

    [Fact]
    public void Png_RoundTripsExifAndXmp()
    {
        var image = Solid(8, 8, new Color(1, 2, 3, 255));
        image.Metadata.Exif = TiffOrientation(6);
        image.Metadata.Xmp = "<x:xmpmeta>png</x:xmpmeta>";

        using var ms = new MemoryStream();
        image.Save(ms, new PngEncoder());
        var loaded = Image.Load(ms.ToArray());

        Assert.NotNull(loaded.Metadata.Exif);
        Assert.Equal(1, ReadOrientation(loaded.Metadata.Exif!)); // rewritten on write
        Assert.Equal(image.Metadata.Xmp, loaded.Metadata.Xmp);
    }

    [Fact]
    public void Jpeg_LoadCapturesExistingExifApp1()
    {
        var image = Solid(8, 8, new Color(40, 50, 60, 255));
        using var ms = new MemoryStream();
        image.Save(ms, new JpegEncoder { Quality = 90 });
        var jpeg = InsertJpegExif(ms.ToArray(), 1);
        var loaded = Image.Load(jpeg);
        Assert.NotNull(loaded.Metadata.Exif);
        Assert.Equal(1, ReadOrientation(loaded.Metadata.Exif!));
    }

    [Fact]
    public void Clone_CopiesMetadata()
    {
        var image = Solid(4, 4, new Color(0, 0, 0, 255));
        image.Metadata.Xmp = "clone-me";
        image.Metadata.Exif = TiffOrientation(1);
        var copy = image.Clone();
        Assert.Equal("clone-me", copy.Metadata.Xmp);
        Assert.NotSame(image.Metadata.Exif, copy.Metadata.Exif);
        Assert.Equal(image.Metadata.Exif, copy.Metadata.Exif);
    }

    private static Image Solid(int w, int h, Color color)
    {
        var data = new Color[w * h];
        Array.Fill(data, color);
        return new Image(w, h, data);
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

    private static int ReadOrientation(byte[] tiff)
    {
        var little = tiff[0] == (byte)'I';
        var ifd = (int)(
            little
                ? BinaryPrimitives.ReadUInt32LittleEndian(tiff.AsSpan(4))
                : BinaryPrimitives.ReadUInt32BigEndian(tiff.AsSpan(4))
        );
        var entry = ifd + 2;
        return little
            ? BinaryPrimitives.ReadUInt16LittleEndian(tiff.AsSpan(entry + 8))
            : BinaryPrimitives.ReadUInt16BigEndian(tiff.AsSpan(entry + 8));
    }

    private static byte[] InsertJpegExif(byte[] jpeg, int orientation)
    {
        var tiff = TiffOrientation(orientation);
        var app1 = new byte[4 + 6 + tiff.Length];
        app1[0] = 0xFF;
        app1[1] = 0xE1;
        BinaryPrimitives.WriteUInt16BigEndian(app1.AsSpan(2), (ushort)(app1.Length - 2));
        Encoding.ASCII.GetBytes("Exif").CopyTo(app1.AsSpan(4));
        tiff.CopyTo(app1.AsSpan(10));
        var wrapped = new byte[jpeg.Length + app1.Length];
        wrapped[0] = 0xFF;
        wrapped[1] = 0xD8;
        app1.CopyTo(wrapped.AsSpan(2));
        jpeg.AsSpan(2).CopyTo(wrapped.AsSpan(2 + app1.Length));
        return wrapped;
    }
}
