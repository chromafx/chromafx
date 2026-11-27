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
using ChromaFx.Core;
using ChromaFx.Core.Exceptions;
using ChromaFx.IO.Formats;

namespace ChromaFx.Native.Codecs;

internal static class WebPCodec
{
    internal static Image Decode(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        NativeLibraryLoader.EnsureRegistered();
        var bytes = NativePixels.ReadStream(stream);
        if (bytes.Length == 0)
            throw new ImageException("WebP data is empty.");
        var orientation = ExifOrientation.ReadWebP(bytes);

        unsafe
        {
            fixed (byte* data = bytes)
            {
                byte* pixels = null;
                int width;
                int height;
                var status = WebPLib.Decode(data, (nuint)bytes.Length, &pixels, &width, &height);
                if (status != WebPLib.Ok || pixels == null || width <= 0 || height <= 0)
                    throw new ImageException("WebP decode failed.");

                try
                {
                    var length = checked(width * height * 4);
                    return ExifOrientation.Apply(
                        NativePixels.ImageFromTight(
                            new ReadOnlySpan<byte>(pixels, length),
                            width,
                            height
                        ),
                        orientation
                    );
                }
                finally
                {
                    WebPLib.Free(pixels);
                }
            }
        }
    }

    internal static Animation DecodeAnimation(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        NativeLibraryLoader.EnsureRegistered();
        var bytes = NativePixels.ReadStream(stream);
        if (bytes.Length == 0)
            throw new ImageException("WebP data is empty.");

        unsafe
        {
            fixed (byte* data = bytes)
            {
                byte* pixels = null;
                int* delays = null;
                int width;
                int height;
                int frameCount;
                int repeatCount;
                var status = WebPLib.DecodeAnim(
                    data,
                    (nuint)bytes.Length,
                    &pixels,
                    &delays,
                    &width,
                    &height,
                    &frameCount,
                    &repeatCount
                );
                if (
                    status != WebPLib.Ok
                    || pixels == null
                    || delays == null
                    || width <= 0
                    || height <= 0
                    || frameCount <= 0
                )
                    throw new ImageException("WebP animation decode failed.");

                try
                {
                    var frameBytes = checked(width * height * 4);
                    var total = checked(frameBytes * frameCount);
                    return NativePixels.AnimationFromPacked(
                        new ReadOnlySpan<byte>(pixels, total),
                        new ReadOnlySpan<int>(delays, frameCount),
                        width,
                        height,
                        frameCount,
                        (ushort)Math.Clamp(repeatCount, 0, ushort.MaxValue)
                    );
                }
                finally
                {
                    WebPLib.Free(pixels);
                    WebPLib.Free(delays);
                }
            }
        }
    }

    internal static void Encode(BinaryWriter writer, Image image, int quality, bool lossless)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(image);
        NativeLibraryLoader.EnsureRegistered();
        quality = Math.Clamp(quality, 0, 100);
        var rented = NativePixels.RentTight(image, out _);
        try
        {
            unsafe
            {
                fixed (byte* rgba = rented)
                {
                    byte* output = null;
                    nuint size = 0;
                    var status = WebPLib.Encode(
                        rgba,
                        image.Width,
                        image.Height,
                        image.Width * 4,
                        quality,
                        lossless ? 1 : 0,
                        &output,
                        &size
                    );
                    if (status != WebPLib.Ok || output == null || size == 0)
                        throw new ImageException("WebP encode failed.");

                    try
                    {
                        writer.Write(new ReadOnlySpan<byte>(output, checked((int)size)));
                    }
                    finally
                    {
                        WebPLib.Free(output);
                    }
                }
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rented);
        }
    }

    internal static void Encode(BinaryWriter writer, Animation animation, int quality, bool lossless)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(animation);
        if (animation.Count == 0)
            throw new ImageException("Animation has no frames.");
        if (animation.Count == 1)
        {
            Encode(writer, animation[0], quality, lossless);
            return;
        }

        NativeLibraryLoader.EnsureRegistered();
        quality = Math.Clamp(quality, 0, 100);
        var rented = NativePixels.RentAnimationPacked(animation, out var delaysCs, out _);
        try
        {
            unsafe
            {
                fixed (byte* rgba = rented)
                fixed (int* delays = delaysCs)
                {
                    byte* output = null;
                    nuint size = 0;
                    var status = WebPLib.EncodeAnim(
                        rgba,
                        delays,
                        animation[0].Width,
                        animation[0].Height,
                        animation.Count,
                        animation.RepeatCount,
                        quality,
                        lossless ? 1 : 0,
                        &output,
                        &size
                    );
                    if (status != WebPLib.Ok || output == null || size == 0)
                        throw new ImageException("WebP animation encode failed.");

                    try
                    {
                        writer.Write(new ReadOnlySpan<byte>(output, checked((int)size)));
                    }
                    finally
                    {
                        WebPLib.Free(output);
                    }
                }
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rented);
        }
    }
}
