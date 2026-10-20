# Agent guide — ChromaFx

Pure C# image processing library (PNG, JPEG, GIF, BMP). Targets **net10.0**.

## Layout

| Path | Role |
|------|------|
| `src/ChromaFx.Core` | Image, Color, Animation, exceptions |
| `src/ChromaFx.IO` | Formats, encoders, Load/Save, `ImageFormats` registry |
| `src/ChromaFx.Processing` | Filters, Mutate/Clone, numerics |
| `src/ChromaFx` | Meta-package only (refs + `buildTransitive` usings) |
| `tests/ChromaFx.Tests` | Unit + golden-file regression |
| `tests/ChromaFx.Benchmarks` | BenchmarkDotNet |

Shared TFMs/version: `Directory.Build.props` (`GeneratePackageOnBuild` is off — use `dotnet pack` when needed). SDK pin: `global.json`.

## Commands

```bash
dotnet build -c Release
dotnet test -c Release --verbosity minimal

# Filter one golden test (Microsoft Testing Platform / xunit.v3)
dotnet test -c Release --filter-class ResamplingFilters

# Benchmarks (BenchmarkDotNet; cross-platform. System.Drawing baselines are Windows-only.)
dotnet run -c Release --project tests/ChromaFx.Benchmarks -- --filter '*'
dotnet run -c Release --project tests/ChromaFx.Benchmarks -- --list flat
```

Tests use **Microsoft Testing Platform** (`global.json` `test.runner` + xunit.v3). CI (`.github/workflows/dotnet.yml`): Ubuntu, .NET 10 SDK, `dotnet test`. Benchmarks are **not** in CI.

## Conventions agents must follow

1. **Minimal diffs** — match existing style; no drive-by refactors.
2. **Commit only when asked** — logical groups; never push unless asked.
3. **Do not commit** `tests/**/TestOutput/`, `BenchmarkDotNet.Artifacts/`.
4. **Streams** — prefer `ReadExactly` over `Stream.Read` for fixed-size reads (CA2022 on net10). Partial reads in loops are OK when intentional.
5. **Filter goldens** — serial via `[Collection("FilterCollection")]`. Compare BMP bytes; Rotate/Skew allow up to 500 differing pixels (`TestBaseClass`).
6. **Regenerate goldens carefully** — copy failing output from `TestOutput/` into `ExpectedResults/` only after confirming the code change is correct; resampling/affine FP can differ across OS/CPU.
7. **PNG roundtrips** — compare decoded pixels (`CheckDecodedPngCorrect`), not encoded bytes. Prefer the same for JPEG/GIF if changing those tests.
8. **Copyright headers (Apache 2.0)** —
   - **JaCraig / Structure.Sketching originals** (including files that lacked a notice): keep `Copyright 2017–2020 JaCraig` and use `Modifications Copyright 2023–2026 Ho Tzin Mein`. Never strip JaCraig.
   - **New files** authored for ChromaFx: `Copyright YYYY Ho Tzin Mein` only (use a year range if the file spans years).
   - Keep the full Apache license block; do not rewrite years on untouched files.

## Where to look

- Cursor rules: `.cursor/rules/`
- Filter harness: `tests/ChromaFx.Tests/BaseClasses/FilterTestBaseClass.cs`
- Golden compare / tolerance: `tests/ChromaFx.Tests/BaseClasses/TestBaseClass.cs`
- Parallelism: `tests/ChromaFx.Tests/BaseClasses/FilterCollection.cs`
