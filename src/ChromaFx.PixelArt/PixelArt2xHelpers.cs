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
using ChromaFx.PixelArt.Filters;
using ChromaFx.Processing.Filters;

namespace ChromaFx.PixelArt;

/// <summary>
/// Creates and applies 2× pixel-art upscalers (including repeated passes for 4× / 8×).
/// </summary>
public static class PixelArt2xHelpers
{
    /// <summary>
    /// Returns a filter instance for <paramref name="algorithm"/>.
    /// </summary>
    public static IFilter CreateFilter(PixelArt2xAlgorithm algorithm) =>
        algorithm switch
        {
            PixelArt2xAlgorithm.Scale2x => new Scale2x(),
            PixelArt2xAlgorithm.Eagle => new Eagle(),
            PixelArt2xAlgorithm.SuperEagle => new SuperEagle(),
            PixelArt2xAlgorithm.Scale2xSaI => new Scale2xSaI(),
            PixelArt2xAlgorithm.Super2xSaI => new Super2xSaI(),
            _ => throw new ArgumentOutOfRangeException(nameof(algorithm), algorithm, null),
        };

    /// <summary>
    /// Applies <paramref name="algorithm"/> <paramref name="passCount"/> times in place
    /// (1 → 2×, 2 → 4×, 3 → 8×).
    /// </summary>
    public static Image ApplyPasses(
        Image image,
        PixelArt2xAlgorithm algorithm,
        int passCount
    )
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(passCount, 1);
        var filter = CreateFilter(algorithm);
        for (var i = 0; i < passCount; ++i)
            filter.Apply(image);
        return image;
    }
}
