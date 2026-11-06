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
using ChromaFx.IO.Formats;
using ChromaFx.Native.Codecs;
using ChromaFx.Native.Encoders;

namespace ChromaFx.Native.Formats.WebP;

/// <summary>
/// Still WebP (RIFF/WEBP). Animated WebP is rejected.
/// </summary>
public sealed class WebPFormat : IImageFormat
{
    private static readonly byte[] Riff = "RIFF"u8.ToArray();
    private static readonly byte[] Webp = "WEBP"u8.ToArray();

    /// <inheritdoc />
    public ImageFormatKey Format => NativeFormats.WebP;

    /// <inheritdoc />
    public IImageEncoder CreateEncoder() => new WebPEncoder();

    /// <inheritdoc />
    public bool CanDecode(string fileName) =>
        fileName.EndsWith(".webp", StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public bool CanDecode(byte[] header) => LooksLikeWebP(header);

    /// <inheritdoc />
    public bool CanDecode(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanSeek)
            return false;

        var pos = stream.Position;
        try
        {
            Span<byte> header = stackalloc byte[12];
            var read = stream.Read(header);
            return read == 12 && LooksLikeWebP(header);
        }
        finally
        {
            stream.Position = pos;
        }
    }

    /// <inheritdoc />
    public bool CanEncode(string fileName) =>
        fileName.EndsWith(".webp", StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public Image Decode(Stream stream) => WebPCodec.Decode(stream);

    /// <inheritdoc />
    public void Encode(BinaryWriter writer, Image image) =>
        Encode(writer, image, WebPEncoder.DefaultQuality, lossless: false);

    /// <summary>
    /// Encodes a still WebP image. <paramref name="lossless"/> ignores <paramref name="quality"/>.
    /// </summary>
    public void Encode(BinaryWriter writer, Image image, int quality, bool lossless) =>
        WebPCodec.Encode(writer, image, quality, lossless);

    private static bool LooksLikeWebP(ReadOnlySpan<byte> header) =>
        header.Length >= 12
        && header[..4].SequenceEqual(Riff)
        && header[8..12].SequenceEqual(Webp);

}
