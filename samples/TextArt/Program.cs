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
using ChromaFx.Processing.HtmlArt;
using ChromaFx.Processing.Terminal;

if (!TryParseArgs(args, out var options, out var error))
{
    Console.Error.WriteLine(error);
    PrintUsage();
    return 1;
}

if (options.ShowHelp)
{
    PrintUsage();
    return 0;
}

Image image;
try
{
    image = LoadOrCreateImage(options);
}
catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException or IOException)
{
    Console.Error.WriteLine(ex.Message);
    return 1;
}

var caps = TerminalImageCapabilities.Detect();

Console.WriteLine(
    $"Source: {(options.ImagePath is null ? $"demo {image.Width}×{image.Height}" : options.ImagePath)} ({image.Width}×{image.Height})"
);
Console.WriteLine(
    $"Capabilities: Kitty={caps.SupportsKitty}, iTerm2={caps.SupportsITerm2}, TrueColor={caps.SupportsTrueColor}, Preferred={caps.Preferred}"
);
Console.WriteLine();

Console.WriteLine($"ASCII (width {options.AsciiWidth}{(options.InvertAscii ? ", invert" : "")}):");
Console.Write(
    image.ToAsciiArt(
        new AsciiArtOptions
        {
            MaxWidth = options.AsciiWidth,
            Invert = options.InvertAscii,
            Characters = options.AsciiCharacters,
        }
    )
);

Console.WriteLine();
Console.WriteLine($"ANSI truecolor half-blocks (exact {options.AnsiWidth}×{options.AnsiHeight}):");
var ansi = new StringBuilder();
image.AppendAnsiArt(
    ansi,
    new AnsiArtOptions
    {
        MaxWidth = options.AnsiWidth,
        MaxHeight = options.AnsiHeight,
        SizeMode = TerminalArtSizeMode.Exact,
        BackgroundColor = new Color(0, 0, 0),
    }
);
Console.Write(ansi);

Console.WriteLine();
Console.WriteLine($"Preferred protocol ({caps.Preferred}):");
var png = image.ToByteArray(FileFormats.Png);
switch (caps.Preferred)
{
    case TerminalImageProtocol.Kitty:
        Console.Write(
            KittyGraphics.FromPng(png, columns: options.AnsiWidth, rows: options.AnsiHeight)
        );
        break;
    case TerminalImageProtocol.ITerm2:
        Console.Write(
            ITerm2InlineImage.FromPng(png, columns: options.AnsiWidth, rows: options.AnsiHeight)
        );
        break;
    case TerminalImageProtocol.AnsiArt:
        Console.Write(
            image.ToAnsiArt(
                new AnsiArtOptions
                {
                    MaxWidth = options.AnsiWidth,
                    MaxHeight = options.AnsiHeight,
                    SizeMode = TerminalArtSizeMode.Exact,
                }
            )
        );
        break;
    default:
        Console.Write(
            image.ToAsciiArt(
                new AsciiArtOptions { MaxWidth = options.AsciiWidth, Invert = options.InvertAscii }
            )
        );
        break;
}

Console.WriteLine();
await File.WriteAllTextAsync(
    options.HtmlOutputPath,
    image.ToHtmlArt(
        new HtmlArtOptions
        {
            MaxWidth = options.HtmlWidth,
            MaxHeight = options.HtmlHeight,
            SizeMode = TerminalArtSizeMode.Exact,
        }
    )
);
Console.WriteLine($"HTML art written to {options.HtmlOutputPath}");
return 0;

static Image LoadOrCreateImage(SampleOptions options)
{
    if (options.ImagePath is null)
        return CreateDemoCover(options.DemoWidth, options.DemoHeight);

    if (!File.Exists(options.ImagePath))
        throw new FileNotFoundException($"Image not found: {options.ImagePath}", options.ImagePath);

    return Image.Load(options.ImagePath);
}

