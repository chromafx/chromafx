![ChromaFX — High-performance .NET image processing library](assets/banner.png)

[![Build Status](https://github.com/chromafx/chromafx/actions/workflows/dotnet.yml/badge.svg)](https://github.com/chromafx/chromafx/actions) 
[![NuGet](https://img.shields.io/nuget/v/ChromaFx.svg)](https://www.nuget.org/packages/ChromaFx/) 
[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](LICENSE)

Modern, high-performance .NET image processing and drawing library.  

**Key points:**
- Pure C#, zero dependency core (BMP, PNG, JPEG, GIF)
- Explicit `Mutate` / `Clone` processing API
- Typed encoders (`JpegEncoder`, `PngEncoder`, …) and a public `ImageFormats` registry for extensions
- Pipeline-friendly fluent context for common filters

## Packages

| Package                | Description                                                                 | NuGet Link                                               |
|------------------------|-----------------------------------------------------------------------------|----------------------------------------------------------|
| `ChromaFx`             | Main package. Modern, high-performance image processing for .NET 10.        | [ChromaFx](https://www.nuget.org/packages/ChromaFx/)     |
| `ChromaFx.Core`        | Core types and abstractions for ChromaFX.                                   | [ChromaFx.Core](https://www.nuget.org/packages/ChromaFx.Core/) |
| `ChromaFx.IO`          | Image file IO, encoding/decoding support.                                   | [ChromaFx.IO](https://www.nuget.org/packages/ChromaFx.IO/) |
| `ChromaFx.Processing`  | Image processing algorithms and effects.                                    | [ChromaFx.Processing](https://www.nuget.org/packages/ChromaFx.Processing/) |

- Use `ChromaFx` for new projects targeting .NET 10.
- Use `ChromaFx.Core`, `ChromaFx.IO`, and `ChromaFx.Processing` for modular usage.

## Installation

```bash
dotnet add package ChromaFx
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

// ImageFormats.Register(new WebPFormat());
```

### API notes

- Prefer the `ChromaFx` meta-package (`ChromaFx.Core` + `.IO` + `.Processing`).
- `Image.Load` / `Save` / `ImageFormats` live in **IO**; Mutate/Clone in **Processing**.
- Typed encoders: `ChromaFx.IO.Encoders` (`JpegEncoder`, `PngEncoder`, …).
- Plugins: `IImageFormat` / `FormatBase` in `ChromaFx.IO.Formats` (wire types are internal).
- Path `Save` uses the registered format matching the extension.
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
| `LoadAnimation(path)` | `Animation.Load(path)` |
| `FormatManager` / `Manager` | `ImageFormats.Register` |
| `IFormat` | `IImageFormat` |
| Public format wire types | internal (Tests keep access via `InternalsVisibleTo`) |

## Requirements
- .NET 10 SDK

## License
Apache 2.0 - see [LICENSE](LICENSE)

## Attribution
This project is a fork of [Structure.Sketching](https://github.com/JaCraig/Structure.Sketching),
originally developed by JaCraig under the Apache License 2.0.

Modifications and ongoing maintenance by Ho Tzin Mein (2023–2026).

## Roadmap
- ✅ **Modularization** – Core, IO, Processing packages split out.
- ✅ **API modernization** – `Mutate`/`Clone`, typed encoders, `ImageFormats` registry.
- ⏭ **Extended formats** – optional native extension package (WebP/AVIF/…).
- ⏭ **Extended features** – pixel upscaling, more drawing/effects.

## Contributing
Contributions, bug reports, and suggestions are welcome!
