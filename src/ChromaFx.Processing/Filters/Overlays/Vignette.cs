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

using System.Numerics;
using ChromaFx.Core;
using ChromaFx.Core.Colors;
using ChromaFx.Processing.Filters.Interfaces;
using ChromaFx.Processing.Numerics;

namespace ChromaFx.Processing.Filters.Overlays;

/// <summary>
/// Adds a vignette effect to an image
/// </summary>
/// <seealso cref="IFilter"/>
/// <remarks>
/// Initializes a new instance of the <see cref="Vignette"/> class.
/// </remarks>
/// <param name="color">The vignette color.</param>
/// <param name="xRadius">The x radius (between 0 and 1).</param>
/// <param name="yRadius">The y radius (between 0 and 1).</param>
public class Vignette(Color color, float xRadius, float yRadius) : IFilter
{

    /// <summary>
    /// Gets or sets the color.
    /// </summary>
    /// <value>The color.</value>
    public Color Color { get; } = color;

    /// <summary>
    /// Gets the x radius.
    /// </summary>
    /// <value>The x radius.</value>
    public float XRadius { get; } = xRadius > 0 ? xRadius : 0.5f;

    /// <summary>
    /// Gets the y radius.
    /// </summary>
    /// <value>The y radius.</value>
    public float YRadius { get; } = yRadius > 0 ? yRadius : 0.5f;

    /// <summary>
    /// Applies the specified image.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <param name="targetLocation">The target location.</param>
    /// <returns>The image</returns>
    public Image Apply(Image image, Rectangle targetLocation = default)
    {
        targetLocation = targetLocation.Normalize(image);
        var tempX = XRadius * image.Width;
        var tempY = YRadius * image.Height;
        var maxDistance = (float)Math.Sqrt(tempX * tempX + tempY * tempY);

        Parallel.For(
            targetLocation.Bottom,
            targetLocation.Top,
            y =>
            {
                for (var x = targetLocation.Left; x < targetLocation.Right; ++x)
                {
                    var distance = Vector2.Distance(image.Center, new Vector2(x, y));
                    var sourceColor = (Vector4)image.Pixels[y * image.Width + x];
                    var result = Vector4.Lerp(
                        Color,
                        sourceColor,
                        1 - 0.9f * (distance / maxDistance)
                    );
                    var tempAlpha = result.W;
                    result = sourceColor * (1 - tempAlpha) + result * sourceColor * tempAlpha;
                    image.Pixels[y * image.Width + x] = result;
                }
            }
        );

        return image;
    }
}
