/*
 * Copyright 2017–2020 JaCraig
 * Modifications Copyright 2023–2026 Ho Tzin Mein
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

namespace ChromaFx.IO.Formats;

/// <summary>
/// Format base class
/// </summary>
/// <seealso cref="IImageFormat"/>
public abstract class FormatBase : IImageFormat
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FormatBase"/> class.
    /// </summary>
    protected FormatBase() { }

    /// <summary>
    /// Gets the format.
    /// </summary>
    public abstract ImageFormatKey Format { get; }

    /// <summary>
    /// Creates the typed encoder for this format.
    /// </summary>
    public abstract IImageEncoder CreateEncoder();

    /// <summary>
    /// Gets the decoder (classic File-based formats). Codec formats override the public Decode/CanDecode methods instead.
    /// </summary>
    private protected virtual IDecoder Decoder =>
        throw new NotSupportedException($"{GetType().Name} does not use an IDecoder.");

    /// <summary>
    /// Gets the encoder (classic File-based formats). Codec formats override the public Encode/CanEncode methods instead.
    /// </summary>
    private protected virtual IEncoder Encoder =>
        throw new NotSupportedException($"{GetType().Name} does not use an IEncoder.");

    /// <inheritdoc />
    public virtual bool CanDecode(Stream stream)
    {
        if (stream == null)
            return false;
        return Decoder.CanDecode(stream);
    }

    /// <inheritdoc />
    public virtual bool CanDecode(byte[] header)
    {
        if (header == null)
            return false;
        return Decoder.CanDecode(header);
    }

    /// <inheritdoc />
    public virtual bool CanDecode(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return false;
        return Decoder.CanDecode(fileName);
    }

    /// <inheritdoc />
    public virtual bool CanEncode(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return false;
        return Encoder.CanEncode(fileName);
    }

    /// <inheritdoc />
    public virtual Image Decode(Stream stream) => Decoder.Decode(stream);

    /// <inheritdoc />
    public virtual void Encode(BinaryWriter writer, Image image) => Encoder.Encode(writer, image);
}
