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

using System.Buffers;
using System.Runtime.InteropServices;
using ChromaFx.Core;
using ChromaFx.Core.Colors;
using ChromaFx.Core.Exceptions;

namespace ChromaFx.Native.Codecs;

internal static class NativePixels
{
    internal static byte[] ReadStream(Stream stream)
    {
        if (stream.CanSeek)
        {
            var remaining = stream.Length - stream.Position;
            if (remaining < 0 || remaining > int.MaxValue)
                throw new ImageException("Image is too large to decode.");
            var buffer = new byte[(int)remaining];
            stream.ReadExactly(buffer);
            return buffer;
        }

        using var copy = new MemoryStream();
        stream.CopyTo(copy);
        return copy.ToArray();
    }

    internal static byte[] RentTight(Image image, out int length)
    {
        ArgumentNullException.ThrowIfNull(image);
        length = checked(image.Width * image.Height * 4);
        var buffer = ArrayPool<byte>.Shared.Rent(length);
        var dest = buffer.AsSpan(0, length);
        var rowBytes = image.Width * 4;
        for (var y = 0; y < image.Height; y++)
        {
            MemoryMarshal.AsBytes(image.GetRowSpan(y)).CopyTo(dest[(y * rowBytes)..]);
        }

        return buffer;
    }

    internal static Image ImageFromTight(ReadOnlySpan<byte> rgba, int width, int height)
    {
        if (width <= 0 || height <= 0)
            throw new ImageException("Decoded image has invalid dimensions.");

        var expected = checked(width * height * 4);
        if (rgba.Length < expected)
            throw new ImageException("Decoded image is shorter than its dimensions.");

        var image = new Image(width, height);
        var rowBytes = width * 4;
        for (var y = 0; y < height; y++)
        {
            var row = MemoryMarshal.Cast<byte, Color>(rgba.Slice(y * rowBytes, rowBytes));
            row.CopyTo(image.GetRowSpan(y));
        }

        return image;
    }

    /// <summary>
    /// Builds an <see cref="Animation"/> from packed RGBA frames and GIF-centisecond delays.
    /// </summary>
    internal static Animation AnimationFromPacked(
        ReadOnlySpan<byte> rgba,
        ReadOnlySpan<int> delaysCs,
        int width,
        int height,
        int frameCount,
        ushort repeatCount
    )
    {
        if (width <= 0 || height <= 0 || frameCount <= 0)
            throw new ImageException("Decoded animation has invalid dimensions.");
        if (delaysCs.Length < frameCount)
            throw new ImageException("Decoded animation is missing frame delays.");

        var frameBytes = checked(width * height * 4);
        var expected = checked(frameBytes * frameCount);
        if (rgba.Length < expected)
            throw new ImageException("Decoded animation is shorter than its dimensions.");

        var frames = new Image[frameCount];
        var delays = new short[frameCount];
        for (var i = 0; i < frameCount; i++)
        {
            frames[i] = ImageFromTight(rgba.Slice(i * frameBytes, frameBytes), width, height);
            var delay = delaysCs[i];
            if (delay < short.MinValue)
                delay = short.MinValue;
            else if (delay > short.MaxValue)
                delay = short.MaxValue;
            delays[i] = (short)delay;
        }

        return new Animation(frames, delays) { RepeatCount = repeatCount };
    }

    /// <summary>
    /// Packs animation frames into a contiguous RGBA buffer (ArrayPool). Caller must Return.
    /// </summary>
    internal static byte[] RentAnimationPacked(Animation animation, out int[] delaysCs, out int length)
    {
        ArgumentNullException.ThrowIfNull(animation);
        if (animation.Count == 0)
            throw new ImageException("Animation has no frames.");

        var width = animation[0].Width;
        var height = animation[0].Height;
        if (width <= 0 || height <= 0)
            throw new ImageException("Animation frame has invalid dimensions.");

        var frameBytes = checked(width * height * 4);
        length = checked(frameBytes * animation.Count);
        var buffer = ArrayPool<byte>.Shared.Rent(length);
        delaysCs = new int[animation.Count];
        for (var i = 0; i < animation.Count; i++)
        {
            var frame = animation[i];
            if (frame.Width != width || frame.Height != height)
            {
                ArrayPool<byte>.Shared.Return(buffer);
                throw new ImageException("Animation frames must share the same dimensions.");
            }

            var dest = buffer.AsSpan(i * frameBytes, frameBytes);
            var rowBytes = width * 4;
            for (var y = 0; y < height; y++)
                MemoryMarshal.AsBytes(frame.GetRowSpan(y)).CopyTo(dest[(y * rowBytes)..]);
            delaysCs[i] = animation.GetFrameDelay(i);
        }

        return buffer;
    }
}
