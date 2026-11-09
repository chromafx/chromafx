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
/// True color without alpha channel reader
/// </summary>
/// <seealso cref="IColorReader"/>
internal class TrueColorNoAlphaReader : IColorReader
{
    /// <inheritdoc />
    public void ReadScanline(ReadOnlySpan<byte> scanline, Span<Color> row, Header header)
    {
        if (header.BitDepth < 8)
            scanline = scanline.ExpandArray(header.BitDepth);

        if (header.BitDepth == 8)
        {
            var dest = MemoryMarshal.Cast<Color, uint>(row);
            var src = scanline[..(header.Width * 3)];
            var x = 0;
            for (; x <= header.Width - 4; x += 4)
            {
                var o = x * 3;
                dest[x] = PackRgb(src, o);
                dest[x + 1] = PackRgb(src, o + 3);
                dest[x + 2] = PackRgb(src, o + 6);
                dest[x + 3] = PackRgb(src, o + 9);
            }

            for (; x < header.Width; ++x)
                dest[x] = PackRgb(src, x * 3);
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
            row[pixelIndex].Alpha = 255;
        }
    }

    private static uint PackRgb(ReadOnlySpan<byte> rgb, int offset) =>
        rgb[offset]
        | ((uint)rgb[offset + 1] << 8)
        | ((uint)rgb[offset + 2] << 16)
        | 0xFF00_0000u;
}
