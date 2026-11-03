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
using ChromaFx.Processing.Filters.Interfaces;
using ChromaFx.Processing.Numerics;

namespace ChromaFx.PixelArt.Filters;

/// <summary>
/// Scale8x helper: three successive <see cref="Scale2x"/> passes (8× width and height).
/// </summary>
/// <seealso cref="IFilter"/>
public class Scale8x : IFilter
{
    /// <inheritdoc cref="Scale2x.Apply"/>
    public Image Apply(Image image, Rectangle targetLocation = default) =>
        Scale2xHelpers.ApplyPasses(image, passCount: 3);
}
