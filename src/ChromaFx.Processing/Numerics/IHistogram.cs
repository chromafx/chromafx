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

namespace ChromaFx.Processing.Numerics;

/// <summary>
/// Histogram interface
/// </summary>
public interface IHistogram
{
    /// <summary>
    /// Equalizes the histogram
    /// </summary>
    /// <returns>this</returns>
    IHistogram Equalize();

    /// <summary>
    /// Equalizes the color.
    /// </summary>
    /// <param name="color">The color.</param>
    /// <returns>The resulting color</returns>
    Color EqualizeColor(Color color);

    /// <summary>
    /// Loads the specified colors.
    /// </summary>
    /// <param name="colors">The colors.</param>
    /// <returns>This</returns>
    IHistogram Load(params Color[] colors);

    /// <summary>
    /// Loads the specified colors without requiring a params array copy.
    /// </summary>
    /// <param name="colors">The colors.</param>
    /// <returns>This</returns>
    IHistogram Load(ReadOnlySpan<Color> colors);

    /// <summary>
    /// Loads an image
    /// </summary>
    /// <param name="image">Image to load</param>
    /// <returns>this</returns>
    IHistogram LoadImage(Image image);

    /// <summary>
    /// Normalizes the histogram
    /// </summary>
    /// <returns>this</returns>
    IHistogram Normalize();
}
