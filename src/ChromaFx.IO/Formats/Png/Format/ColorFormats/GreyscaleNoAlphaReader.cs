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
/// Greyscale no alpha reader
/// </summary>
/// <seealso cref="IColorReader"/>
internal class GreyscaleNoAlphaReader : IColorReader
{
    /// <inheritdoc />
    public void ReadScanline(ReadOnlySpan<byte> scanline, Span<Color> row, Header header)
    {
        // Sub-byte samples are scaled to 0–255; 16-bit samples keep the high byte.
        byte[] rented = null;
        try
        {
            var scale = 1;
            var step = 1;
            if (header.BitDepth < 8)
            {
                int expandedLen = scanline.Length * 8 / header.BitDepth;
                rented = ArrayPool<byte>.Shared.Rent(expandedLen);
                scanline.ExpandArrayTo(header.BitDepth, rented.AsSpan(0, expandedLen));
                scanline = rented.AsSpan(0, expandedLen);
                scale = 255 / ((1 << header.BitDepth) - 1);
            }
            else if (header.BitDepth == 16)
            {
                step = 2;
            }

            var dest = MemoryMarshal.Cast<Color, uint>(row);
            for (var x = 0; x < header.Width; ++x)
            {
                var v = (uint)(scanline[x * step] * scale);
                dest[x] = v | (v << 8) | (v << 16) | 0xFF00_0000u;
            }
        }
        finally
        {
            if (rented is not null)
                ArrayPool<byte>.Shared.Return(rented);
        }
    }
}
