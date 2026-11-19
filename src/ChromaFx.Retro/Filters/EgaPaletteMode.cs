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

namespace ChromaFx.Retro.Filters;

/// <summary>
/// EGA palette modes for <see cref="EgaQuantize"/>.
/// </summary>
public enum EgaPaletteMode
{
    /// <summary>Full 64-color <c>00/55/AA/FF</c> cube (all levels per channel).</summary>
    Full64,

    /// <summary>
    /// Adaptive 16-of-64: histogram of snapped cube colors, keep the 16 most frequent,
    /// remap the rest to nearest selected (EGA 16-color mode style).
    /// </summary>
    Adaptive16,
}
