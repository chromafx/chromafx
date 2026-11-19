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
/// VGA palette modes for <see cref="VgaQuantize"/>.
/// </summary>
public enum VgaPaletteMode
{
    /// <summary>
    /// Full 6-bit DAC cube (64³ ≈ 262 144 levels). Snaps every pixel to multiples of 4.
    /// </summary>
    DacLevels,

    /// <summary>
    /// Mode 13h-style: snap to the 6-bit DAC, then reduce to 256 adaptive palette colors
    /// (octree) with entries kept on DAC levels.
    /// </summary>
    Adaptive256,
}
