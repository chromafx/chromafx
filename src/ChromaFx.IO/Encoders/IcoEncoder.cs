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
using ChromaFx.IO.Formats.Ico;

namespace ChromaFx.IO.Encoders;

/// <summary>
/// ICO encoder. Each image is stored as PNG, at most 256×256.
/// Use the multi-image <c>Encode</c> overload for multi-size icons.
/// </summary>
public sealed class IcoEncoder : IImageEncoder
{
    /// <inheritdoc />
    public ImageFormatKey Format => ManagedFormats.Ico;

    /// <inheritdoc />
    public void Encode(Stream stream, Image image) =>
        EncoderIO.Write(stream, image, (w, img) => new IcoFormat().Encode(w, img));

    /// <summary>
    /// Encodes one or more PNG images into a single ICO (1–256 entries, each ≤256×256).
    /// Decode still returns the largest directory entry.
    /// </summary>
    public void Encode(Stream stream, IReadOnlyList<Image> images) =>
        EncoderIO.Write(stream, images, IcoCodec.Encode);
}
