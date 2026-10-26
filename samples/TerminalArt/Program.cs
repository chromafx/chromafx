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

using System.Text;
using ChromaFx.Core;
using ChromaFx.Core.Colors;
using ChromaFx.IO;
using ChromaFx.IO.Formats;
using ChromaFx.Processing.Terminal;

var cover = CreateDemoCover(64, 64);
var caps = TerminalImageCapabilities.Detect();

Console.WriteLine(
    $"Capabilities: Kitty={caps.SupportsKitty}, iTerm2={caps.SupportsITerm2}, TrueColor={caps.SupportsTrueColor}, Preferred={caps.Preferred}"
);
Console.WriteLine();

Console.WriteLine("ASCII (dark terminal, width 48):");
Console.Write(
    cover.ToAsciiArt(
        new AsciiArtOptions
        {
            MaxWidth = 48,
            Invert = true,
            Characters = " .:-=+*#%@",
        }
    )
);

Console.WriteLine();
Console.WriteLine("ANSI truecolor half-blocks (exact 24×12):");
var ansi = new StringBuilder();
cover.AppendAnsiArt(
    ansi,
    new AnsiArtOptions
    {
        MaxWidth = 24,
        MaxHeight = 12,
        SizeMode = TerminalArtSizeMode.Exact,
        BackgroundColor = new Color(0, 0, 0),
    }
);
Console.Write(ansi);

Console.WriteLine();
Console.WriteLine($"Preferred protocol ({caps.Preferred}):");
var png = cover.ToByteArray(FileFormats.Png);
switch (caps.Preferred)
{
    case TerminalImageProtocol.Kitty:
        Console.Write(KittyGraphics.FromPng(png, columns: 24, rows: 12));
        break;
    case TerminalImageProtocol.ITerm2:
        Console.Write(ITerm2InlineImage.FromPng(png, columns: 24, rows: 12));
        break;
    case TerminalImageProtocol.AnsiArt:
        Console.Write(
            cover.ToAnsiArt(
                new AnsiArtOptions
                {
                    MaxWidth = 24,
                    MaxHeight = 12,
                    SizeMode = TerminalArtSizeMode.Exact,
                }
            )
        );
        break;
    default:
        Console.Write(cover.ToAsciiArt(new AsciiArtOptions { MaxWidth = 48, Invert = true }));
        break;
}

Console.WriteLine();
Console.WriteLine("Pattern: image.ToByteArray(FileFormats.Png) → KittyGraphics / ITerm2InlineImage.FromPng.");

static Image CreateDemoCover(int width, int height)
{
    var image = new Image(width, height);
    var cx = (width - 1) * 0.5;
    var cy = (height - 1) * 0.5;
    var maxDist = Math.Sqrt(cx * cx + cy * cy);

    for (var y = 0; y < height; y++)
    {
        var row = image.GetRowSpan(y);
        for (var x = 0; x < width; x++)
        {
            var t = (x + y) / (double)(width + height - 2);
            var (r, g, b) = HsvToRgb(t * 360.0, 0.85, 0.95);
            var dx = x - cx;
            var dy = y - cy;
            var vignette = 1.0 - Math.Sqrt(dx * dx + dy * dy) / maxDist * 0.45;
            row[x] = new Color(
                (byte)Math.Clamp((int)(r * vignette), 0, 255),
                (byte)Math.Clamp((int)(g * vignette), 0, 255),
                (byte)Math.Clamp((int)(b * vignette), 0, 255)
            );
        }
    }

    return image;
}

static (byte R, byte G, byte B) HsvToRgb(double h, double s, double v)
{
    h = (h % 360 + 360) % 360;
    var c = v * s;
    var x = c * (1 - Math.Abs(h / 60 % 2 - 1));
    var m = v - c;
    double r1,
        g1,
        b1;
    if (h < 60)
        (r1, g1, b1) = (c, x, 0);
    else if (h < 120)
        (r1, g1, b1) = (x, c, 0);
    else if (h < 180)
        (r1, g1, b1) = (0, c, x);
    else if (h < 240)
        (r1, g1, b1) = (0, x, c);
    else if (h < 300)
        (r1, g1, b1) = (x, 0, c);
    else
        (r1, g1, b1) = (c, 0, x);

    return (
        (byte)Math.Clamp((int)((r1 + m) * 255), 0, 255),
        (byte)Math.Clamp((int)((g1 + m) * 255), 0, 255),
        (byte)Math.Clamp((int)((b1 + m) * 255), 0, 255)
    );
}
