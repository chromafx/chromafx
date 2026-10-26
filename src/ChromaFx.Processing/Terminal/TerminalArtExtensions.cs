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

using System.Buffers;
using System.Text;
using ChromaFx.Core;
using ChromaFx.Core.Colors;

namespace ChromaFx.Processing.Terminal;

public static class TerminalArtExtensions
{
    /// <summary>Default dark → light ramp for light terminal backgrounds.</summary>
    private const string DefaultCharacters = "#@%=+*:-. ";

    private const char UpperHalfBlock = '\u2580'; // ▀
    private const char FullBlock = '\u2588'; // █

    /// <summary>
    /// Renders the image as ASCII art using a fixed-width character ramp.
    /// </summary>
    public static string ToAsciiArt(this Image image, int maxWidth = 0) =>
        ToAsciiArt(image, new AsciiArtOptions { MaxWidth = maxWidth });

    /// <summary>
    /// Renders the image as ASCII art using the given options.
    /// </summary>
    /// <remarks>
    /// Output size accounts for <see cref="TerminalArtOptions.CharacterAspect"/>
    /// (default automatic value 2) so typical terminal glyphs look proportional.
    /// Each cell is a box-average of the corresponding source region. Luminance
    /// uses ITU-R BT.601 weights and composites against
    /// <see cref="AsciiArtOptions.BackgroundColor"/>.
    /// Prefer <see cref="AppendAsciiArt"/> when redrawing into a reused buffer.
    /// </remarks>
    public static string ToAsciiArt(this Image image, AsciiArtOptions options)
    {
        var builder = new StringBuilder();
        AppendAsciiArt(image, builder, options);
        return builder.ToString();
    }

    /// <summary>
    /// Appends ASCII art to <paramref name="destination"/> (does not clear it).
    /// </summary>
    public static void AppendAsciiArt(
        this Image image,
        StringBuilder destination,
        AsciiArtOptions options
    )
    {
        ArgumentNullException.ThrowIfNull(image);
        ArgumentNullException.ThrowIfNull(destination);
        ArgumentNullException.ThrowIfNull(options);
        if (image.Width <= 0 || image.Height <= 0)
            return;

        var characters = options.Characters;
        if (string.IsNullOrEmpty(characters))
            characters = DefaultCharacters;
        ReadOnlySpan<char> ramp = characters.AsSpan();
        var lastIndex = ramp.Length - 1;
        var invert = options.Invert;
        var background = ResolveAsciiBackground(options);
        var characterAspect = TerminalArtLayout.ResolveCharacterAspect(options);

        var layout = TerminalArtLayout.Compute(
            image.Width,
            image.Height,
            options.MaxWidth,
            options.MaxHeight,
            characterAspect,
            options.SizeMode
        );

        destination.EnsureCapacity(
            destination.Length + layout.OutputWidth * (layout.OutputHeight + 1)
        );
        var scaleX = (double)image.Width / layout.ContentWidth;
        var scaleY = (double)image.Height / layout.ContentHeight;
        var padChar = ColorToRampChar(background, ramp, lastIndex, invert);

        for (var y = 0; y < layout.OutputHeight; y++)
        {
            var cy = y - layout.PadTop;
            for (var x = 0; x < layout.OutputWidth; x++)
            {
                var cx = x - layout.PadLeft;
                if ((uint)cx >= (uint)layout.ContentWidth || (uint)cy >= (uint)layout.ContentHeight)
                {
                    destination.Append(padChar);
                    continue;
                }

                var color = TerminalArtSampling.GetBoxAveragedColor(
                    image,
                    cx * scaleX,
                    cy * scaleY,
                    (cx + 1) * scaleX,
                    (cy + 1) * scaleY,
                    background
                );
                destination.Append(ColorToRampChar(color, ramp, lastIndex, invert));
            }

            destination.AppendLine();
        }
    }

    /// <summary>
    /// Writes ASCII art into <paramref name="destination"/>.
    /// </summary>
    public static void WriteAsciiArt(
        this Image image,
        IBufferWriter<char> destination,
        AsciiArtOptions options
    )
    {
        ArgumentNullException.ThrowIfNull(destination);
        var builder = new StringBuilder();
        AppendAsciiArt(image, builder, options);
        CopyToWriter(destination, builder);
    }

    /// <summary>
    /// Renders the image as ANSI terminal art for modern terminals.
    /// </summary>
    public static string ToAnsiArt(this Image image, int maxWidth = 0) =>
        ToAnsiArt(image, new AnsiArtOptions { MaxWidth = maxWidth });

    /// <summary>
    /// Renders the image as ANSI terminal art using the given options.
    /// </summary>
    /// <remarks>
    /// By default uses upper half blocks (<c>▀</c>) with 24-bit foreground and
    /// background colors so each character covers two vertical samples — suited
    /// to album-art thumbnails and other TUI image panels. Automatic
    /// <see cref="TerminalArtOptions.CharacterAspect"/> is 1 for half-blocks and
    /// 2 for full blocks. Set <see cref="AnsiArtOptions.ColorMode"/> to
    /// <see cref="AnsiColorMode.Ansi256"/> for broader terminal compatibility.
    /// Prefer <see cref="AppendAnsiArt"/> when redrawing into a reused buffer.
    /// </remarks>
    public static string ToAnsiArt(this Image image, AnsiArtOptions options)
    {
        var builder = new StringBuilder();
        AppendAnsiArt(image, builder, options);
        return builder.ToString();
    }

