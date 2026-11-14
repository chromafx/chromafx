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
using ChromaFx.IO.Encoders;
using ChromaFx.Native;
using ChromaFx.Native.Encoders;

// Touch a key so NativeFormatsBootstrap ModuleInitializer registers codecs.
_ = NativeFormats.WebP;

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
    source = LoadOrCreate(options);
}
catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException or IOException)
{
    Console.Error.WriteLine(ex.Message);
    return 1;
}

Directory.CreateDirectory(options.OutputDir);
var pngPath = Path.Combine(options.OutputDir, "00-source.png");
source.Save(pngPath);

Console.WriteLine(
    $"Source: {(options.ImagePath is null ? $"demo {source.Width}×{source.Height}" : options.ImagePath)}"
);
Console.WriteLine($"Output: {options.OutputDir}");
Console.WriteLine($"  {Path.GetFileName(pngPath)} ({source.Width}×{source.Height})");
Console.WriteLine();
Console.WriteLine("Still WebP / AVIF / JPEG XL round-trips (lossless). Requires native/build.sh.");
Console.WriteLine();

var codecs = new (string Label, string Ext, IImageEncoder Encoder)[]
{
    ("WebP", "webp", new WebPEncoder { Lossless = true }),
    ("AVIF", "avif", new AvifEncoder { Lossless = true }),
    ("JPEG XL", "jxl", new JxlEncoder { Lossless = true }),
};

var failures = 0;
foreach (var (label, ext, encoder) in codecs)
{
    var encodedPath = Path.Combine(options.OutputDir, $"01-{ext}.{ext}");
    var decodedPath = Path.Combine(options.OutputDir, $"02-{ext}-decoded.png");
    try
    {
        source.Save(encodedPath, encoder);
        var encodedBytes = new FileInfo(encodedPath).Length;
        var decoded = Image.Load(encodedPath);
        decoded.Save(decodedPath);
        var delta = MaxChannelDelta(source, decoded);
        Console.WriteLine(
            $"  {label,-8} → {Path.GetFileName(encodedPath)} ({encodedBytes:N0} bytes) → decode maxΔ={delta}"
        );
    }
    catch (Exception ex)
    {
        failures++;
        Console.Error.WriteLine($"  {label,-8} failed: {ex.Message}");
        Console.Error.WriteLine(
            "           Build RID binaries first: native/build.sh (see ChromaFx.Native / AGENTS.md)."
        );
    }
}

if (failures == codecs.Length)
{
    Console.Error.WriteLine();
    Console.Error.WriteLine("All native codecs failed — nothing to round-trip.");
    return 2;
}

Console.WriteLine();
Console.WriteLine(failures == 0 ? "Done." : $"Done with {failures} failure(s).");
return failures == 0 ? 0 : 2;

static Image LoadOrCreate(SampleOptions options)
{
    if (options.ImagePath is null)
        return CreateDemo(options.DemoWidth, options.DemoHeight);

    if (!File.Exists(options.ImagePath))
        throw new FileNotFoundException($"Image not found: {options.ImagePath}", options.ImagePath);

    return Image.Load(options.ImagePath);
}

static Image CreateDemo(int width, int height)
{
    var image = new Image(width, height);
    for (var y = 0; y < height; y++)
    {
        var row = image.GetRowSpan(y);
        for (var x = 0; x < width; x++)
        {
            row[x] = new Color(
                (byte)(40 + x * 6),
                (byte)(80 + y * 4),
                (byte)(160 - x * 3),
                (byte)(x < width / 4 ? 180 : 255)
            );
        }
    }

    return image;
}

static int MaxChannelDelta(Image a, Image b)
{
    if (a.Width != b.Width || a.Height != b.Height)
        return int.MaxValue;

    var max = 0;
    for (var y = 0; y < a.Height; y++)
    {
        var ra = a.GetRowSpan(y);
        var rb = b.GetRowSpan(y);
        for (var x = 0; x < a.Width; x++)
        {
            max = Math.Max(max, Math.Abs(ra[x].Red - rb[x].Red));
            max = Math.Max(max, Math.Abs(ra[x].Green - rb[x].Green));
            max = Math.Max(max, Math.Abs(ra[x].Blue - rb[x].Blue));
            max = Math.Max(max, Math.Abs(ra[x].Alpha - rb[x].Alpha));
        }
    }

    return max;
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
                    if (!int.TryParse(value, out var size) || size < 4 || size > 512)
                    {
                        error = $"Invalid --demo-size: {value} (use 4–512).";
                        return false;
                    }
                    options.DemoWidth = size;
                    options.DemoHeight = size;
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
        NativeCodecs — still WebP / AVIF / JPEG XL round-trip smoke demo

        Usage:
          dotnet run --project samples/NativeCodecs -- [image] [options]

        Requires RID-native libraries from native/build.sh (same as NativeCodecTests).

        Arguments:
          image                 Optional source image. Omit for a generated demo.

        Options:
          -o, --out <dir>       Output directory (default ./native-out under the app dir)
          --demo-size <n>       Generated demo side length when no image (default 32)
          -h, --help            Show this help

        Examples:
          ./native/build.sh
          dotnet run --project samples/NativeCodecs
          dotnet run --project samples/NativeCodecs -- photo.png -o ./out
        """
    );
}

sealed class SampleOptions
{
    public string? ImagePath { get; set; }
    public string OutputDir { get; set; } = "";
    public int DemoWidth { get; set; } = 32;
    public int DemoHeight { get; set; } = 32;
    public bool ShowHelp { get; set; }

    public static SampleOptions CreateDefaults() =>
        new()
        {
            OutputDir = Path.Combine(AppContext.BaseDirectory, "native-out"),
            DemoWidth = 32,
            DemoHeight = 32,
        };
}
