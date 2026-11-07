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
                WebPBitstreamFeatures features;
                var status = WebPLib.WebPGetFeaturesInternal(
                    data,
                    (nuint)bytes.Length,
                    &features,
                    WebPLib.DecoderAbiVersion
                );
                if (status != 0)
                    throw new ImageException("WebP bitstream could not be read.");
                if (features.HasAnimation != 0)
                {
                    throw new NotSupportedException(
                        "Animated WebP is not supported. ChromaFx.Native decodes still images only."
                    );
                }

                int width;
                int height;
                var pixels = WebPLib.WebPDecodeRGBA(data, (nuint)bytes.Length, &width, &height);
                if (pixels == null || width <= 0 || height <= 0)
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
                    WebPLib.WebPFree(pixels);
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
                    nuint size = lossless
                        ? WebPLib.WebPEncodeLosslessRGBA(
                            rgba,
                            image.Width,
                            image.Height,
                            image.Width * 4,
                            &output
                        )
                        : WebPLib.WebPEncodeRGBA(
                            rgba,
                            image.Width,
                            image.Height,
                            image.Width * 4,
                            quality,
                            &output
                        );
                    if (size == 0 || output == null)
                        throw new ImageException("WebP encode failed.");

                    try
                    {
                        writer.Write(new ReadOnlySpan<byte>(output, checked((int)size)));
                    }
                    finally
                    {
                        WebPLib.WebPFree(output);
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
