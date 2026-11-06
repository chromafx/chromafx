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

namespace ChromaFx.Native.Formats.Jxl;

/// <summary>
/// Still JPEG XL (codestream or container). Animated JPEG XL is rejected.
/// </summary>
public sealed class JxlFormat : IImageFormat
{
    /// <inheritdoc />
    public ImageFormatKey Format => NativeFormats.JpegXl;

    /// <inheritdoc />
    public IImageEncoder CreateEncoder() => new JxlEncoder();

    /// <inheritdoc />
    public bool CanDecode(string fileName) =>
        fileName.EndsWith(".jxl", StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public bool CanDecode(byte[] header) => header != null && LooksLikeJxl(header);

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
            return read >= 2 && LooksLikeJxl(header[..read]);
        }
        finally
        {
            stream.Position = pos;
        }
    }

    /// <inheritdoc />
    public bool CanEncode(string fileName) =>
        fileName.EndsWith(".jxl", StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public Image Decode(Stream stream) => JxlCodec.Decode(stream);

    /// <inheritdoc />
    public void Encode(BinaryWriter writer, Image image) =>
        Encode(writer, image, JxlEncoder.DefaultQuality, lossless: false);

    /// <summary>
    /// Encodes a still JPEG XL image. <paramref name="lossless"/> ignores <paramref name="quality"/>.
    /// </summary>
    public void Encode(BinaryWriter writer, Image image, int quality, bool lossless) =>
        JxlCodec.Encode(writer, image, quality, lossless);

    private static bool LooksLikeJxl(ReadOnlySpan<byte> header)
    {
        if (header.Length >= 2 && header[0] == 0xFF && header[1] == 0x0A)
            return true;
        if (header.Length < 12)
            return false;
        if (
            header[0] == 0
            && header[1] == 0
            && header[2] == 0
            && header[3] == 0x0C
            && header[4] == (byte)'J'
            && header[5] == (byte)'X'
            && header[6] == (byte)'L'
            && header[7] == (byte)' '
        )
            return true;
        return header[4] == (byte)'f'
            && header[5] == (byte)'t'
            && header[6] == (byte)'y'
            && header[7] == (byte)'p'
            && header[8] == (byte)'j'
            && header[9] == (byte)'x'
            && header[10] == (byte)'l';
    }
}
