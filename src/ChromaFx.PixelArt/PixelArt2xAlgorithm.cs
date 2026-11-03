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

namespace ChromaFx.PixelArt;

/// <summary>
/// 2× pixel-art upscalers available for repeated passes (e.g. RotSprite pre-scale).
/// </summary>
public enum PixelArt2xAlgorithm
{
    /// <summary>Classic Scale2x (EPX); exact matches, no blending.</summary>
    Scale2x = 0,

    /// <summary>Classic Eagle; exact matches, no blending.</summary>
    Eagle,

    /// <summary>SuperEagle; may blend edge colors.</summary>
    SuperEagle,

    /// <summary>Classic 2×SaI; may blend edge colors.</summary>
    Scale2xSaI,

    /// <summary>Super2xSaI; may blend edge colors.</summary>
    Super2xSaI,
}
