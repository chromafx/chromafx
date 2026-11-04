# Changelog

All notable changes to ChromaFx are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html)
(0.x may still include breaking API changes).

## [Unreleased]

### Added
- `ChromaFx.Native` opt-in stub package — registers WebP/AVIF format probes and typed encoders; Decode/Encode throw until native codecs ship (not in the meta-package).
- `IHistogram.Load(ReadOnlySpan<Color>)` for zero-copy histogram loads.
- `Distance.EuclideanSquared` for compare-without-sqrt paths.
- Benchmarks: Gamma, Posterize, Blend, Median, Equalize, AdaptiveEqualize, GIF decode; AdaptiveThreshold / Crop / BMP RGB24 setup moved out of timed path; BMP RGB24 encode bench.

### Changed
- `ImageFormats.CreateEncoder` / `CreateAnimationEncoder` resolve registered plugins for `FileFormats.WebP` / `Avif` (last registration wins).
- `PixelSimd.ApplyRgbLut` uses a packed `uint` unrolled path (Gamma / Posterize); `ApplyChannelLuts` for per-channel tables.
- AdaptiveThreshold, AdaptiveEqualize, Kuwahara, Blend, Median, Dilate, Constrict, SNNBlur, SinWave, and Turbulence cut snapshot / per-row heap allocations.
- Crop / ResizeCanvas use row `CopyTo` instead of per-pixel dest arrays.
- Equalize, StretchContrast, and Logarithm apply via channel LUTs; `RgbHistogram.LoadImage` is row-major.
- BMP RGB24/32 decode/encode packed shuffles; BMP encode skips intermediate RGBA buffer; `Image(byte[])` fills by row `CopyTo`.
- JPEG YCbCr→RGB and greyscale convert write directly into `GetRowSpan` (IDCT unchanged).
- PNG decode writes scanlines into `GetRowSpan` (no intermediate full-image `Color[]`); 8-bit RGB packs with an unrolled path.
- GIF LZW reuses a 256-byte block buffer; drops a dead RestoreToPrevious `lastFrame` copy; frame palette expands via packed `uint` writes; octree quantize hoists row spans.

### Fixed
- `Color(byte, byte, byte, byte)` no longer allocates a temporary array on every construction.

## [0.6.0]

First tag-only NuGet release. Packages are published from `v*.*.*` tags only (no continuous publish on every `master` commit). The `0.5.0-beta.{CI run}` line ends here; 0.x has no `-beta` suffix (SemVer 0.x already signals pre-1.0).

### Added
- `Scale3x` (EPX) — triples width/height; fluent `Scale3x()`.
- `Hq2x` / `Hq3x` / `Hq4x` — Maxim Stepin HQX family (may blend); fluent helpers (adapted from FFmpeg ISC `vf_hqx`).
- `CleanEdge` — torcado cleanEdge upscaler (MIT); fluent `CleanEdge(scale)`.
- `CleanEdgeRotate` — cleanEdge sample-through rotation (1× AABB; cardinal exact); fluent `CleanEdgeRotate(degrees)`.
- `PixelArt2xHelpers` applies any 2× scaler for N passes (Scale4x / Scale8x and tests).

### Changed
- `RotateSprite` follows Xenowhirl RotSprite: similarity Scale2x → 8×, center-pivot NN rotate+⅛ with offset search, detail restore; output sized to rotated AABB (cardinal 90/180/270 exact remap).
- PixelArt hot paths: `CleanEdge` uses a tight source buffer, quadrant neighborhood reuse, and `Parallel.For`; HQX uses interior 3×3 sampling plus reused YUV for pattern/`Diff`.
- CI packs and publishes NuGet only on version tags.

### Fixed
- SaI-family blends (`Scale2xSaI`, `Super2xSaI`, `SuperEagle`) no longer produce semi-transparent edges (avoided `Color.Average` clamp-on-add).
- `RotateSprite` default empty fill is opaque black instead of transparent, so uncovered areas do not show as white wedges.

## [0.5.0-beta]

NuGet versions are `0.5.0-beta.{CI run}` from the packaging commit onward.

### Added
- `Image` / `Animation` load from `ReadOnlyMemory<byte>`, `byte[]`, and `ReadOnlySpan<byte>` (zero-copy when array-backed; span uses one pooled copy).
- `LoadAsync` / `SaveAsync` (async I/O around sync codecs) for images and animations.
- Terminal rendering under `ChromaFx.Processing.Terminal` (ASCII/ANSI, Kitty, iTerm2).
- Sync `Load(Stream)` buffers non-seekable streams.
- Opt-in **`ChromaFx.PixelArt`** (not in the meta-package): Scale2x / Scale4x / Scale8x, Eagle / SuperEagle, Scale2xSaI / Super2xSaI, and `RotateSprite`.

### Fixed
- GIF NETSCAPE 2.0 loop block write/parse and `Animation.RepeatCount` round-trip.
- Path `Save` resolves format before creating/truncating the output file.
- Empty `Animation` still-image fallback throws instead of indexing frame 0.
- BMP 24-bit encode wrote `biSizeImage` / file size as `PixelCount * 4` (RGBA) instead of padded BGR row bytes (`EndOfStreamException` on round-trip).
- BMP golden compares ignore `bfSize` / `biSizeImage` after the size-field fix.

### Changed
- Package line unified to **0.5.0-beta.\*** (was mixed 0.4.0-beta / 1.0.0-beta props).
- CI sets package version on **build** so assembly and nupkg versions match.
- Apache-2.0 license expression on all NuGet packages; SourceLink + `snupkg` symbols.

## [0.4.0-beta] — prior nuget.org line

Earlier prereleases on nuget.org used the `0.4.0-beta.{build}` scheme. See GitHub commits for detail.
