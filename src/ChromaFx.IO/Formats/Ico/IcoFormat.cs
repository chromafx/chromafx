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
using ChromaFx.IO.Encoders;

namespace ChromaFx.IO.Formats.Ico;

/// <summary>
/// Windows ICO. Decode uses the largest image (PNG, or uncompressed 1/4/8/24/32-bit BMP with its AND mask).
/// Encode writes PNG image(s), each at most 256×256 (use <see cref="IcoEncoder"/> multi-image overload for multi-size).
/// </summary>
/// <seealso cref="FormatBase"/>
public sealed class IcoFormat : FormatBase
{
    /// <inheritdoc />
    public override ImageFormatKey Format => ManagedFormats.Ico;

    /// <inheritdoc />
    public override IImageEncoder CreateEncoder() => new IcoEncoder();

    /// <inheritdoc />
    public override bool CanEncode(string fileName) => FormatProbe.HasExtension(fileName, ".ico");

    /// <inheritdoc />
    public override void Encode(BinaryWriter writer, Image image) => IcoCodec.Encode(writer, image);

    /// <inheritdoc />
    public override bool CanDecode(string fileName) => CanEncode(fileName);

    /// <inheritdoc />
    public override bool CanDecode(byte[] header) => header != null && IcoCodec.LooksLike(header);

    /// <inheritdoc />
    public override bool CanDecode(Stream stream) =>
        FormatProbe.LooksLikeAtStart(stream, 22, IcoCodec.LooksLike);

    /// <inheritdoc />
    public override Image Decode(Stream stream) => IcoCodec.Decode(stream);
}
