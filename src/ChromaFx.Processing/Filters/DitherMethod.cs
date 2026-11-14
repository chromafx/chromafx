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

using ChromaFx.Core.Quantization;

namespace ChromaFx.Processing.Filters;

/// <summary>
/// Dither method for <see cref="Quantize"/> (ordered Bayer plus Floyd–Steinberg).
/// Ordered values align with <see cref="OrderedDitherMethod"/>.
/// </summary>
public enum DitherMethod
{
    /// <summary>Nearest palette color only.</summary>
    None = 0,

    /// <summary>Bayer 4×4 ordered dither.</summary>
    Bayer4x4 = OrderedDitherMethod.Bayer4x4,

    /// <summary>Bayer 8×8 ordered dither.</summary>
    Bayer8x8 = OrderedDitherMethod.Bayer8x8,

    /// <summary>Floyd–Steinberg error diffusion.</summary>
    FloydSteinberg = 3,
}
