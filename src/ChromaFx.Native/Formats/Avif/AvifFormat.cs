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

namespace ChromaFx.Native.Formats.Avif;

/// <summary>
/// Stub AVIF format (ISO BMFF ftyp/avif brands and .avif extension). Codecs are not packaged yet.
/// </summary>
public sealed class AvifFormat : IImageFormat
{
    /// <inheritdoc />
    public FileFormats Format => FileFormats.Avif;

    /// <inheritdoc />
    public bool CanDecode(string fileName) =>
        fileName.EndsWith(".avif", StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public bool CanDecode(byte[] header) => LooksLikeAvif(header);

    /// <inheritdoc />
    public bool CanDecode(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanSeek)
            return false;

        var pos = stream.Position;
        try
        {
            Span<byte> header = stackalloc byte[32];
            var read = stream.Read(header);
            return read >= 12 && LooksLikeAvif(header[..read]);
        }
        finally
        {
            stream.Position = pos;
        }
    }

    /// <inheritdoc />
    public bool CanEncode(string fileName) =>
        fileName.EndsWith(".avif", StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public Image Decode(Stream stream) => throw NativeNotReady();

    /// <inheritdoc />
    public void Encode(BinaryWriter writer, Image image) => throw NativeNotReady();

    private static bool LooksLikeAvif(ReadOnlySpan<byte> header)
    {
        // ISO BMFF: size(4) + 'ftyp'(4) + brand…
        if (header.Length < 12)
            return false;
        if (
            header[4] != (byte)'f'
            || header[5] != (byte)'t'
            || header[6] != (byte)'y'
            || header[7] != (byte)'p'
        )
            return false;

        // Major brand or compatible brands may include avif / avis / mif1.
        ReadOnlySpan<byte> haystack = header[8..];
        return ContainsFourCC(haystack, "avif"u8)
            || ContainsFourCC(haystack, "avis"u8)
            || ContainsFourCC(haystack, "mif1"u8);
    }

    private static bool ContainsFourCC(ReadOnlySpan<byte> data, ReadOnlySpan<byte> fourCC)
    {
        for (var i = 0; i + 4 <= data.Length; i++)
        {
            if (data.Slice(i, 4).SequenceEqual(fourCC))
                return true;
        }

        return false;
    }

    private static NotSupportedException NativeNotReady() =>
        new(
            "ChromaFx.Native AVIF codecs are not packaged yet. Decode/Encode will be enabled in a later release."
        );
}
