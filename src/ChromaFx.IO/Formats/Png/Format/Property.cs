/*
 * Copyright 2017–2020 JaCraig
 * Modifications Copyright 2023–2025 Ho Tzin Mein
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

using ChromaFx.IO.Formats.Png.Format.Helpers;
using System.Text;

namespace ChromaFx.IO.Formats.Png.Format;

/// <summary>
/// A key, value property
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="Property"/> class.
/// </remarks>
/// <param name="key">The key.</param>
/// <param name="value">The value.</param>
public class Property(string key, string value)
{

    /// <summary>
    /// Gets or sets the key.
    /// </summary>
    /// <value>
    /// The key.
    /// </value>
    public string Key { get; set; } = key ?? string.Empty;

    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    /// <value>
    /// The value.
    /// </value>
    public string Value { get; set; } = value ?? string.Empty;

    /// <summary>
    /// Performs an implicit conversion from <see cref="Chunk"/> to <see cref="Property"/>.
    /// </summary>
    /// <param name="chunk">The chunk.</param>
    /// <returns>
    /// The result of the conversion.
    /// </returns>
    public static implicit operator Property(Chunk chunk)
    {
        var count = 0;
        for (var x = 0; x < chunk.Data.Length; ++x, ++count)
        {
            if (chunk.Data[x] == 0)
                break;
        }

        return new Property(Encoding.UTF8.GetString(chunk.Data, 0, count),
            Encoding.UTF8.GetString(chunk.Data, count + 1, chunk.Data.Length - count - 1));
    }
}