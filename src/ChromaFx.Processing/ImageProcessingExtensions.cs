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
using ChromaFx.Processing.Filters.ColorMatrix;
using System.Text;

namespace ChromaFx.Processing;

public static class ImageProcessingExtensions
{
    public static string ToAsciiArt(this Image image)
    {
        var AsciiCharacters = new[] { "#", "#", "@", "%", "=", "+", "*", ":", "-", ".", " " };
        var showLine = true;
        var tempImage = new Greyscale601().Apply(image.Copy());
        var builder = new StringBuilder();
        for (var y = 0; y < tempImage.Height; ++y)
        {
            for (var x = 0; x < tempImage.Width; ++x)
            {
                if (!showLine)
                    continue;
                var rValue = tempImage.Pixels[y * tempImage.Width + x].Red / 255f;
                builder.Append(AsciiCharacters[(int)(rValue * AsciiCharacters.Length)]);
            }
            if (showLine)
            {
                builder.AppendLine();
                showLine = false;
            }
            else
            {
                showLine = true;
            }
        }
        return builder.ToString();
    }
}
