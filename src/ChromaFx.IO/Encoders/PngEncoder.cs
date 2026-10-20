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
using ChromaFx.IO.Formats;
using ChromaFx.IO.Formats.Png;

namespace ChromaFx.IO.Encoders;

/// <summary>
/// PNG encoder options.
/// </summary>
public sealed class PngEncoder : IImageEncoder
{
    /// <inheritdoc />
    public FileFormats Format => FileFormats.Png;

    /// <inheritdoc />
    public void Encode(Stream stream, Image image)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(image);
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        if (!new PngFormat().Encode(writer, image))
            throw new ImageException("PNG encode failed.");
    }
}
