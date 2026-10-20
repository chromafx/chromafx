/*
 * Copyright 2017–2020 JaCraig
 * Modifications Copyright 2023–2026 Ho Tzin Mein
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
using ChromaFx.Core.Colors;
using ChromaFx.Processing.Simd;

namespace ChromaFx.Processing.Filters.Arithmetic;

/// <summary>
/// Does a subtract operation between two images.
/// </summary>
/// <seealso cref="IFilter"/>
/// <remarks>
/// Initializes a new instance of the <see cref="Subtract"/> class.
/// </remarks>
/// <param name="secondImage">The second image.</param>
public class Subtract(Image secondImage) : ArithmeticFilterBase(secondImage)
{
    /// <inheritdoc />
    protected override void ProcessRow(Span<Color> destination, ReadOnlySpan<Color> source) =>
        PixelSimd.SubtractSaturating(destination, source);
}
