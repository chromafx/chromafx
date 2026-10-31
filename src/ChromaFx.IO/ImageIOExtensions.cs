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
/// Image load/save extensions.
/// </summary>
public static class ImageIOExtensions
{
    /// <summary>
    /// Saves the image to a file path (format inferred from extension).
    /// </summary>
    public static void Save(this Image image, string fileName)
    {
        ArgumentNullException.ThrowIfNull(image);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ImageFormats.Encode(fileName, image);
    }

    /// <summary>
    /// Saves the image to a file path using the specified encoder.
    /// </summary>
    public static void Save(this Image image, string fileName, IImageEncoder encoder)
    {
        ArgumentNullException.ThrowIfNull(image);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentNullException.ThrowIfNull(encoder);
        new FileInfo(fileName).Directory?.Create();
        using var stream = File.Open(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
        ImageFormats.Encode(stream, image, encoder);
    }

    /// <summary>
    /// Saves the image to a stream using the specified encoder.
    /// </summary>
    public static void Save(this Image image, Stream stream, IImageEncoder encoder)
    {
        ArgumentNullException.ThrowIfNull(image);
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(encoder);
        ImageFormats.Encode(stream, image, encoder);
    }

    /// <summary>
    /// Saves the image to a stream using a built-in format.
    /// </summary>
    public static void Save(this Image image, Stream stream, FileFormats format)
    {
        ArgumentNullException.ThrowIfNull(image);
        ArgumentNullException.ThrowIfNull(stream);
        ImageFormats.Encode(stream, image, format);
    }

    /// <summary>
    /// Asynchronously saves the image to a file path (sync encode, async write).
    /// Format is inferred from the extension.
    /// </summary>
    public static async Task SaveAsync(
        this Image image,
        string fileName,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(image);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        using var buffer = new MemoryStream();
        ImageFormats.Encode(buffer, image, fileName);
        new FileInfo(fileName).Directory?.Create();
        await WriteBufferAsync(buffer, fileName, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously saves the image to a file path using the specified encoder
    /// (sync encode, async write).
    /// </summary>
    public static async Task SaveAsync(
        this Image image,
        string fileName,
        IImageEncoder encoder,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(image);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentNullException.ThrowIfNull(encoder);
        using var buffer = new MemoryStream();
        ImageFormats.Encode(buffer, image, encoder);
        new FileInfo(fileName).Directory?.Create();
        await WriteBufferAsync(buffer, fileName, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously saves the image to a stream using the specified encoder
    /// (sync encode, async write).
    /// </summary>
    public static async Task SaveAsync(
        this Image image,
        Stream stream,
        IImageEncoder encoder,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(image);
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(encoder);
        using var buffer = new MemoryStream();
        ImageFormats.Encode(buffer, image, encoder);
        await CopyBufferAsync(buffer, stream, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously saves the image to a stream using a built-in format
    /// (sync encode, async write).
    /// </summary>
    public static async Task SaveAsync(
        this Image image,
        Stream stream,
        FileFormats format,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(image);
        ArgumentNullException.ThrowIfNull(stream);
        using var buffer = new MemoryStream();
        ImageFormats.Encode(buffer, image, format);
        await CopyBufferAsync(buffer, stream, cancellationToken).ConfigureAwait(false);
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

    private static async Task CopyBufferAsync(
        MemoryStream buffer,
        Stream destination,
        CancellationToken cancellationToken
    )
    {
        buffer.Position = 0;
        await buffer.CopyToAsync(destination, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Encodes the image to a new byte array using a built-in format.
    /// </summary>
    public static byte[] ToByteArray(this Image image, FileFormats format)
    {
        ArgumentNullException.ThrowIfNull(image);
        using var stream = new MemoryStream();
        image.Save(stream, format);
        return stream.ToArray();
    }

    /// <summary>
    /// Encodes the image to a new byte array using the specified encoder.
    /// </summary>
    public static byte[] ToByteArray(this Image image, IImageEncoder encoder)
    {
        ArgumentNullException.ThrowIfNull(image);
        ArgumentNullException.ThrowIfNull(encoder);
        using var stream = new MemoryStream();
        image.Save(stream, encoder);
        return stream.ToArray();
    }

    /// <summary>
    /// Encodes the image as a base-64 string.
    /// </summary>
    public static string ToBase64String(this Image image, FileFormats desiredFormat) =>
        Convert.ToBase64String(image.ToByteArray(desiredFormat));

    /// <summary>
    /// Encodes the image as a base-64 string using the specified encoder.
    /// </summary>
    public static string ToBase64String(this Image image, IImageEncoder encoder) =>
        Convert.ToBase64String(image.ToByteArray(encoder));
}
