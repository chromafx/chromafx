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
using ChromaFx.Core;
using ChromaFx.Core.Exceptions;
using ChromaFx.IO.Encoders;
using ChromaFx.IO.Formats;
using ChromaFx.IO.Formats.Bmp;
using ChromaFx.IO.Formats.Gif;
using ChromaFx.IO.Formats.Jpeg;
using ChromaFx.IO.Formats.Png;

namespace ChromaFx.IO;

/// <summary>
/// Global registry of image and animation formats used by <see cref="Image.Load"/> / <see cref="Image"/> Save.
/// </summary>
public static class ImageFormats
{
    private static readonly object Gate = new();
    private static readonly List<IImageFormat> Formats = [];
    private static readonly List<IAnimationFormat> AnimationFormats = [];
    private static bool _builtinsRegistered;

    /// <summary>
    /// Registers an image format (decode/encode). Later registrations are preferred for encode-by-extension ties.
    /// </summary>
    public static void Register(IImageFormat format)
    {
        ArgumentNullException.ThrowIfNull(format);
        lock (Gate)
        {
            Formats.Add(format);
        }
    }

    /// <summary>
    /// Registers an animation format.
    /// </summary>
    public static void Register(IAnimationFormat format)
    {
        ArgumentNullException.ThrowIfNull(format);
        lock (Gate)
        {
            AnimationFormats.Add(format);
            if (format is IImageFormat imageFormat && !Formats.Contains(imageFormat))
                Formats.Add(imageFormat);
        }
    }

    /// <summary>
    /// Ensures built-in BMP/PNG/JPEG/GIF formats are registered.
    /// </summary>
    public static void EnsureBuiltIns()
    {
        if (_builtinsRegistered)
            return;
        lock (Gate)
        {
            if (_builtinsRegistered)
                return;
            Formats.Add(new BmpFormat());
            Formats.Add(new PngFormat());
            Formats.Add(new JpegFormat());
            var gif = new GifFormat();
            Formats.Add(gif);
            AnimationFormats.Add(gif);
            _builtinsRegistered = true;
        }
    }

    internal static Image Decode(Stream stream)
    {
        EnsureBuiltIns();
        IImageFormat[] snapshot;
        lock (Gate)
            snapshot = Formats.ToArray();

        foreach (var format in snapshot)
        {
            if (format.CanDecode(stream))
                return format.Decode(stream) ?? throw new ImageException("Decoder returned null.");
        }

        throw new UnknownImageFormatException("No registered format can decode the stream.");
    }

    internal static Animation DecodeAnimation(Stream stream)
    {
        EnsureBuiltIns();
        IAnimationFormat[] snapshot;
        lock (Gate)
            snapshot = AnimationFormats.ToArray();

        foreach (var format in snapshot)
        {
            if (format.CanDecode(stream))
                return format.DecodeAnimation(stream)
                    ?? throw new ImageException("Animation decoder returned null.");
        }

        throw new UnknownImageFormatException(
            "No registered animation format can decode the stream."
        );
    }

    internal static void Encode(string fileName, Image image)
    {
        EnsureBuiltIns();
        IImageFormat[] snapshot;
        lock (Gate)
            snapshot = Formats.ToArray();

        // Prefer last registered match so extensions can override built-ins.
        for (var i = snapshot.Length - 1; i >= 0; i--)
        {
            if (!snapshot[i].CanEncode(fileName))
                continue;
            new FileInfo(fileName).Directory?.Create();
            using var stream = File.Open(
                fileName,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None
            );
            using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
            if (!snapshot[i].Encode(writer, image))
                throw new ImageException($"Failed to encode image to '{fileName}'.");
            return;
        }

        throw new UnknownImageFormatException($"No registered format can encode '{fileName}'.");
    }

    internal static void Encode(string fileName, Animation animation)
    {
        EnsureBuiltIns();
        IAnimationFormat[] snapshot;
        lock (Gate)
            snapshot = AnimationFormats.ToArray();

        foreach (var format in snapshot)
        {
            if (!format.CanEncode(fileName))
                continue;
            new FileInfo(fileName).Directory?.Create();
            using var stream = File.Open(
                fileName,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None
            );
            using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
            if (!format.Encode(writer, animation))
                throw new ImageException($"Failed to encode animation to '{fileName}'.");
            return;
        }

        // Fall back to first frame as a still image.
        Encode(fileName, animation[0]);
    }

    internal static void Encode(Stream stream, Image image, IImageEncoder encoder)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(image);
        ArgumentNullException.ThrowIfNull(encoder);
        encoder.Encode(stream, image);
    }

    internal static void Encode(Stream stream, Image image, FileFormats format)
    {
        var encoder = CreateEncoder(format);
        Encode(stream, image, encoder);
    }

    internal static IImageEncoder CreateEncoder(FileFormats format) =>
        format switch
        {
            FileFormats.Bmp => new BmpEncoder(),
            FileFormats.Png => new PngEncoder(),
            FileFormats.Jpeg => new JpegEncoder(),
            FileFormats.Gif => new GifEncoder(),
            _ => throw new UnknownImageFormatException($"Unsupported format '{format}'."),
        };
}
