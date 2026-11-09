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

using System.Runtime.InteropServices;
using ChromaFx.Core.Colors;
using ChromaFx.Core.ExtensionMethods;

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
        var scale = 1;
        var step = 1;
        if (header.BitDepth < 8)
        {
            scanline = scanline.ExpandArray(header.BitDepth);
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
}
