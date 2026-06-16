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
using System.Text;

namespace ChromaFx.Processing;

public static class ImageProcessingExtensions
{
    private static readonly string[] DefaultCharacters =
        ["#", "@", "%", "=", "+", "*", ":", "-", ".", " "];

    /// <summary>
    /// Renders the image as ASCII art using a fixed-width character ramp.
    /// </summary>
    /// <param name="image">The image to render.</param>
    /// <param name="maxWidth">
    /// Maximum output width in characters. When zero, the image width is used.
    /// </param>
    /// <returns>A multi-line ASCII representation of the image.</returns>
    /// <remarks>
    /// Pairs of source rows are averaged to approximate the ~2:1 height-to-width ratio of
    /// typical terminal characters. Luminance follows ITU-R BT.601 weighting.
    /// </remarks>
    public static string ToAsciiArt(this Image image, int maxWidth = 0)
    {
        ArgumentNullException.ThrowIfNull(image);
        if (image.Pixels is null || image.Width <= 0 || image.Height <= 0)
            return string.Empty;

        var sourceWidth = image.Width;
        var stepX = 1;
        var outputWidth = sourceWidth;
        if (maxWidth > 0 && sourceWidth > maxWidth)
        {
            stepX = (int)Math.Ceiling((double)sourceWidth / maxWidth);
            outputWidth = (sourceWidth + stepX - 1) / stepX;
        }

        const int verticalStep = 2;
        var outputLines = (image.Height + verticalStep - 1) / verticalStep;
        var builder = new StringBuilder(outputWidth * (outputLines + 1));
        var lastCharacterIndex = DefaultCharacters.Length - 1;

        for (var y = 0; y < image.Height; y += verticalStep)
        {
            for (var x = 0; x < sourceWidth; x += stepX)
            {
                var luminance = GetAverageLuminance(image, x, y, stepX, verticalStep);
                var characterIndex = (int)(luminance * lastCharacterIndex);
                if (characterIndex > lastCharacterIndex)
                    characterIndex = lastCharacterIndex;
                else if (characterIndex < 0)
                    characterIndex = 0;

                builder.Append(DefaultCharacters[characterIndex]);
            }

            builder.AppendLine();
        }

        return builder.ToString();
    }

    private static float GetAverageLuminance(
        Image image,
        int x,
        int y,
        int stepX,
        int verticalStep
    )
    {
        var sum = 0f;
        var count = 0;
        var maxY = Math.Min(y + verticalStep, image.Height);
        var maxX = Math.Min(x + stepX, image.Width);

        for (var sy = y; sy < maxY; sy++)
        {
            var rowStart = sy * image.Width;
            for (var sx = x; sx < maxX; sx++)
            {
                sum += GetLuminance(image.Pixels[rowStart + sx]);
                count++;
            }
        }

        return count == 0 ? 0f : sum / count;
    }

    private static float GetLuminance(Color pixel)
    {
        var red = pixel.Red / 255f;
        var green = pixel.Green / 255f;
        var blue = pixel.Blue / 255f;
        return 0.299f * red + 0.587f * green + 0.114f * blue;
    }
}
