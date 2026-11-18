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

using System.Buffers;
using System.Runtime.InteropServices;
using ChromaFx.Core;
using ChromaFx.Core.Colors;

namespace ChromaFx.IO.Formats.Png.Format.ColorFormats;

/// <summary>
/// True color with alpha reader
/// </summary>
/// <seealso cref="IColorReader"/>
internal class TrueColorAlphaReader : IColorReader
{
    /// <inheritdoc />
    public void ReadScanline(ReadOnlySpan<byte> scanline, Span<Color> row, Header header)
    {
        byte[] rented = null;
        try
        {
            if (header.BitDepth < 8)
            {
                int expandedLen = scanline.Length * 8 / header.BitDepth;
                rented = ArrayPool<byte>.Shared.Rent(expandedLen);
                scanline.ExpandArrayTo(header.BitDepth, rented.AsSpan(0, expandedLen));
                scanline = rented.AsSpan(0, expandedLen);
            }

            // 8-bit RGBA matches Color memory layout — bulk copy.
            if (header.BitDepth == 8)
            {
                var dest = MemoryMarshal.AsBytes(row);
                scanline[..(header.Width * 4)].CopyTo(dest);
                return;
            }

            var bytesPerChannel = header.BitDepth / 8;
            var scanlineIndex = 0;

            for (var pixelIndex = 0; pixelIndex < header.Width; pixelIndex++)
            {
                row[pixelIndex].Red = scanline[scanlineIndex];
                scanlineIndex += bytesPerChannel;
                row[pixelIndex].Green = scanline[scanlineIndex];
                scanlineIndex += bytesPerChannel;
                row[pixelIndex].Blue = scanline[scanlineIndex];
                scanlineIndex += bytesPerChannel;
                row[pixelIndex].Alpha = scanline[scanlineIndex];
                scanlineIndex += bytesPerChannel;
            }
        }
        finally
        {
            if (rented is not null)
                ArrayPool<byte>.Shared.Return(rented);
        }
    }
}
