/*
Copyright 2025 Ho Tzin Mein

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using ChromaFx.Core.Colors;
using ChromaFx.Processing.Filters.Interfaces;
using ChromaFx.Processing.Numerics;
using ChromaFx.Core;
using ChromaFx.Processing.Filters.ColorMatrix;

namespace ChromaFx.Processing.Filters.Overlays;

/// <summary>
/// Blends two images together.
/// </summary>
/// <seealso cref="IFilter"/>
public class Blend : IFilter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Blend"/> class.
    /// </summary>
    /// <param name="image">The image to apply.</param>
    /// <param name="alpha">The alpha value for the image.</param>
    /// <param name="sourceLocation">The source location.</param>
    public Blend(Image image, float alpha, Rectangle sourceLocation = default)
    {
        Alpha = alpha;
        image = image.Copy();
        new Alpha(alpha).Apply(image);
        Image = image;
        SourceLocation = sourceLocation;
        SourceLocation =
            SourceLocation == default
                ? new Rectangle(0, 0, Image.Width, Image.Height)
                : SourceLocation.Clamp(Image);
    }

    /// <summary>
    /// Gets or sets the alpha.
    /// </summary>
    /// <value>The alpha.</value>
    public float Alpha { get; private set; }

    /// <summary>
    /// Gets or sets the image.;
    /// </summary>
    /// <value>The image.</value>
    public Image Image { get; }

    /// <summary>
    /// Gets or sets the source location.
    /// </summary>
    /// <value>The source location.</value>
    public Rectangle SourceLocation { get; }

    /// <summary>
    /// Applies the specified image.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <param name="targetLocation">The target location.</param>
    /// <returns>The image</returns>
    public Image Apply(Image image, Rectangle targetLocation = default)
    {
        targetLocation = targetLocation.Normalize(image);

        for (
            int y1 = targetLocation.Bottom, y2 = SourceLocation.Bottom;
            y1 < targetLocation.Top && y2 < SourceLocation.Top;
            ++y1, ++y2
        )
        {
            var targetRow = new Color[targetLocation.Width];
            var sourceRow = new Color[SourceLocation.Width];

            // Copy data from image to targetRow
            Array.Copy(
                image.Pixels,
                y1 * image.Width + targetLocation.Left,
                targetRow,
                0,
                targetLocation.Width
            );

            // Copy data from Image to sourceRow
            Array.Copy(
                Image.Pixels,
                y2 * Image.Width + SourceLocation.Left,
                sourceRow,
                0,
                SourceLocation.Width
            );

            for (
                int x1 = 0, x2 = 0;
                x1 < targetLocation.Width && x2 < SourceLocation.Width;
                ++x1, ++x2
            )
            {
                var tempAlpha = sourceRow[x2].Alpha / 255f;
                targetRow[x1] = targetRow[x1] * (1f - tempAlpha) + sourceRow[x2] * tempAlpha;
            }

            // Copy the modified row back to the image
            Array.Copy(
                targetRow,
                0,
                image.Pixels,
                y1 * image.Width + targetLocation.Left,
                targetLocation.Width
            );
        }

        return image;
    }
}
