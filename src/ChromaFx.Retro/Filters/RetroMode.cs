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
using ChromaFx.Processing.Filters;
using ChromaFx.Processing.Filters.Resampling;
using ChromaFx.Processing.Numerics;

namespace ChromaFx.Retro.Filters;

/// <summary>
/// Nearest-neighbor resize to a target resolution (e.g. 320×200 Mode 13h-style).
/// Fun resolution snap — not a hardware mode emulator.
/// </summary>
/// <param name="width">Target width (≥ 1). Default 320.</param>
/// <param name="height">Target height (≥ 1). Default 200.</param>
public sealed class RetroMode(int width = 320, int height = 200) : IFilter
{
    /// <summary>
    /// Gets the target width.
    /// </summary>
    public int Width { get; } = width;

    /// <summary>
    /// Gets the target height.
    /// </summary>
    public int Height { get; } = height;

    /// <summary>
    /// Applies nearest-neighbor resize to the whole image (partial <paramref name="targetLocation"/> is ignored).
    /// </summary>
    public Image Apply(Image image, Rectangle targetLocation = default)
    {
        if (image.Width <= 0 || image.Height <= 0)
            return image;

        var w = Math.Max(1, Width);
        var h = Math.Max(1, Height);
        if (image.Width == w && image.Height == h)
            return image;

        new Resize(w, h, ResamplingFilter.NearestNeighbor).Apply(image);
        return image;
    }
}
