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

namespace ChromaFx.Native.Codecs;

internal static class AvifCodec
{
    internal static Image Decode(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        NativeLibraryLoader.EnsureRegistered();
        var bytes = NativePixels.ReadStream(stream);
        if (bytes.Length == 0)
            throw new ImageException("AVIF data is empty.");

        unsafe
        {
            fixed (byte* data = bytes)
            {
                byte* pixels = null;
                int width;
                int height;
                var status = AvifLib.Decode(data, (nuint)bytes.Length, &pixels, &width, &height);
                if (status == AvifLib.ErrAnimation)
                {
                    throw new NotSupportedException(
                        "Animated AVIF is not supported. ChromaFx.Native decodes still images only."
                    );
                }

                if (status != AvifLib.Ok || pixels == null || width <= 0 || height <= 0)
                    throw new ImageException("AVIF decode failed.");

                try
                {
                    var length = checked(width * height * 4);
                    return NativePixels.ImageFromTight(
                        new ReadOnlySpan<byte>(pixels, length),
                        width,
                        height
                    );
                }
                finally
                {
                    AvifLib.Free(pixels);
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
                    var status = AvifLib.Encode(
                        rgba,
                        image.Width,
                        image.Height,
                        image.Width * 4,
                        quality,
                        lossless ? 1 : 0,
                        &output,
                        &size
                    );
                    if (status != AvifLib.Ok || output == null || size == 0)
                        throw new ImageException("AVIF encode failed.");

                    try
                    {
                        writer.Write(new ReadOnlySpan<byte>(output, checked((int)size)));
                    }
                    finally
                    {
                        AvifLib.Free(output);
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
