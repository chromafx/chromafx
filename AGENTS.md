# Agent guide — ChromaFx

Pure C# image processing library (PNG, JPEG, GIF, BMP, TGA, TIFF, ICO). Targets **net10.0**.

## Layout

| Path | Role |
|------|------|
| `src/ChromaFx.Core` | Image, Color, Animation, exceptions; extensions in `ChromaFx.Core`; color spaces in `ChromaFx.Core.Colors`; shared `Quantization/` (`Octree`, `OrderedDither`, `OrderedDitherMethod`) |
| `src/ChromaFx.IO` | Formats, encoders, Load/Save; `ManagedFormats` ids + `ImageFormats` registry (GIF encode uses Core octree) |
| `src/ChromaFx.Processing` | Filters, Mutate/Clone, numerics; `IFilter` / `Quantize` / `DitherMethod` in `ChromaFx.Processing.Filters`; resampling enum `ResamplingFilter` + `ResamplingKernels.Create` in `…Filters.Resampling` |
| `src/ChromaFx.PixelArt` | Opt-in Scale2x / Scale3x / Hq2x–4x / CleanEdge / Eagle / SaI and RotateSprite / CleanEdgeRotate (not in meta-package) |
| `src/ChromaFx.Retro` | Opt-in retro palettes (CGA / EGA approx / VGA DAC / web-safe) with `OrderedDitherMethod`, plus `BadWebScale`, `Bevel`, `RetroMode`; more Millennium filters planned (not in meta-package) |
| `src/ChromaFx.Native` | Opt-in still WebP/AVIF/JPEG XL (not in meta-package; build RID libs with `native/build.sh`) |
| `src/ChromaFx` | Meta-package only (refs + `buildTransitive` usings) |
| `tests/ChromaFx.Tests` | Unit + golden-file regression |
| `tests/ChromaFx.Benchmarks` | BenchmarkDotNet |
| `samples/TextArt` | ASCII / ANSI / HTML / Kitty / iTerm2 smoke |
| `samples/PixelArt` | PixelArt upscalers → PNG out |
| `samples/NativeCodecs` | Native still WebP/AVIF/JXL round-trip (needs `native/build.sh`) |

This repository is the library only. Sibling folders in the local workspace (`chromafx-site`, `github-org`) are documented in the workspace root `AGENTS.md`.

Shared TFMs/version: `Directory.Build.props` (`GeneratePackageOnBuild` is off — use `dotnet pack` when needed). SDK pin: `global.json`. After a `vX.Y.Z` tag ships, bump locals in the same file to `(X.Y.Z+1)-local` and the non-tag CI line in `.github/workflows/dotnet.yml` to `(X.Y.Z+1)-ci.$GITHUB_RUN_NUMBER`. Leave `PackageReleaseNotes` describing the last shipped tag until the next release.

## Commands

```bash
dotnet build -c Release
dotnet test -c Release --verbosity minimal

# If only a newer runtime is installed (e.g. .NET 11 while TFM is net10.0):
# DOTNET_ROLL_FORWARD=LatestMajor dotnet test -c Release --verbosity minimal

# NativeCodecTests fail unless native/build.sh has been run; skip them when not touching ChromaFx.Native
dotnet test -c Release --verbosity minimal --filter-not-class ChromaFx.Tests.NativeCodecTests

# Filter one golden test (Microsoft Testing Platform / xunit.v3)
dotnet test -c Release --filter-class ResamplingFilters

# Benchmarks (BenchmarkDotNet; cross-platform. System.Drawing baselines are Windows-only.)
dotnet run -c Release --project tests/ChromaFx.Benchmarks -- --filter '*'
dotnet run -c Release --project tests/ChromaFx.Benchmarks -- --list flat
```

Tests use **Microsoft Testing Platform** (`global.json` `test.runner` + xunit.v3). CI (`.github/workflows/dotnet.yml`): Ubuntu, .NET 10 SDK, `dotnet test`. Native codecs are cached by a hash of `native/`; CI compiles them on a cache miss, a version tag, or a manual run, and other runs restore those binaries. A manual run on `master` or `legacy` replaces that cache. Benchmarks are **not** in CI.

## Conventions agents must follow

1. **Minimal diffs** — match existing style; no drive-by refactors.
2. **Commit only when asked** — logical groups; never push unless asked.
3. **Do not commit** `tests/**/TestOutput/`, `BenchmarkDotNet.Artifacts/`.
4. **Streams** — prefer `ReadExactly` over `Stream.Read` for fixed-size reads (CA2022 on net10). Partial reads in loops are OK when intentional.
5. **Pixels** — prefer `image.GetRowSpan(y)[x]` for filter/IO pixel access. Do not treat `PixelSpan` as tightly packed (`Stride` may exceed `Width`). Tight `Color[]` temps sized to `PixelCount` still use `y * Width + x`. For encoders, copy logical rows (`CopyRgbaBytesTo` / `MemoryMarshal.AsBytes(GetRowSpan(y))`), not full `AsBytes()` storage.
6. **Filter goldens** — serial via `[Collection("FilterCollection")]`. Compare BMP bytes; Rotate/Skew allow up to 500 differing pixels (`TestBaseClass`).
7. **Regenerate goldens carefully** — copy failing output from `TestOutput/` into `ExpectedResults/` only after confirming the code change is correct. CI runs on **Ubuntu**; resampling/affine FP can still differ across OS/CPU. Default `AdaptiveEqualize` uses integer CDF (cross-platform). Do **not** refresh goldens for bit-identical refactors — leave the Ubuntu-proven BMPs. Prefer regenerating intentional visual changes on Linux, or fix from CI `TestOutput` artifacts rather than trusting a macOS-only refresh for floaty filters.
8. **Quantization / dither** — Core owns the shared octree and Bayer tables. Processing `Quantize` ordered dither uses `OrderedDither.QuantizeBiasScale` (~±32 at strength 1); Retro uses `OrderedDither.RetroBiasScale` with rounding (~±127.5). Do not unify those scales without intentional golden regen. Keep layering: IO and Processing both depend only on Core (no IO↔Processing).
9. **PNG roundtrips** — compare decoded pixels (`CheckDecodedPngCorrect`), not encoded bytes. Prefer the same for JPEG/GIF if changing those tests.
10. **Copyright headers (Apache 2.0)** —
   - **JaCraig / Structure.Sketching originals** (including files that lacked a notice): keep `Copyright 2017–2020 JaCraig` and use `Modifications Copyright 2023–2026 Ho Tzin Mein`. Never strip JaCraig.
   - **New files** authored for ChromaFx: `Copyright YYYY Ho Tzin Mein` only (use a year range if the file spans years).
   - Keep the full Apache license block; do not rewrite years on untouched files.

## Where to look

- Path-scoped agent rules (local only, gitignored): `.cursor/rules/` (also `.devin/rules/`)
- Filter harness: `tests/ChromaFx.Tests/BaseClasses/FilterTestBaseClass.cs`
- Golden compare / tolerance: `tests/ChromaFx.Tests/BaseClasses/TestBaseClass.cs`
- Parallelism: `tests/ChromaFx.Tests/BaseClasses/FilterCollection.cs`
