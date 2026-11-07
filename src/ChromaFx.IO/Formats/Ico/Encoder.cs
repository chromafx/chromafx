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

namespace ChromaFx.IO.Formats.Ico;

/// <summary>
/// ICO encoder (one PNG image, at most 256×256).
/// </summary>
internal sealed class Encoder : IEncoder
{
    /// <inheritdoc />
    public bool CanEncode(string fileName) =>
        !string.IsNullOrEmpty(fileName) && fileName.EndsWith(".ico", StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public void Encode(BinaryWriter writer, Image image) => IcoCodec.Encode(writer, image);

    /// <inheritdoc />
    public void Encode(BinaryWriter writer, Animation animation) =>
        throw new NotSupportedException("ICO does not support animation.");
}
