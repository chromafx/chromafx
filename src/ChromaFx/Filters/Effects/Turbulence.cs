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

using ChromaFx.ExtensionMethods;
using ChromaFx.Filters.Interfaces;
using ChromaFx.Numerics;
using ChromaFx.Procedural;
using ChromaFx.Colors;

namespace ChromaFx.Filters.Effects;

/// <summary>
/// Adds turbulence to an image
/// </summary>
/// <seealso cref="IFilter"/>
/// <remarks>
/// Initializes a new instance of the <see cref="SinWave"/> class.
/// </remarks>
/// <param name="roughness">The roughness.</param>
/// <param name="power">The power.</param>
/// <param name="seed">The seed.</param>
public class Turbulence(int roughness = 8, float power = 0.02f, int seed = 25123864) : IFilter
{

    /// <summary>
    /// Gets or sets the power.
    /// </summary>
    /// <value>The power.</value>
    public float Power { get; set; } = power;

    /// <summary>
    /// Gets or sets the roughness.
    /// </summary>
    /// <value>The roughness.</value>
    public int Roughness { get; set; } = roughness;

    /// <summary>
    /// Gets or sets the seed.
    /// </summary>
    /// <value>The seed.</value>
    public int Seed { get; set; } = seed;

    /// <summary>
    /// Applies the filter to the specified image.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <param name="targetLocation">The target location.</param>
    /// <returns>The image</returns>
    public Image Apply(Image image, Rectangle targetLocation = default)
    {
        targetLocation =
            targetLocation == default
                ? new Rectangle(0, 0, image.Width, image.Height)
                : targetLocation.Clamp(image);
        var result = new Color[image.Pixels.Length];
        Array.Copy(image.Pixels, result, result.Length);
        var xNoise = PerlinNoise.Generate(
            image.Width,
            image.Height,
            255,
            0,
            0.0625f,
            1.0f,
            0.5f,
            Roughness,
            Seed
        );
        var yNoise = PerlinNoise.Generate(
            image.Width,
            image.Height,
            255,
            0,
            0.0625f,
            1.0f,
            0.5f,
            Roughness,
            Seed * 2
        );
        Parallel.For(
            targetLocation.Bottom,
            targetLocation.Top,
            y =>
            {
                for (var x = targetLocation.Left; x < targetLocation.Right; ++x)
                {
                    var xDistortion = x + xNoise.Pixels[y * image.Width + x].Red * Power;
                    var yDistortion = y + yNoise.Pixels[y * image.Width + x].Red * Power;
                    var x1 = (int)xDistortion.Clamp(0, image.Width - 1);
                    var y1 = (int)yDistortion.Clamp(0, image.Height - 1);
                    var resultOffset = y * image.Width + x;
                    var sourceOffset = y1 * image.Width + x1;

                    result[resultOffset] = image.Pixels[sourceOffset];
                }
            }
        );

        return image.ReCreate(image.Width, image.Height, result);
    }
}
