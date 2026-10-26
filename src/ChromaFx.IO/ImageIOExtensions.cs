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
