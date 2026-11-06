![ChromaFx — .NET image processing library](assets/banner.png)

[![Build Status](https://github.com/chromafx/chromafx/actions/workflows/dotnet.yml/badge.svg)](https://github.com/chromafx/chromafx/actions) 
[![NuGet](https://img.shields.io/nuget/v/ChromaFx.svg)](https://www.nuget.org/packages/ChromaFx/) 
[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](LICENSE)

Modern .NET image processing library.

A lean, managed alternative for .NET apps that need image processing without native codec DLLs.

**Key points:**
- Pure managed C# on .NET 10 — no third-party NuGet dependencies, no native codec binaries to deploy (BMP, PNG, JPEG, GIF, TGA, TIFF)
- Uses `Span<T>`, `ArrayPool`, and SIMD on key paths
- Explicit `Mutate` / `Clone` processing API
- Typed encoders (`JpegEncoder`, `PngEncoder`, …) and a public `ImageFormats` registry for extensions
- Pipeline-friendly fluent context for common filters
- `Load` / `LoadAsync` from path, stream, `byte[]`, memory, or span; matching `Save` / `SaveAsync`
- Terminal rendering (`ChromaFx.Processing.Terminal`) — ASCII/ANSI art plus Kitty and iTerm2 inline images
- Opt-in [`ChromaFx.PixelArt`](https://www.nuget.org/packages/ChromaFx.PixelArt/) — Scale2x / Scale3x / Hq2x–4x / CleanEdge / Eagle / SaI upscalers and `RotateSprite` / `CleanEdgeRotate` (not in the meta-package)
- Opt-in [`ChromaFx.Native`](https://www.nuget.org/packages/ChromaFx.Native/) — still WebP, AVIF, and JPEG XL (not in the meta-package; animated WebP/AVIF/JPEG XL are not supported)

## Performance & Architecture

Fully managed on .NET 10 — no native codec binaries for Docker/Lambda multi-arch. Native code can still edge out pure managed pixel loops; ChromaFx prioritizes deployability and allocation-conscious processing for typical decode → filter → encode pipelines. Measure with `tests/ChromaFx.Benchmarks` for your scenario.

## Packages

| Package                | Description                                                                 | NuGet Link                                               |
|------------------------|-----------------------------------------------------------------------------|----------------------------------------------------------|
| `ChromaFx`             | Meta-package for .NET 10 (`Core` + `IO` + `Processing`).                    | [ChromaFx](https://www.nuget.org/packages/ChromaFx/) |
| `ChromaFx.Core`        | Core types and abstractions.                                                | [ChromaFx.Core](https://www.nuget.org/packages/ChromaFx.Core/) |
| `ChromaFx.IO`          | Image file IO, encoding/decoding support.                                   | [ChromaFx.IO](https://www.nuget.org/packages/ChromaFx.IO/) |
| `ChromaFx.Processing`  | Image processing algorithms and effects.                                    | [ChromaFx.Processing](https://www.nuget.org/packages/ChromaFx.Processing/) |
| `ChromaFx.PixelArt`    | Opt-in pixel-art upscaling (Scale2x / Scale3x, Hq2x–4x, CleanEdge, Eagle, SaI) and `RotateSprite` / `CleanEdgeRotate`. Not in the meta-package. | [ChromaFx.PixelArt](https://www.nuget.org/packages/ChromaFx.PixelArt/) |
| `ChromaFx.Native`      | Opt-in still WebP, AVIF, and JPEG XL. Not in the meta-package. | [ChromaFx.Native](https://www.nuget.org/packages/ChromaFx.Native/) |

- Use `ChromaFx` for new projects targeting .NET 10.
- Use `ChromaFx.Core`, `ChromaFx.IO`, and `ChromaFx.Processing` for modular usage.
- Add `ChromaFx.PixelArt` for Scale2x / Scale3x / Hq2x–4x / CleanEdge / Eagle / SaI or `RotateSprite` / `CleanEdgeRotate`.
- Add `ChromaFx.Native` for still WebP, AVIF, and JPEG XL. Animated WebP/AVIF/JPEG XL are not supported.

## Installation

Current NuGet line is **0.6.2** (tag-only releases; 0.x may still change before 1.0):

```bash
dotnet add package ChromaFx
# optional:
dotnet add package ChromaFx.PixelArt
dotnet add package ChromaFx.Native   # still WebP / AVIF / JPEG XL
```

## Getting Started

```csharp
// With PackageReference to ChromaFx, common namespaces are injected.
var thumb = Image.Load("input.png")
    .Clone(ctx => ctx
        .Resize(200, 200)
        .Rotate(90)
        .Invert());

thumb.Save("output.jpg", new JpegEncoder { Quality = 85 });

Image.Load("input.png")
    .Mutate(ctx => ctx.Resize(800, 600).Brightness(0.1f))
    .Save("output.png");
```

```csharp
var fromBytes = Image.Load(pngBytes);                 // zero-copy when array-backed
var image = await Image.LoadAsync("input.png");       // async read, sync decode
Console.Write(image.ToAsciiArt(new AsciiArtOptions { MaxWidth = 40, Invert = true }));
```

### API notes

- Prefer the `ChromaFx` meta-package (`ChromaFx.Core` + `.IO` + `.Processing`). Add `ChromaFx.PixelArt` separately for pixel-art filters.
- `Image.Load` / `Save` / `ImageFormats` live in **IO**; Mutate/Clone in **Processing**.
- Fluent context: core ops on `IImageProcessingContext`, more via extensions (`BoxBlur`, `Saturation`, `Dilate`, `Grayscale`, …); escape hatch `Apply(IFilter)`.
- Typed encoders: `ChromaFx.IO.Encoders` (`JpegEncoder`, `PngEncoder`, `BmpEncoder`, `GifEncoder` as `IAnimationEncoder`, `TgaEncoder`, `TiffEncoder`). Still WebP, AVIF, and JPEG XL encoders are in `ChromaFx.Native.Encoders`.
- Format ids are `ImageFormatKey` values (the name, compared ordinal-ignore-case). `FileFormats` names the managed codecs. `NativeFormats` in `ChromaFx.Native` names still WebP, AVIF, and JPEG XL. `Save(stream, key)` uses the last registered format's encoder. A missing key throws.
- `Animation` supports per-frame delays (`GetFrameDelay` / `SetFrameDelay` / `FrameDelays`); `Delay` sets all frames.
- Path `Save` / `SaveAsync` use the last registered format that accepts the extension (plugins can override built-ins). `ImageFormats.TryGetFormat` looks up by id and returns the first match.
- Meta-package injects common usings via `buildTransitive/ChromaFx.props`.
- Pack with `dotnet pack` (not on every local build).

### Migration (breaking)

These older APIs are **removed** (not obsolete):

| Removed | Use instead |
|---------|-------------|
| `img.Apply(...)` / `img.Apply<T>()` | `img.Mutate(c => c.Invert())` / `c.Apply(filter)` |
| `img.Process(...)` / `ImagePipeline` | `img.Mutate` / `Clone` |
| `ImageOperatorExtensions` (`BrightnessUp`, …) | `Clone`/`Mutate` + context ops |
| `img.Copy()` | `img.Clone()` |
| `img.Save(path)` → `bool` | `void` (throws) |
| `IImageFormat.Encode` → `bool` | `void` (throws) |
| `LoadAnimation(path)` | `Animation.Load(path)` |
| `FormatManager` / `Manager` | `ImageFormats.Register` |
| `IFormat` | `IImageFormat` |
| `FileFormats` enum (`WebP`, `Avif`, `JpegXl` included) | `ImageFormatKey`; managed ids stay on `FileFormats`, native ids are `NativeFormats` |
| Public format types as the only API | Prefer `ImageFormats` / typed encoders (`BmpFormat` etc. remain public for advanced use) |

## Requirements
- .NET 10 SDK

## Documentation
Guides: [chromafx.net](https://chromafx.net) ([chromafx/chromafx-site](https://github.com/chromafx/chromafx-site)).

## Changelog
See [CHANGELOG.md](CHANGELOG.md) for notable changes.

## License
Apache 2.0 - see [LICENSE](LICENSE)

## Attribution
This project is a fork of [Structure.Sketching](https://github.com/JaCraig/Structure.Sketching),
originally developed by JaCraig under the Apache License 2.0.

Modifications and ongoing maintenance by Ho Tzin Mein (2023–2026).

## Roadmap
- ✅ **Modularization** – Core, IO, Processing packages split out.
- ✅ **API modernization** – `Mutate`/`Clone`, typed encoders, `ImageFormats` registry.
- ✅ **PixelArt** – opt-in `ChromaFx.PixelArt` (Scale2x / Scale3x / Hq2x–4x / CleanEdge / Eagle / SaI, `RotateSprite` / `CleanEdgeRotate`).
- ✅ **Performance (pass)** – Color ctor alloc fix; faster `ApplyRgbLut`; AdaptiveThreshold / AdaptiveEqualize / Kuwahara / Blend alloc cuts.
- ✅ **Extended formats** – managed TGA and TIFF; opt-in `ChromaFx.Native` still WebP, AVIF, and JPEG XL (no animation).
- ⏭ **Extended features** – more drawing/effects.
- ⏭ **Performance** – more Span/ArrayPool/SIMD on codecs and remaining filters.

## Contributing
Contributions, bug reports, and suggestions are welcome.
