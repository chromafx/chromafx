# Agent guide — ChromaFx

Pure C# image processing library (PNG, JPEG, GIF, BMP). Targets **net10.0**.

## Layout

| Path | Role |
|------|------|
| `src/ChromaFx.Core` | Image, Color, streams, core types |
| `src/ChromaFx.IO` | Format encode/decode |
| `src/ChromaFx.Processing` | Filters, pipelines, numerics |
| `src/ChromaFx` | Facade package (references the three above) |
| `tests/ChromaFx.Tests` | Unit + golden-file regression |
| `tests/ChromaFx.Benchmarks` | BenchmarkDotNet |

Shared TFMs/version: `Directory.Build.props`. SDK pin: `global.json`.

## Commands

```bash
dotnet build -c Release
dotnet test -c Release --verbosity minimal

# Filter one golden test (Microsoft Testing Platform / xunit.v3)
dotnet test -c Release --filter-class ResamplingFilters

# Benchmarks (run from repo root; use output-relative TestImage paths)
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

## Where to look

- Cursor rules: `.cursor/rules/`
- Filter harness: `tests/ChromaFx.Tests/BaseClasses/FilterTestBaseClass.cs`
- Golden compare / tolerance: `tests/ChromaFx.Tests/BaseClasses/TestBaseClass.cs`
- Parallelism: `tests/ChromaFx.Tests/BaseClasses/FilterCollection.cs`
