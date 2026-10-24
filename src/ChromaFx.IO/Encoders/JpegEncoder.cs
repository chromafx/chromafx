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
using ChromaFx.IO.Formats;

namespace ChromaFx.IO.Encoders;

/// <summary>
/// JPEG encoder options.
/// </summary>
public sealed class JpegEncoder : IImageEncoder
{
    /// <summary>
    /// Gets or sets JPEG quality (1–100). Default is 90.
    /// </summary>
    public int Quality { get; set; } = 90;

    /// <inheritdoc />
    public FileFormats Format => FileFormats.Jpeg;

    /// <inheritdoc />
    public void Encode(Stream stream, Image image)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(image);
        var quality = Math.Clamp(Quality, 1, 100);
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        new ChromaFx.IO.Formats.Jpeg.Format.File().Write(writer, image, quality);
    }
}
