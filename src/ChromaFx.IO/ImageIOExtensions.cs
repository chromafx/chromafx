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
        ImageFormats.Encode(stream, image, encoder);
    }

    /// <summary>
    /// Saves the image to a stream using a built-in format.
    /// </summary>
    public static void Save(this Image image, Stream stream, FileFormats format)
    {
        ArgumentNullException.ThrowIfNull(image);
        ImageFormats.Encode(stream, image, format);
    }

    /// <summary>
    /// Encodes the image as a base-64 string.
    /// </summary>
    public static string ToBase64String(this Image image, FileFormats desiredFormat)
    {
        using var stream = new MemoryStream();
        image.Save(stream, desiredFormat);
        return Convert.ToBase64String(stream.ToArray());
    }

    /// <summary>
    /// Encodes the image as a base-64 string using the specified encoder.
    /// </summary>
    public static string ToBase64String(this Image image, IImageEncoder encoder)
    {
        using var stream = new MemoryStream();
        image.Save(stream, encoder);
        return Convert.ToBase64String(stream.ToArray());
    }
}
