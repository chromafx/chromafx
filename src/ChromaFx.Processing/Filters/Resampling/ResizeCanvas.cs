/*
 * Copyright 2017–2020 JaCraig
 * Modifications Copyright 2023–2025 Ho Tzin Mein
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
using ChromaFx.Processing.Filters.Interfaces;
using ChromaFx.Processing.Filters.Resampling.Enums;
using ChromaFx.Processing.Numerics;

namespace ChromaFx.Processing.Filters.Resampling;

/// <summary>
/// Resizes the canvas
/// </summary>
/// <seealso cref="IFilter"/>
/// <remarks>
/// Initializes a new instance of the <see cref="ResizeCanvas"/> class.
/// </remarks>
/// <param name="width">The width.</param>
/// <param name="height">The height.</param>
/// <param name="options">The options.</param>
public class ResizeCanvas(int width, int height, ResizeOptions options = ResizeOptions.UpperLeft) : IFilter
{

    /// <summary>
    /// Gets or sets the height.
    /// </summary>
    /// <value>The height.</value>
    public int Height { get; set; } = height;

    /// <summary>
    /// Gets or sets the options.
    /// </summary>
    /// <value>The options.</value>
    public ResizeOptions Options { get; set; } = options;

    /// <summary>
    /// Gets or sets the width.
    /// </summary>
    /// <value>The width.</value>
    public int Width { get; set; } = width;

    /// <summary>
    /// Applies the filter to the specified image.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <param name="targetLocation">The target location.</param>
    /// <returns>The image</returns>
    public Image Apply(Image image, Rectangle targetLocation = default)
    {
        var final = new Color[Width * Height];
        var xOffset = 0;
        var yOffset = 0;

        if (Options == ResizeOptions.Center)
        {
            xOffset = (image.Width - Width) / 2;
            yOffset = (image.Height - Height) / 2;
        }

        Parallel.For(
            0,
            Height,
            y =>
            {
                if (y + yOffset >= image.Height || y + yOffset < 0)
                    return;

                for (var x = 0; x < Width; ++x)
                {
                    if (x + xOffset >= image.Width)
                        break;
                    if (x + xOffset < 0)
                        continue;

                    final[y * Width + x] = image.Pixels[(y + yOffset) * image.Width + x + xOffset];
                }
            }
        );

        return image.ReCreate(Width, Height, final);
    }
}