    /// <summary>
    /// Appends ANSI art to <paramref name="destination"/> (does not clear it).
    /// </summary>
    public static void AppendAnsiArt(
        this Image image,
        StringBuilder destination,
        AnsiArtOptions options
    )
    {
        ArgumentNullException.ThrowIfNull(image);
        ArgumentNullException.ThrowIfNull(destination);
        ArgumentNullException.ThrowIfNull(options);
        if (image.Width <= 0 || image.Height <= 0)
            return;

        var useHalfBlocks = options.UseHalfBlocks;
        var characterAspect = TerminalArtLayout.ResolveCharacterAspect(options);

        var layout = TerminalArtLayout.Compute(
            image.Width,
            image.Height,
            options.MaxWidth,
            options.MaxHeight,
            characterAspect,
            options.SizeMode
        );
        var colorHeight = useHalfBlocks ? layout.ContentHeight * 2 : layout.ContentHeight;
        var bg = options.BackgroundColor;
        var background = new Color(bg.Red, bg.Green, bg.Blue, 255);
        var colorMode = options.ColorMode;

        destination.EnsureCapacity(
            destination.Length + layout.OutputWidth * layout.OutputHeight * 42 + layout.OutputHeight * 8
        );
        var scaleX = (double)image.Width / layout.ContentWidth;
        var scaleY = (double)image.Height / Math.Max(1, colorHeight);

        int? lastFg = null;
        int? lastBg = null;

        for (var y = 0; y < layout.OutputHeight; y++)
        {
            lastFg = null;
            lastBg = null;
            var cy = y - layout.PadTop;

            for (var x = 0; x < layout.OutputWidth; x++)
            {
                var cx = x - layout.PadLeft;
                var inContent =
                    (uint)cx < (uint)layout.ContentWidth && (uint)cy < (uint)layout.ContentHeight;

                if (useHalfBlocks)
                {
                    Color top;
                    Color bottom;
                    if (!inContent)
                    {
                        top = background;
                        bottom = background;
                    }
                    else
                    {
                        var srcX0 = cx * scaleX;
                        var srcX1 = (cx + 1) * scaleX;
                        var topRow = cy * 2;
                        top = TerminalArtSampling.SampleColorRow(
                            image,
                            srcX0,
                            srcX1,
                            topRow,
                            scaleY,
                            background
                        );
                        bottom = TerminalArtSampling.SampleColorRow(
                            image,
                            srcX0,
                            srcX1,
                            topRow + 1,
                            scaleY,
                            background
                        );
                    }

                    AnsiSgr.AppendColor(destination, ref lastFg, isForeground: true, top, colorMode);
                    AnsiSgr.AppendColor(
                        destination,
                        ref lastBg,
                        isForeground: false,
                        bottom,
                        colorMode
                    );
                    destination.Append(UpperHalfBlock);
                }
                else
                {
                    var color = inContent
                        ? TerminalArtSampling.SampleColorRow(
                            image,
                            cx * scaleX,
                            (cx + 1) * scaleX,
                            cy,
                            scaleY,
                            background
                        )
                        : background;
                    AnsiSgr.AppendColor(
                        destination,
                        ref lastFg,
                        isForeground: true,
                        color,
                        colorMode
                    );
                    destination.Append(FullBlock);
                }
            }

            destination.Append(AnsiSgr.Reset);
            destination.AppendLine();
        }
    }

    /// <summary>
    /// Writes ANSI art into <paramref name="destination"/>.
    /// </summary>
    public static void WriteAnsiArt(
        this Image image,
        IBufferWriter<char> destination,
        AnsiArtOptions options
    )
    {
        ArgumentNullException.ThrowIfNull(destination);
        var builder = new StringBuilder();
        AppendAnsiArt(image, builder, options);
        CopyToWriter(destination, builder);
    }

    private static void CopyToWriter(IBufferWriter<char> destination, StringBuilder builder)
    {
        var length = builder.Length;
        if (length == 0)
            return;

        var span = destination.GetSpan(length);
        builder.CopyTo(0, span, length);
        destination.Advance(length);
    }

    private static Color ResolveAsciiBackground(AsciiArtOptions options)
    {
        if (options.BackgroundColor.IntData != 0)
        {
            var c = options.BackgroundColor;
            return new Color(c.Red, c.Green, c.Blue, 255);
        }

        return options.Invert ? new Color(0, 0, 0, 255) : new Color(255, 255, 255, 255);
    }

    private static char ColorToRampChar(
        Color color,
        ReadOnlySpan<char> ramp,
        int lastIndex,
        bool invert
    )
    {
        var luminance = (77 * color.Red + 150 * color.Green + 29 * color.Blue) >> 8;
        var characterIndex = (luminance * lastIndex + 127) / 255;
        if ((uint)characterIndex > (uint)lastIndex)
            characterIndex = lastIndex;
        if (invert)
            characterIndex = lastIndex - characterIndex;
        return ramp[characterIndex];
    }
}
