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

using System.Text;
using ChromaFx.Core.Exceptions;
using Xunit;

namespace ChromaFx.Tests;

/// <summary>
/// Covers the public Load/Save/Mutate/Clone/registry surface (not filter goldens).
/// </summary>
public class ApiSurfaceTests
{
    [Fact]
    public void MutateChangesOriginal_CloneDoesNot()
    {
        var image = new Image(2, 1, [new Color(10, 20, 30, 255), new Color(40, 50, 60, 255)]);
        var before = image[0, 0];

        var copy = image.Clone(ctx => ctx.Invert());
        Assert.Equal(before, image[0, 0]);
        Assert.NotEqual(before, copy[0, 0]);

        image.Mutate(ctx => ctx.Invert());
        Assert.NotEqual(before, image[0, 0]);
        Assert.Equal(copy[0, 0], image[0, 0]);
    }

    [Fact]
    public void IndexerRejectsOutOfRange()
    {
        var image = new Image(1, 1, [new Color(255, 255, 255, 255)]);
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = image[-1, 0]);
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = image[0, 1]);
        Assert.Throws<ArgumentOutOfRangeException>(() => image[1, 0] = new Color(0, 0, 0, 255));
    }

    [Fact]
    public void SaveWithJpegEncoderRoundTripsDimensions()
    {
        var source = Image.Load("./TestImages/Formats/Bmp/EncodingTest.bmp");
        using var stream = new MemoryStream();
        source.Save(stream, new JpegEncoder { Quality = 70 });
        stream.Position = 0;

        var decoded = Image.Load(stream);
        Assert.Equal(source.Width, decoded.Width);
        Assert.Equal(source.Height, decoded.Height);
    }

    [Fact]
    public void SaveUnknownExtensionThrows()
    {
        var image = new Image(1, 1, [new Color(255, 255, 255, 255)]);
        var path = Path.Combine(Path.GetTempPath(), $"chromafx-{Guid.NewGuid():N}.nope");
        try
        {
            Assert.Throws<UnknownImageFormatException>(() => image.Save(path));
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public void LoadUnknownStreamThrows()
    {
        using var stream = new MemoryStream([0x00, 0x01, 0x02, 0x03]);
        Assert.Throws<UnknownImageFormatException>(() => Image.Load(stream));
    }

    [Fact]
    public void RegisterCustomFormatDrivesSaveByExtension()
    {
        var stub = new StubImageFormat();
        ImageFormats.Register(stub);

        var image = new Image(1, 1, [new Color(255, 0, 0, 255)]);
        var path = Path.Combine(Path.GetTempPath(), $"chromafx-{Guid.NewGuid():N}.cfxstub");
        try
        {
            image.Save(path);
            Assert.Equal(1, stub.EncodeCalls);
            Assert.Equal("STUB"u8.ToArray(), File.ReadAllBytes(path));
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public void AnimationLoadReadsGifFrames()
    {
        var animation = Animation.Load("./TestImages/Formats/Gif/rings.gif");
        Assert.True(animation.Count >= 1);
        Assert.True(animation[0].Width > 0);
        Assert.True(animation[0].Height > 0);
    }

    private sealed class StubImageFormat : IImageFormat
    {
        public int EncodeCalls { get; private set; }

        public FileFormats Format => FileFormats.Png;

        public bool CanDecode(string fileName) => false;

        public bool CanDecode(byte[] header) => false;

        public bool CanDecode(Stream stream) => false;

        public bool CanEncode(string fileName) =>
            fileName.EndsWith(".cfxstub", StringComparison.OrdinalIgnoreCase);

        public Image Decode(Stream stream) => throw new NotSupportedException();

        public bool Encode(BinaryWriter writer, Image image)
        {
            EncodeCalls++;
            writer.Write(Encoding.ASCII.GetBytes("STUB"));
            return true;
        }
    }
}
