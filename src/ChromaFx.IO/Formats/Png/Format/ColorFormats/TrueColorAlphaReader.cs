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
/// True color with alpha reader
/// </summary>
/// <seealso cref="IColorReader"/>
internal class TrueColorAlphaReader : IColorReader
{
    /// <inheritdoc />
    public void ReadScanline(byte[] scanline, Span<Color> row, Header header)
    {
        scanline = scanline.ExpandArray(header.BitDepth);

        // 8-bit RGBA matches Color memory layout — bulk copy.
        if (header.BitDepth == 8)
        {
            var dest = MemoryMarshal.AsBytes(row);
            scanline.AsSpan(0, header.Width * 4).CopyTo(dest);
            return;
        }

        var bytesPerPixel = header.BitDepth * 4 / 8;
        var bytesPerChannel = header.BitDepth / 8;

        int pixelIndex = 0;
        int scanlineIndex = 0;

        for (var x = 0; x < scanline.Length; x += bytesPerPixel)
        {
            row[pixelIndex].Red = scanline[scanlineIndex];
            scanlineIndex += bytesPerChannel;
            row[pixelIndex].Green = scanline[scanlineIndex];
            scanlineIndex += bytesPerChannel;
            row[pixelIndex].Blue = scanline[scanlineIndex];
            scanlineIndex += bytesPerChannel;
            row[pixelIndex].Alpha = scanline[scanlineIndex];
            pixelIndex++;
            scanlineIndex += bytesPerChannel;
        }
    }
}
