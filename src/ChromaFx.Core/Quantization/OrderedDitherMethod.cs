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

namespace ChromaFx.Core.Quantization;

/// <summary>
/// Ordered (Bayer) dither method shared by Processing <c>Quantize</c> and Retro palette filters.
/// </summary>
public enum OrderedDitherMethod
{
    /// <summary>No ordered dither.</summary>
    None = 0,

    /// <summary>Bayer 4×4 ordered dither.</summary>
    Bayer4x4,

    /// <summary>Bayer 8×8 ordered dither.</summary>
    Bayer8x8,
}
