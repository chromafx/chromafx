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
using ChromaFx.IO.Encoders;
using ChromaFx.IO.Formats;
using ChromaFx.Native.Formats.WebP;

namespace ChromaFx.Native.Encoders;

/// <summary>
/// Typed still-WebP encoder.
/// </summary>
public sealed class WebPEncoder : IImageEncoder
{
    /// <summary>
    /// Default lossy quality (0–100), matching cwebp.
    /// </summary>
    public const int DefaultQuality = 75;

    /// <summary>
    /// Gets or sets lossy quality (0–100). Ignored when <see cref="Lossless"/> is true.
    /// </summary>
    public int Quality { get; set; } = DefaultQuality;

    /// <summary>
    /// Gets or sets whether to write a lossless WebP.
    /// </summary>
    public bool Lossless { get; set; }

    /// <inheritdoc />
    public FileFormats Format => FileFormats.WebP;

    /// <inheritdoc />
    public void Encode(Stream stream, Image image)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(image);
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        new WebPFormat().Encode(writer, image, Quality, Lossless);
    }
}
