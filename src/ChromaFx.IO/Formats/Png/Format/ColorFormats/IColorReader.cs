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

using ChromaFx.Core.Colors;

namespace ChromaFx.IO.Formats.Png.Format.ColorFormats;

/// <summary>
/// Color reader interface
/// </summary>
internal interface IColorReader
{
    /// <summary>
    /// Reads a scan line into the destination image row.
    /// </summary>
    /// <param name="scanline">Scanline</param>
    /// <param name="row">Destination row (length = header.Width)</param>
    /// <param name="header">Header information</param>
    void ReadScanline(ReadOnlySpan<byte> scanline, Span<Color> row, Header header);
}
