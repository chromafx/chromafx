/*
 * Copyright 2025–2026 Ho Tzin Mein
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

using ChromaFx.Core;
using ChromaFx.IO.Encoders;
using ChromaFx.IO.Formats;

namespace ChromaFx.IO;

/// <summary>
/// Animation save / encode extensions. Prefer <see cref="Animation.Load"/> for loading.
/// Saving a non-GIF path (e.g. <c>.png</c>) encodes the first frame as a still image.
/// </summary>
public static class AnimationIOExtensions
{
    /// <summary>
    /// Saves the animation to a file path (format chosen by extension).
    /// </summary>
    public static void Save(this Animation animation, string fileName)
    {
        ArgumentNullException.ThrowIfNull(animation);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ImageFormats.Encode(fileName, animation);
    }

    /// <summary>
    /// Saves the animation to a file path using the given encoder.
    /// </summary>
    public static void Save(this Animation animation, string fileName, IAnimationEncoder encoder)
    {
        ArgumentNullException.ThrowIfNull(animation);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentNullException.ThrowIfNull(encoder);
        new FileInfo(fileName).Directory?.Create();
        using var stream = File.Open(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
        ImageFormats.Encode(stream, animation, encoder);
    }

    /// <summary>
    /// Saves the animation to a stream using the given encoder.
    /// </summary>
    public static void Save(this Animation animation, Stream stream, IAnimationEncoder encoder)
    {
        ArgumentNullException.ThrowIfNull(animation);
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(encoder);
        ImageFormats.Encode(stream, animation, encoder);
    }

    /// <summary>
    /// Saves the animation to a stream using a built-in animation format (currently GIF).
    /// </summary>
    public static void Save(this Animation animation, Stream stream, FileFormats format)
    {
        ArgumentNullException.ThrowIfNull(animation);
        ArgumentNullException.ThrowIfNull(stream);
        ImageFormats.Encode(stream, animation, format);
    }

    /// <summary>
    /// Asynchronously saves the animation to a file path (sync encode, async write).
    /// Format is inferred from the extension.
    /// </summary>
    public static async Task SaveAsync(
        this Animation animation,
        string fileName,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(animation);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        using var buffer = new MemoryStream();
        ImageFormats.Encode(buffer, animation, fileName);
        new FileInfo(fileName).Directory?.Create();
        await WriteBufferAsync(buffer, fileName, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously saves the animation to a file path using the given encoder
    /// (sync encode, async write).
    /// </summary>
    public static async Task SaveAsync(
        this Animation animation,
        string fileName,
        IAnimationEncoder encoder,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(animation);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentNullException.ThrowIfNull(encoder);
        using var buffer = new MemoryStream();
        ImageFormats.Encode(buffer, animation, encoder);
        new FileInfo(fileName).Directory?.Create();
        await WriteBufferAsync(buffer, fileName, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously saves the animation to a stream using the given encoder
    /// (sync encode, async write).
    /// </summary>
    public static async Task SaveAsync(
        this Animation animation,
        Stream stream,
        IAnimationEncoder encoder,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(animation);
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(encoder);
        using var buffer = new MemoryStream();
        ImageFormats.Encode(buffer, animation, encoder);
        buffer.Position = 0;
        await buffer.CopyToAsync(stream, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously saves the animation to a stream using a built-in animation format
    /// (sync encode, async write).
    /// </summary>
    public static async Task SaveAsync(
        this Animation animation,
        Stream stream,
        FileFormats format,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(animation);
        ArgumentNullException.ThrowIfNull(stream);
        using var buffer = new MemoryStream();
        ImageFormats.Encode(buffer, animation, format);
        buffer.Position = 0;
        await buffer.CopyToAsync(stream, cancellationToken).ConfigureAwait(false);
    }

    private static async Task WriteBufferAsync(
        MemoryStream buffer,
        string fileName,
        CancellationToken cancellationToken
    )
    {
        if (buffer.TryGetBuffer(out var segment))
        {
            await File.WriteAllBytesAsync(
                    fileName,
                    segment.AsMemory(0, (int)buffer.Length),
                    cancellationToken
                )
                .ConfigureAwait(false);
            return;
        }

        await File.WriteAllBytesAsync(fileName, buffer.ToArray(), cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Encodes the animation to a new byte array using a built-in animation format.
    /// </summary>
    public static byte[] ToByteArray(this Animation animation, FileFormats format)
    {
        ArgumentNullException.ThrowIfNull(animation);
        using var stream = new MemoryStream();
        animation.Save(stream, format);
        return stream.ToArray();
    }

    /// <summary>
    /// Encodes the animation to a new byte array using the given encoder.
    /// </summary>
    public static byte[] ToByteArray(this Animation animation, IAnimationEncoder encoder)
    {
        ArgumentNullException.ThrowIfNull(animation);
        ArgumentNullException.ThrowIfNull(encoder);
        using var stream = new MemoryStream();
        animation.Save(stream, encoder);
        return stream.ToArray();
    }

    /// <summary>
    /// Encodes the animation as a base-64 string using a built-in animation format.
    /// </summary>
    public static string ToBase64String(this Animation animation, FileFormats desiredFormat) =>
        Convert.ToBase64String(animation.ToByteArray(desiredFormat));

    /// <summary>
    /// Encodes the animation as a base-64 string using the given encoder.
    /// </summary>
    public static string ToBase64String(this Animation animation, IAnimationEncoder encoder) =>
        Convert.ToBase64String(animation.ToByteArray(encoder));
}