static bool TryParseArgs(string[] args, out SampleOptions options, out string? error)
{
    options = SampleOptions.CreateDefaults();
    error = null;

    for (var i = 0; i < args.Length; i++)
    {
        var arg = args[i];
        if (arg is "-h" or "--help")
        {
            options.ShowHelp = true;
            return true;
        }

        if (arg.StartsWith('-'))
        {
            if (arg is "--no-invert")
            {
                options.InvertAscii = false;
                continue;
            }

            if (i + 1 >= args.Length)
            {
                error = $"Missing value for {arg}.";
                return false;
            }

            var value = args[++i];
            switch (arg)
            {
                case "--width" or "-w":
                    if (!TryParsePositiveInt(value, out var asciiWidth))
                    {
                        error = $"Invalid --width: {value}";
                        return false;
                    }
                    options.AsciiWidth = asciiWidth;
                    break;
                case "--height":
                    if (!TryParsePositiveInt(value, out var htmlHeightFromHeight))
                    {
                        error = $"Invalid --height: {value}";
                        return false;
                    }
                    options.HtmlHeight = htmlHeightFromHeight;
                    break;
                case "--ascii-width":
                    if (!TryParsePositiveInt(value, out var asciiWidthExplicit))
                    {
                        error = $"Invalid --ascii-width: {value}";
                        return false;
                    }
                    options.AsciiWidth = asciiWidthExplicit;
                    break;
                case "--ansi-width":
                    if (!TryParsePositiveInt(value, out var ansiWidth))
                    {
                        error = $"Invalid --ansi-width: {value}";
                        return false;
                    }
                    options.AnsiWidth = ansiWidth;
                    break;
                case "--ansi-height":
                    if (!TryParsePositiveInt(value, out var ansiHeight))
                    {
                        error = $"Invalid --ansi-height: {value}";
                        return false;
                    }
                    options.AnsiHeight = ansiHeight;
                    break;
                case "--html-width":
                    if (!TryParsePositiveInt(value, out var htmlWidth))
                    {
                        error = $"Invalid --html-width: {value}";
                        return false;
                    }
                    options.HtmlWidth = htmlWidth;
                    break;
                case "--html-height":
                    if (!TryParsePositiveInt(value, out var htmlHeight))
                    {
                        error = $"Invalid --html-height: {value}";
                        return false;
                    }
                    options.HtmlHeight = htmlHeight;
                    break;
                case "--demo-size":
                    if (!TryParsePositiveInt(value, out var demo))
                    {
                        error = $"Invalid --demo-size: {value}";
                        return false;
                    }
                    options.DemoWidth = demo;
                    options.DemoHeight = demo;
                    break;
                case "--out" or "-o":
                    options.HtmlOutputPath = Path.GetFullPath(value);
                    break;
                case "--chars":
                    if (string.IsNullOrEmpty(value))
                    {
                        error = "--chars cannot be empty.";
                        return false;
                    }
                    options.AsciiCharacters = value;
                    break;
                default:
                    error = $"Unknown option: {arg}";
                    return false;
            }

            continue;
        }

        if (options.ImagePath is not null)
        {
            error = $"Unexpected argument: {arg}";
            return false;
        }

        options.ImagePath = Path.GetFullPath(arg);
    }

    // --width without --html-width also drives HTML width for a single knob.
    if (args.Any(a => a is "--width" or "-w") && !args.Any(a => a == "--html-width"))
        options.HtmlWidth = options.AsciiWidth;

    if (args.Any(a => a == "--height") && !args.Any(a => a == "--html-height"))
    {
        // already assigned HtmlHeight
    }

    return true;
}

static bool TryParsePositiveInt(string text, out int value)
{
    if (int.TryParse(text, out value) && value > 0)
        return true;
    value = 0;
    return false;
}

static void PrintUsage()
{
    Console.WriteLine(
        """
        TextArt — ASCII / ANSI / HTML / Kitty / iTerm2 preview for ChromaFx

        Usage:
          dotnet run --project samples/TextArt -- [image] [options]

        Arguments:
          image                 Path to an image (PNG/JPEG/…). Omit for a generated demo.

        Options:
          -w, --width <n>       ASCII (and HTML, unless --html-width) max width (default 48)
          --height <n>          HTML max height (default 24)
          --ascii-width <n>     ASCII max width (default 48)
          --ansi-width <n>      ANSI / Kitty / iTerm2 columns (default 24)
          --ansi-height <n>     ANSI / Kitty / iTerm2 rows (default 12)
          --html-width <n>      HTML max width (default 48)
          --html-height <n>     HTML max height (default 24)
          --demo-size <n>       Generated demo side length when no image (default 64)
          -o, --out <path>      HTML output path (default ./text-art.html under the app dir)
          --chars <ramp>        ASCII character ramp (default " .:-=+*#%@")
          --no-invert           Do not invert the ASCII ramp
          -h, --help            Show this help

        Examples:
          dotnet run --project samples/TextArt
          dotnet run --project samples/TextArt -- photo.jpg
          dotnet run --project samples/TextArt -- photo.jpg -w 80 --ansi-width 40 --ansi-height 20 -o out.html
        """
    );
}

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

sealed class SampleOptions
{
    public string? ImagePath { get; set; }
    public int AsciiWidth { get; set; }
    public int AnsiWidth { get; set; }
    public int AnsiHeight { get; set; }
    public int HtmlWidth { get; set; }
    public int HtmlHeight { get; set; }
    public int DemoWidth { get; set; }
    public int DemoHeight { get; set; }
    public string HtmlOutputPath { get; set; } = "";
    public string AsciiCharacters { get; set; } = " .:-=+*#%@";
    public bool InvertAscii { get; set; } = true;
    public bool ShowHelp { get; set; }

    public static SampleOptions CreateDefaults() =>
        new()
        {
            AsciiWidth = 48,
            AnsiWidth = 24,
            AnsiHeight = 12,
            HtmlWidth = 48,
            HtmlHeight = 24,
            DemoWidth = 64,
            DemoHeight = 64,
            HtmlOutputPath = Path.Combine(AppContext.BaseDirectory, "text-art.html"),
            AsciiCharacters = " .:-=+*#%@",
            InvertAscii = true,
        };
}
