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

namespace ChromaFx.IO.Formats.Tga;

/// <summary>
/// TGA decoder.
/// </summary>
internal sealed class Decoder : IDecoder
{
    /// <inheritdoc />
    public int HeaderSize => 18;

    /// <inheritdoc />
    public bool CanDecode(string fileName) =>
        !string.IsNullOrEmpty(fileName)
        && (
            fileName.EndsWith(".tga", StringComparison.OrdinalIgnoreCase)
            || fileName.EndsWith(".targa", StringComparison.OrdinalIgnoreCase)
        );

    /// <inheritdoc />
    public bool CanDecode(byte[] header) => header != null && TgaCodec.LooksLike(header);

    /// <inheritdoc />
    public bool CanDecode(Stream stream)
    {
        if (stream == null || !stream.CanSeek || stream.Length < HeaderSize)
            return false;

        stream.Seek(0, SeekOrigin.Begin);
        var header = new byte[HeaderSize];
        try
        {
            stream.ReadExactly(header);
        }
        catch (EndOfStreamException)
        {
            stream.Seek(0, SeekOrigin.Begin);
            return false;
        }

        var ok = TgaCodec.LooksLike(header);
        stream.Seek(0, SeekOrigin.Begin);
        return ok;
    }

    /// <inheritdoc />
    public Image Decode(Stream stream) => TgaCodec.Decode(stream);

    /// <inheritdoc />
    public Animation DecodeAnimation(Stream stream) =>
        throw new NotSupportedException("TGA does not support animation.");
}
