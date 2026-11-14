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

using System.Buffers;
using System.Text;
using ChromaFx.Core;
using ChromaFx.Core.Colors;
using ChromaFx.Processing.Terminal;

namespace ChromaFx.Processing.HtmlArt;

/// <summary>
/// Fixed-width colored HTML art rendering (monospace <c>&lt;pre&gt;</c> / <c>&lt;span&gt;</c>).
/// </summary>
public static class HtmlArtExtensions
{
    private const char UpperHalfBlock = '\u2580'; // ▀
    private const char FullBlock = '\u2588'; // █

    private const string DefaultPreStyle =
        "font-family:ui-monospace,Consolas,monospace;line-height:1;letter-spacing:0;margin:0;white-space:pre";

    /// <summary>
    /// Renders the image as fixed-width colored HTML art.
    /// </summary>
    public static string ToHtmlArt(this Image image, int maxWidth = 0) =>
        ToHtmlArt(image, new HtmlArtOptions { MaxWidth = maxWidth });

    /// <summary>
    /// Renders the image as fixed-width colored HTML using the given options.
    /// </summary>
    /// <remarks>
    /// Uses the same half-block / full-block sampling model as terminal ANSI art,
    /// emitting monospace <c>&lt;span&gt;</c> elements with CSS colors (truecolor
    /// <c>#rrggbb</c>). Adjacent cells that share foreground/background are merged
    /// into one span. Prefer <see cref="AppendHtmlArt"/> when redrawing into a
    /// reused buffer.
    /// </remarks>
    public static string ToHtmlArt(this Image image, HtmlArtOptions options)
    {
        var builder = new StringBuilder();
        AppendHtmlArt(image, builder, options);
        return builder.ToString();
    }

    /// <summary>
    /// Appends HTML art to <paramref name="destination"/> (does not clear it).
    /// </summary>
    public static void AppendHtmlArt(
        this Image image,
        StringBuilder destination,
        HtmlArtOptions options
    )
    {
        ArgumentNullException.ThrowIfNull(image);
        ArgumentNullException.ThrowIfNull(destination);
        ArgumentNullException.ThrowIfNull(options);
        if (image.Width <= 0 || image.Height <= 0)
            return;

        var useHalfBlocks = options.UseHalfBlocks;
        var characterAspect = TerminalArtLayout.ResolveCharacterAspect(options, useHalfBlocks);

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

        destination.EnsureCapacity(
            destination.Length
                + layout.OutputWidth * layout.OutputHeight * 48
                + layout.OutputHeight * 8
                + 128
        );
        var scaleX = (double)image.Width / layout.ContentWidth;
        var scaleY = (double)image.Height / Math.Max(1, colorHeight);

        if (options.WrapInPre)
        {
            destination.Append("<pre style=\"");
            destination.Append(DefaultPreStyle);
            destination.Append("\">");
        }

        int? lastFg = null;
        int? lastBg = null;
        var spanOpen = false;

        for (var y = 0; y < layout.OutputHeight; y++)
        {
            lastFg = null;
            lastBg = null;
            spanOpen = false;
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

                    EnsureHtmlSpan(
                        destination,
                        ref lastFg,
                        ref lastBg,
                        ref spanOpen,
                        top,
                        bottom,
                        withBackground: true
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
                    EnsureHtmlSpan(
                        destination,
                        ref lastFg,
                        ref lastBg,
                        ref spanOpen,
                        color,
                        background,
                        withBackground: false
                    );
                    destination.Append(FullBlock);
                }
            }

            if (spanOpen)
            {
                destination.Append("</span>");
                spanOpen = false;
            }

            destination.AppendLine();
        }

        if (options.WrapInPre)
            destination.Append("</pre>");
    }

    /// <summary>
    /// Writes HTML art into <paramref name="destination"/>.
    /// </summary>
    public static void WriteHtmlArt(
        this Image image,
        IBufferWriter<char> destination,
        HtmlArtOptions options
    )
    {
        ArgumentNullException.ThrowIfNull(destination);
        var builder = new StringBuilder();
        AppendHtmlArt(image, builder, options);
        CopyToWriter(destination, builder);
    }

    private static void EnsureHtmlSpan(
        StringBuilder destination,
        ref int? lastFg,
        ref int? lastBg,
        ref bool spanOpen,
        Color foreground,
        Color background,
        bool withBackground
    )
    {
        var fgKey = RgbKey(foreground);
        var bgKey = withBackground ? RgbKey(background) : -1;
        if (spanOpen && lastFg == fgKey && lastBg == bgKey)
            return;

        if (spanOpen)
            destination.Append("</span>");

        destination.Append("<span style=\"color:");
        AppendCssHex(destination, foreground);
        if (withBackground)
        {
            destination.Append(";background-color:");
            AppendCssHex(destination, background);
        }

        destination.Append("\">");
        lastFg = fgKey;
        lastBg = bgKey;
        spanOpen = true;
    }

    private static int RgbKey(Color color) => color.Red << 16 | color.Green << 8 | color.Blue;

    private static void AppendCssHex(StringBuilder destination, Color color)
    {
        destination.Append('#');
        AppendByteHex(destination, color.Red);
        AppendByteHex(destination, color.Green);
        AppendByteHex(destination, color.Blue);
    }

    private static void AppendByteHex(StringBuilder destination, byte value)
    {
        destination.Append(HexNibble(value >> 4));
        destination.Append(HexNibble(value & 0xF));
    }

    private static char HexNibble(int value) =>
        (char)(value < 10 ? '0' + value : 'a' + (value - 10));

    private static void CopyToWriter(IBufferWriter<char> destination, StringBuilder builder)
    {
        var length = builder.Length;
        if (length == 0)
            return;

        var span = destination.GetSpan(length);
        builder.CopyTo(0, span, length);
        destination.Advance(length);
    }
}
