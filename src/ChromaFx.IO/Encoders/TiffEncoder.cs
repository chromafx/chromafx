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
using ChromaFx.IO.Formats.Tiff;

namespace ChromaFx.IO.Encoders;

/// <summary>
/// TIFF encoder (little-endian classic TIFF, 8-bit RGBA).
/// Default <see cref="Compression"/> is <see cref="TiffCompression.None"/>.
/// </summary>
public sealed class TiffEncoder : IImageEncoder
{
    /// <inheritdoc />
    public ImageFormatKey Format => ManagedFormats.Tiff;

    /// <summary>
    /// Strip compression. Default <see cref="TiffCompression.None"/>.
    /// </summary>
    public TiffCompression Compression { get; set; } = TiffCompression.None;

    /// <inheritdoc />
    public void Encode(Stream stream, Image image) =>
        EncoderIO.Write(stream, image, (w, img) => TiffCodec.Encode(w, img, Compression));
}
