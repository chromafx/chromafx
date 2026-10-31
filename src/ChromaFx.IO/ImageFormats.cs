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
/// Global registry of image and animation formats used by <see cref="Image.Load"/> / Save.
/// Decode uses the first registered format that accepts the stream; encode-by-extension prefers
/// the last registered match so plugins can override built-ins.
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
    /// Tries to get the first registered format with the given <see cref="FileFormats"/> id.
    /// </summary>
    public static bool TryGetFormat(FileFormats format, out IImageFormat imageFormat)
    {
        EnsureBuiltIns();
        lock (Gate)
        {
            imageFormat = Formats.FirstOrDefault(f => f.Format == format);
            return imageFormat is not null;
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
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanSeek)
        {
            using var buffered = BufferStream(stream);
            return Decode(buffered);
        }

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
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanSeek)
        {
            using var buffered = BufferStream(stream);
            return DecodeAnimation(buffered);
        }

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

    private static MemoryStream BufferStream(Stream stream)
    {
        var buffered = new MemoryStream();
        stream.CopyTo(buffered);
        buffered.Position = 0;
        return buffered;
    }

    internal static void Encode(string fileName, Image image)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentNullException.ThrowIfNull(image);
        var format = ResolveImageEncoder(fileName);
        new FileInfo(fileName).Directory?.Create();
        using var stream = File.Open(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
        Encode(stream, image, format);
    }

    internal static void Encode(string fileName, Animation animation)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentNullException.ThrowIfNull(animation);
        if (TryResolveAnimationEncoder(fileName, out var format))
        {
            new FileInfo(fileName).Directory?.Create();
            using var stream = File.Open(
                fileName,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None
            );
            Encode(stream, animation, format);
            return;
        }

        // Fall back to first frame as a still image (may still throw if extension is unknown).
        if (animation.Count == 0)
            throw new ArgumentException("Animation contains no frames.", nameof(animation));
        Encode(fileName, animation[0]);
    }

    /// <summary>
    /// Encodes by file-name extension into an open stream (used by SaveAsync).
    /// </summary>
    internal static void Encode(Stream stream, Image image, string fileNameForExtension)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(image);
        Encode(stream, image, ResolveImageEncoder(fileNameForExtension));
    }

    /// <summary>
    /// Encodes by file-name extension into an open stream (used by SaveAsync).
    /// </summary>
    internal static void Encode(Stream stream, Animation animation, string fileNameForExtension)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(animation);
        if (TryResolveAnimationEncoder(fileNameForExtension, out var format))
        {
            Encode(stream, animation, format);
            return;
        }

        // Fall back to first frame as a still image.
        if (animation.Count == 0)
            throw new ArgumentException("Animation contains no frames.", nameof(animation));
        Encode(stream, animation[0], fileNameForExtension);
    }

    private static IImageFormat ResolveImageEncoder(string fileNameForExtension)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileNameForExtension);
        EnsureBuiltIns();
        IImageFormat[] snapshot;
        lock (Gate)
            snapshot = Formats.ToArray();

        // Prefer last registered match so extensions can override built-ins.
        for (var i = snapshot.Length - 1; i >= 0; i--)
        {
            if (snapshot[i].CanEncode(fileNameForExtension))
                return snapshot[i];
        }

        throw new UnknownImageFormatException(
            $"No registered format can encode '{fileNameForExtension}'."
        );
    }

    private static bool TryResolveAnimationEncoder(
        string fileNameForExtension,
        out IAnimationFormat format
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileNameForExtension);
        EnsureBuiltIns();
        IAnimationFormat[] snapshot;
        lock (Gate)
            snapshot = AnimationFormats.ToArray();

        // Prefer last registered match so extensions can override built-ins (same as images).
        for (var i = snapshot.Length - 1; i >= 0; i--)
        {
            if (!snapshot[i].CanEncode(fileNameForExtension))
                continue;
            format = snapshot[i];
            return true;
        }

        format = null!;
        return false;
    }

    private static void Encode(Stream stream, Image image, IImageFormat format)
    {
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        format.Encode(writer, image);
    }

    private static void Encode(Stream stream, Animation animation, IAnimationFormat format)
    {
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        format.Encode(writer, animation);
    }

    internal static void Encode(Stream stream, Image image, IImageEncoder encoder)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(image);
        ArgumentNullException.ThrowIfNull(encoder);
        encoder.Encode(stream, image);
    }

    internal static void Encode(Stream stream, Animation animation, IAnimationEncoder encoder)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(animation);
        ArgumentNullException.ThrowIfNull(encoder);
        encoder.Encode(stream, animation);
    }

    internal static void Encode(Stream stream, Image image, FileFormats format)
    {
        var encoder = CreateEncoder(format);
        Encode(stream, image, encoder);
    }

    internal static void Encode(Stream stream, Animation animation, FileFormats format)
    {
        var encoder = CreateAnimationEncoder(format);
        Encode(stream, animation, encoder);
    }

    internal static IImageEncoder CreateEncoder(FileFormats format) =>
        format switch
        {
            FileFormats.Bmp => new BmpEncoder(),
            FileFormats.Png => new PngEncoder(),
            FileFormats.Jpeg => new JpegEncoder(),
            FileFormats.Gif => new GifEncoder(),
            FileFormats.WebP or FileFormats.Avif => throw new UnknownImageFormatException(
                $"Format '{format}' requires a registered plugin encoder (or a future native package)."
            ),
            _ => throw new UnknownImageFormatException($"Unsupported format '{format}'."),
        };

    internal static IAnimationEncoder CreateAnimationEncoder(FileFormats format) =>
        format switch
        {
            FileFormats.Gif => new GifEncoder(),
            FileFormats.WebP or FileFormats.Avif => throw new UnknownImageFormatException(
                $"Format '{format}' requires a registered plugin encoder (or a future native package)."
            ),
            _ => throw new UnknownImageFormatException(
                $"Format '{format}' does not support animation encoding."
            ),
        };
}
