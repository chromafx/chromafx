/*
Copyright 2025 Ho Tzin Mein

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using ChromaFx.IO.Formats.Png.Format.Enums;
using ChromaFx.IO.Formats.Png.Format.Helpers;

namespace ChromaFx.IO.Formats.Png.Format;

/// <summary>
/// Palette class
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="Palette" /> class.
/// </remarks>
/// <param name="data">The data.</param>
/// <param name="type">The type.</param>
public class Palette(byte[] data, PaletteType type)
{

    /// <summary>
    /// Gets or sets the data.
    /// </summary>
    /// <value>
    /// The data.
    /// </value>
    public byte[] Data { get; set; } = data;

    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    /// <value>
    /// The type.
    /// </value>
    public PaletteType Type { get; set; } = type;

    /// <summary>
    /// Performs an implicit conversion from <see cref="Chunk" /> to <see cref="Palette" />.
    /// </summary>
    /// <param name="chunk">The chunk.</param>
    /// <returns>
    /// The result of the conversion.
    /// </returns>
    public static implicit operator Palette(Chunk chunk)
    {
        return new Palette(chunk.Data, chunk.Type == ChunkTypes.Palette ? PaletteType.Color : PaletteType.Alpha);
    }
}