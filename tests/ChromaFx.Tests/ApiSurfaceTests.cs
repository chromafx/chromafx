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
using ChromaFx.Native;
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
            Assert.False(File.Exists(path));
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
    public void LoadNullStreamThrows()
    {
        Assert.Throws<ArgumentNullException>(() => Image.Load((Stream)null!));
    }

    [Fact]
    public void LoadFromByteArrayAndSpanRoundTripsBmp()
    {
        var bytes = File.ReadAllBytes("./TestImages/Formats/Bmp/EncodingTest.bmp");
        var fromArray = Image.Load(bytes);
        var fromMemory = Image.Load(bytes.AsMemory());
        var fromSpan = Image.Load(bytes.AsSpan());

        Assert.Equal(fromArray.Width, fromMemory.Width);
        Assert.Equal(fromArray.Height, fromMemory.Height);
        Assert.Equal(fromArray.Width, fromSpan.Width);
        Assert.Equal(fromArray.Height, fromSpan.Height);
        Assert.Equal(fromArray[0, 0], fromSpan[0, 0]);
    }

    [Fact]
    public async Task LoadAsyncAndSaveAsyncRoundTripPng()
    {
        var source = await Image.LoadAsync("./TestImages/Formats/Bmp/EncodingTest.bmp", CancellationToken.None);
        var path = Path.Combine(Path.GetTempPath(), $"chromafx-{Guid.NewGuid():N}.png");
        try
        {
            await source.SaveAsync(path, CancellationToken.None);
            var loaded = await Image.LoadAsync(path, CancellationToken.None);
            Assert.Equal(source.Width, loaded.Width);
            Assert.Equal(source.Height, loaded.Height);
            Assert.Equal(source[0, 0], loaded[0, 0]);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public async Task LoadAsyncFromStreamRoundTrips()
    {
        await using var stream = File.OpenRead("./TestImages/Formats/Bmp/EncodingTest.bmp");
        var loaded = await Image.LoadAsync(stream, CancellationToken.None);
        Assert.True(loaded.Width > 0);
        Assert.True(loaded.Height > 0);
    }

    [Fact]
    public async Task AnimationLoadAsyncAndSaveAsyncRoundTripGif()
    {
        var source = await Animation.LoadAsync("./TestImages/Formats/Gif/rings.gif", CancellationToken.None);
        var path = Path.Combine(Path.GetTempPath(), $"chromafx-{Guid.NewGuid():N}.gif");
        try
        {
            await source.SaveAsync(path, CancellationToken.None);
            var loaded = await Animation.LoadAsync(path, CancellationToken.None);
            Assert.Equal(source.Count, loaded.Count);
            Assert.Equal(source[0].Width, loaded[0].Width);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public void AnimationToByteArrayRoundTripsGif()
    {
        var red = new Image(4, 4);
        red.Buffer.Fill(Color.RedColor);
        var animation = new Animation([red], [20]) { RepeatCount = 1 };
        var bytes = animation.ToByteArray(FileFormats.Gif);
        Assert.True(bytes.Length > 6);
        Assert.Equal((byte)'G', bytes[0]);

        using var stream = new MemoryStream(bytes);
        var loaded = Animation.Load(stream);
        Assert.Single(loaded);
        Assert.Equal(20, loaded.GetFrameDelay(0));
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
    public void RegisteredWebPPluginDrivesKeySaveAndToByteArray()
    {
        var stub = new StubWebPFormat();
        ImageFormats.Register(stub);

        var image = new Image(1, 1, [new Color(255, 0, 0, 255)]);
        using var stream = new MemoryStream();
        image.Save(stream, NativeFormats.WebP);
        Assert.Equal(1, stub.EncodeCalls);
        Assert.Equal("WEBPSTUB"u8.ToArray(), stream.ToArray());

        var bytes = image.ToByteArray(new ImageFormatKey("WEBP"));
        Assert.Equal(2, stub.EncodeCalls);
        Assert.Equal("WEBPSTUB"u8.ToArray(), bytes);
    }

    [Fact]
    public void SaveByUnknownKeyThrows()
    {
        var image = new Image(1, 1, [new Color(1, 2, 3, 255)]);
        using var stream = new MemoryStream();
        Assert.Throws<UnknownImageFormatException>(() =>
            image.Save(stream, new ImageFormatKey("nope"))
        );
    }

    [Fact]
    public void AnimationLoadReadsGifFrames()
    {
        var animation = Animation.Load("./TestImages/Formats/Gif/rings.gif");
        Assert.True(animation.Count >= 1);
        Assert.True(animation[0].Width > 0);
        Assert.True(animation[0].Height > 0);
        Assert.Equal(animation.Count, animation.FrameDelays.Count);
    }

    [Fact]
    public void AnimationPerFrameDelaysRoundTripViaGifEncoder()
    {
        var red = new Image(8, 8);
        var blue = new Image(8, 8);
        red.Buffer.Fill(Color.RedColor);
        blue.Buffer.Fill(Color.BlueColor);

        var animation = new Animation([red, blue], [10, 40]) { RepeatCount = 3 };
        using var stream = new MemoryStream();
        animation.Save(stream, new GifEncoder());
        stream.Position = 0;

        var loaded = Animation.Load(stream);
        Assert.Equal(2, loaded.Count);
        Assert.Equal(10, loaded.GetFrameDelay(0));
        Assert.Equal(40, loaded.GetFrameDelay(1));
        Assert.Equal((ushort)3, loaded.RepeatCount);
    }

    [Fact]
    public void AnimationRepeatCountInfiniteRoundTripsViaGif()
    {
        var red = new Image(4, 4);
        var blue = new Image(4, 4);
        red.Buffer.Fill(Color.RedColor);
        blue.Buffer.Fill(Color.BlueColor);

        var animation = new Animation([red, blue], [5, 5]) { RepeatCount = 0 };
        var bytes = animation.ToByteArray(FileFormats.Gif);
        var loaded = Animation.Load(bytes);
        Assert.Equal((ushort)0, loaded.RepeatCount);
    }

    [Fact]
    public void AnimationSaveEmptyThrows()
    {
        var empty = new Animation([]);
        var path = Path.Combine(Path.GetTempPath(), $"chromafx-{Guid.NewGuid():N}.png");
        Assert.Throws<ArgumentException>(() => empty.Save(path));
        Assert.False(File.Exists(path));
    }

    [Fact]
    public void LoadNonSeekableStreamBuffersAndDecodes()
    {
        var bytes = File.ReadAllBytes("./TestImages/Formats/Bmp/EncodingTest.bmp");
        using var nonSeekable = new NonSeekableStream(bytes);
        var loaded = Image.Load(nonSeekable);
        Assert.True(loaded.Width > 0);
        Assert.True(loaded.Height > 0);
    }

    [Fact]
    public void AnimationToBase64StringRoundTripsGifHeader()
    {
        var red = new Image(2, 2);
        red.Buffer.Fill(Color.RedColor);
        var animation = new Animation([red], [10]) { RepeatCount = 1 };
        var b64 = animation.ToBase64String(FileFormats.Gif);
        var bytes = Convert.FromBase64String(b64);
        Assert.Equal((byte)'G', bytes[0]);
        Assert.Equal((byte)'I', bytes[1]);
    }

    private sealed class NonSeekableStream(byte[] data) : MemoryStream(data)
    {
        public override bool CanSeek => false;

        public override long Seek(long offset, SeekOrigin loc) =>
            throw new NotSupportedException();
    }

    [Fact]
    public void TryGetFormatFindsBuiltInPng()
    {
        Assert.True(ImageFormats.TryGetFormat(FileFormats.Png, out var format));
        Assert.Equal(FileFormats.Png, format!.Format);
    }

    [Fact]
    public void FluentContextExtensionsCompile()
    {
        var image = new Image(2, 2);
        image.Mutate(ctx => ctx.BoxBlur(3).Saturation(0.1f).Grayscale());
        Assert.Equal(2, image.Width);
    }

    private sealed class StubImageFormat : IImageFormat
    {
        public int EncodeCalls { get; private set; }

        public ImageFormatKey Format => new("cfxstub");

        public bool CanDecode(string fileName) => false;

        public bool CanDecode(byte[] header) => false;

        public bool CanDecode(Stream stream) => false;

        public bool CanEncode(string fileName) =>
            fileName.EndsWith(".cfxstub", StringComparison.OrdinalIgnoreCase);

        public Image Decode(Stream stream) => throw new NotSupportedException();

        public void Encode(BinaryWriter writer, Image image)
        {
            EncodeCalls++;
            writer.Write(Encoding.ASCII.GetBytes("STUB"));
        }
    }

    private sealed class StubWebPFormat : IImageFormat
    {
        public int EncodeCalls { get; private set; }

        public ImageFormatKey Format => NativeFormats.WebP;

        public bool CanDecode(string fileName) => false;

        public bool CanDecode(byte[] header) => false;

        public bool CanDecode(Stream stream) => false;

        public bool CanEncode(string fileName) =>
            fileName.EndsWith(".webp", StringComparison.OrdinalIgnoreCase);

        public Image Decode(Stream stream) => throw new NotSupportedException();

        public void Encode(BinaryWriter writer, Image image)
        {
            EncodeCalls++;
            writer.Write(Encoding.ASCII.GetBytes("WEBPSTUB"));
        }
    }
}
