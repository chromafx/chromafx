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
using ChromaFx.IO.Formats.Tga;

namespace ChromaFx.IO.Encoders;

/// <summary>
/// TGA encoder (uncompressed 32-bit top-left).
/// </summary>
public sealed class TgaEncoder : IImageEncoder
{
    /// <inheritdoc />
    public ImageFormatKey Format => ManagedFormats.Tga;

    /// <inheritdoc />
    public void Encode(Stream stream, Image image) =>
        EncoderIO.Write(stream, image, (w, img) => new TgaFormat().Encode(w, img));
}
