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

using ChromaFx.Core;
using ChromaFx.IO.Formats;

namespace ChromaFx.IO.Encoders;

/// <summary>
/// Encodes an <see cref="Image"/> to a stream with format-specific options.
/// </summary>
public interface IImageEncoder
{
    /// <summary>
    /// Gets the file format this encoder produces.
    /// </summary>
    FileFormats Format { get; }

    /// <summary>
    /// Encodes <paramref name="image"/> into <paramref name="stream"/>.
    /// </summary>
    void Encode(Stream stream, Image image);
}
