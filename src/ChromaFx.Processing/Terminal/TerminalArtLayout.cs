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

namespace ChromaFx.Processing.Terminal;

internal readonly struct TerminalArtLayout
{
    public int OutputWidth { get; init; }
    public int OutputHeight { get; init; }
    public int ContentWidth { get; init; }
    public int ContentHeight { get; init; }
    public int PadLeft { get; init; }
    public int PadTop { get; init; }

    public static float ResolveCharacterAspect(AsciiArtOptions options)
    {
        if (options.CharacterAspect < 0f)
            throw new ArgumentOutOfRangeException(
                nameof(options),
                options.CharacterAspect,
                "CharacterAspect cannot be negative."
            );
        return options.CharacterAspect > 0f ? options.CharacterAspect : 2f;
    }

    public static float ResolveCharacterAspect(AnsiArtOptions options)
    {
        if (options.CharacterAspect < 0f)
            throw new ArgumentOutOfRangeException(
                nameof(options),
                options.CharacterAspect,
                "CharacterAspect cannot be negative."
            );
        if (options.CharacterAspect > 0f)
            return options.CharacterAspect;
        return options.UseHalfBlocks ? 1f : 2f;
    }

    public static TerminalArtLayout Compute(
        int imageWidth,
        int imageHeight,
        int maxWidth,
        int maxHeight,
        float characterAspect,
        TerminalArtSizeMode sizeMode
    )
    {
        if (sizeMode == TerminalArtSizeMode.Exact)
        {
            if (maxWidth <= 0 || maxHeight <= 0)
                throw new ArgumentException(
                    "Exact size mode requires MaxWidth and MaxHeight greater than zero."
                );

            ComputeFittedSize(
                imageWidth,
                imageHeight,
                maxWidth,
                maxHeight,
                characterAspect,
                out var contentWidth,
                out var contentHeight
            );

            return new TerminalArtLayout
            {
                OutputWidth = maxWidth,
                OutputHeight = maxHeight,
                ContentWidth = contentWidth,
                ContentHeight = contentHeight,
                PadLeft = (maxWidth - contentWidth) / 2,
                PadTop = (maxHeight - contentHeight) / 2,
            };
        }

        ComputeFittedSize(
            imageWidth,
            imageHeight,
            maxWidth,
            maxHeight,
            characterAspect,
            out var width,
            out var height
        );

        return new TerminalArtLayout
        {
            OutputWidth = width,
            OutputHeight = height,
            ContentWidth = width,
            ContentHeight = height,
            PadLeft = 0,
            PadTop = 0,
        };
    }

    private static void ComputeFittedSize(
        int imageWidth,
        int imageHeight,
        int maxWidth,
        int maxHeight,
        float characterAspect,
        out int outputWidth,
        out int outputHeight
    )
    {
        var width = (double)imageWidth;
        var height = imageHeight / (double)characterAspect;

        if (maxWidth > 0 && width > maxWidth)
        {
            var scale = maxWidth / width;
            width *= scale;
            height *= scale;
        }

        if (maxHeight > 0 && height > maxHeight)
        {
            var scale = maxHeight / height;
            width *= scale;
            height *= scale;
        }

        outputWidth = Math.Max(1, (int)Math.Round(width));
        outputHeight = Math.Max(1, (int)Math.Round(height));

        if (maxWidth > 0)
            outputWidth = Math.Min(outputWidth, maxWidth);
        if (maxHeight > 0)
            outputHeight = Math.Min(outputHeight, maxHeight);
    }
}
