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

using ChromaFx.Core;
using ChromaFx.Core.Colors;
using ChromaFx.IO;
using ChromaFx.PixelArt;
using ChromaFx.Processing;

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

Image source;
try
{
    source = LoadOrCreateSprite(options);
}
catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException or IOException)
{
    Console.Error.WriteLine(ex.Message);
    return 1;
}

Directory.CreateDirectory(options.OutputDir);
var sourcePath = Path.Combine(options.OutputDir, "00-source.png");
source.Save(sourcePath);
Console.WriteLine(
    $"Source: {(options.ImagePath is null ? $"demo sprite {source.Width}×{source.Height}" : options.ImagePath)}"
);
Console.WriteLine($"Output: {options.OutputDir}");
Console.WriteLine($"  {Path.GetFileName(sourcePath)} ({source.Width}×{source.Height})");

foreach (var (name, apply) in Upscalers())
{
    var result = source.Clone(apply);
    var path = Path.Combine(options.OutputDir, $"{name}.png");
    result.Save(path);
    Console.WriteLine($"  {Path.GetFileName(path)} ({result.Width}×{result.Height})");
}

Console.WriteLine("Done.");
return 0;

static IEnumerable<(string Name, Action<IImageProcessingContext> Apply)> Upscalers() =>
    [
        ("01-scale2x", c => c.Scale2x()),
        ("02-scale3x", c => c.Scale3x()),
        ("03-hq2x", c => c.Hq2x()),
        ("04-hq3x", c => c.Hq3x()),
        ("05-eagle", c => c.Eagle()),
        ("06-super-eagle", c => c.SuperEagle()),
        ("07-scale2xsai", c => c.Scale2xSaI()),
        ("08-super2xsai", c => c.Super2xSaI()),
        ("09-rotate45-clean", c => c.CleanEdgeRotate(45f)),
    ];

static Image LoadOrCreateSprite(SampleOptions options)
{
    if (options.ImagePath is null)
        return CreateDemoSprite(options.DemoSize);

    if (!File.Exists(options.ImagePath))
        throw new FileNotFoundException($"Image not found: {options.ImagePath}", options.ImagePath);

    return Image.Load(options.ImagePath);
}

static Image CreateDemoSprite(int size)
{
    // Compact checker / diamond sprite so EPX / HQX differences are obvious.
    var image = new Image(size, size);
    var mid = (size - 1) * 0.5;
    for (var y = 0; y < size; y++)
    {
        var row = image.GetRowSpan(y);
        for (var x = 0; x < size; x++)
        {
            var onDiamond = Math.Abs(x - mid) + Math.Abs(y - mid) < size * 0.35;
            var checker = ((x / 2) + (y / 2)) % 2 == 0;
            if (onDiamond)
                row[x] = new Color(240, 80, 40);
            else if (checker)
                row[x] = new Color(40, 120, 220);
            else
                row[x] = new Color(20, 24, 32);
        }
    }

    return image;
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
            if (i + 1 >= args.Length)
            {
                error = $"Missing value for {arg}.";
                return false;
            }

            var value = args[++i];
            switch (arg)
            {
                case "-o" or "--out":
                    options.OutputDir = Path.GetFullPath(value);
                    break;
                case "--demo-size":
                    if (!int.TryParse(value, out var size) || size < 8 || size > 256)
                    {
                        error = $"Invalid --demo-size: {value} (use 8–256).";
                        return false;
                    }
                    options.DemoSize = size;
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

    return true;
}

static void PrintUsage()
{
    Console.WriteLine(
        """
        PixelArt — Scale2x / HQX / Eagle / SaI / CleanEdgeRotate smoke demo

        Usage:
          dotnet run --project samples/PixelArt -- [image] [options]

        Arguments:
          image                 Optional source image (PNG/JPEG/…). Omit for a generated sprite.

        Options:
          -o, --out <dir>       Output directory (default ./pixelart-out under the app dir)
          --demo-size <n>       Generated sprite side length when no image (default 16)
          -h, --help            Show this help

        Examples:
          dotnet run --project samples/PixelArt
          dotnet run --project samples/PixelArt -- sprite.png -o ./out
        """
    );
}

sealed class SampleOptions
{
    public string? ImagePath { get; set; }
    public string OutputDir { get; set; } = "";
    public int DemoSize { get; set; } = 16;
    public bool ShowHelp { get; set; }

    public static SampleOptions CreateDefaults() =>
        new()
        {
            OutputDir = Path.Combine(AppContext.BaseDirectory, "pixelart-out"),
            DemoSize = 16,
        };
}
