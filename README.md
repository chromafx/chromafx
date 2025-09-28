# ChromaFX

[![Build Status](https://github.com/chromafx/chromafx/actions/workflows/dotnet.yml/badge.svg)](https://github.com/chromafx/chromafx/actions) 
[![NuGet](https://img.shields.io/nuget/v/ChromaFx.svg)](https://www.nuget.org/packages/ChromaFx/) 
[![NuGet](https://img.shields.io/nuget/v/ChromaFx.Legacy.svg)](https://www.nuget.org/packages/ChromaFx.Legacy/) 
[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](LICENSE)

Modern, high-performance .NET image processing and drawing library.  

**Key points:**
- Pure C#, zero dependency implementation
- Image encoding/decoding (PNG, JPEG, GIF, BMP)
- Drawing primitives and effects
- Pipeline-friendly API for efficient image processing

## Packages

ChromaFX is distributed as multiple NuGet packages to suit different needs:

| Package                | Description                                                                 | NuGet Link                                               |
|------------------------|-----------------------------------------------------------------------------|----------------------------------------------------------|
| `ChromaFx`             | Main package. Modern, high-performance image processing for .NET 8+.        | [ChromaFx](https://www.nuget.org/packages/ChromaFx/)     |
| `ChromaFx.Core`        | Core types and abstractions for ChromaFX.                                   | [ChromaFx.Core](https://www.nuget.org/packages/ChromaFx.Core/) |
| `ChromaFx.IO`          | Image file IO, encoding/decoding support.                                   | [ChromaFx.IO](https://www.nuget.org/packages/ChromaFx.IO/) |
| `ChromaFx.Processing`  | Image processing algorithms and effects.                                    | [ChromaFx.Processing](https://www.nuget.org/packages/ChromaFx.Processing/) |
| `ChromaFx.Legacy`      | Compatibility package for legacy .NET projects and APIs.                    | [ChromaFx.Legacy](https://www.nuget.org/packages/ChromaFx.Legacy/) |

- Use `ChromaFx` for new projects targeting .NET 8 or later.
- Use `ChromaFx.Core`, `ChromaFx.IO`, and `ChromaFx.Processing` for modular usage or advanced scenarios.
- Use `ChromaFx.Legacy` if you need support for older APIs or migration from previous versions.

## Installation
Install via NuGet package:

```bash
dotnet add package ChromaFx
```

For modular usage:

```bash
dotnet add package ChromaFx.Core
dotnet add package ChromaFx.IO
dotnet add package ChromaFx.Processing
```

For legacy support:

```bash
dotnet add package ChromaFx.Legacy
```

## Requirements
- .NET 8 or later

## License
Apache 2.0 - see [LICENSE](LICENSE)