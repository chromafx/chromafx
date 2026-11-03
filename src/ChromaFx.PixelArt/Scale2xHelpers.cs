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

using ChromaFx.Core;

namespace ChromaFx.PixelArt;

/// <summary>
/// Helpers that apply <see cref="Scale2x"/> repeatedly (Scale4x / Scale8x).
/// </summary>
public static class Scale2xHelpers
{
    /// <summary>
    /// Applies <see cref="Scale2x"/> <paramref name="passCount"/> times in place.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <param name="passCount">Number of 2× passes (2 → 4×, 3 → 8×).</param>
    /// <returns>The image.</returns>
    public static Image ApplyPasses(Image image, int passCount) =>
        PixelArt2xHelpers.ApplyPasses(image, PixelArt2xAlgorithm.Scale2x, passCount);
}
