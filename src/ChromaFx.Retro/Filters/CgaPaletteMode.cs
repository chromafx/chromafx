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
/// Historical CGA palette modes for <see cref="CgaQuantize"/>.
/// </summary>
public enum CgaPaletteMode
{
    /// <summary>Standard 16-color RGBI palette.</summary>
    Standard16,

    /// <summary>Palette 1 (Low): Black, Cyan, Magenta, Light Gray.</summary>
    Palette1Low,

    /// <summary>Palette 1 (High): Black, Bright Cyan, Bright Magenta, White.</summary>
    Palette1High,

    /// <summary>Palette 2 (Low): Black, Green, Red, Brown.</summary>
    Palette2Low,

    /// <summary>Palette 2 (High): Black, Bright Green, Bright Red, Yellow.</summary>
    Palette2High,

    /// <summary>2-color monochrome: Black and White.</summary>
    Monochrome2,
}
