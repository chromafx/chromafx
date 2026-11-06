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
}
