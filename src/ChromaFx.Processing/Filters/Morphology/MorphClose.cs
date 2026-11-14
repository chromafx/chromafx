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
using ChromaFx.Processing.Numerics;

namespace ChromaFx.Processing.Filters.Morphology;

/// <summary>
/// Morphological closing: dilate then erode (constrict). Fills small dark gaps.
/// </summary>
/// <param name="apertureRadius">Kernel radius (≥ 1). Default 1.</param>
public sealed class MorphClose(int apertureRadius = 1) : IFilter
{
    /// <summary>
    /// Gets the aperture radius.
    /// </summary>
    public int ApertureRadius { get; } = Math.Max(1, apertureRadius);

    /// <inheritdoc />
    public Image Apply(Image image, Rectangle targetLocation = default)
    {
        new Dilate(ApertureRadius).Apply(image, targetLocation);
        new Constrict(ApertureRadius).Apply(image, targetLocation);
        return image;
    }
}
