# Changelog

All notable changes to ChromaFx are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html)
(0.x may still include breaking API changes).

## [Unreleased]

### Added
- Managed ICO. Decode uses the largest image in the file (PNG, or uncompressed 1/4/8/24/32-bit BMP plus the AND mask). `IcoEncoder` writes one PNG image, at most 256×256. `FileFormats.Ico` is `ico`.

### Changed
- JPEG, PNG, TIFF, and WebP decode bake EXIF/TIFF orientation (tag 274, values 1–8) into the pixels. TIFF no longer rejects orientations other than top-left. The saved file does not keep the orientation tag, so a later load does not rotate twice.
- Bit-identical performance pass: AdaptiveThreshold, Kuwahara, Pointillism, Resize, and NonMaximalSuppression reuse `FilterBufferPool`; PNG encode/decode scanlines and TGA decode reuse `ArrayPool<byte>`; PerlinNoise drops a redundant full-image `Color[]`.
- JPEG baseline decode reuses one coefficient block and dequantizes/stores through spans (no per-block allocation); the IDCT indexes coefficients without bounds checks; the entropy bit reader refills up to 64 bits from buffered bytes and stops at markers instead of reading one byte at a time and unreading; the IDCT column pass and YCbCr→RGB conversion use `Vector128` SIMD with the same integer arithmetic (bit-identical output).
- Filters: `Median` and `AdaptiveEqualize` slide per-channel histograms instead of sorting/rebuilding per pixel; `Kuwahara` and `AdaptiveThreshold` use column sums; `Dilate`/`Constrict` are separable; `PerlinNoise`/`Turbulence` generate rows in parallel; TGA encodes whole rows and decodes uncompressed 24/32-bit directly (bit-identical output).

### Fixed
- JPEG: progressive files with restart intervals decode (non-interleaved scans counted restart intervals per interleaved MCU instead of per block and failed with `MissingFf00`).
- Pixellate averages the neighborhood pixels (was always sampling the block origin) and no longer divides by zero or hangs for `PixelSize <= 1`.
- NonMaximalSuppression visits the top-right diagonal during hysteresis (`Right` bound was mistyped as `Left`); strong edges are marked before a serial flood (no parallel races); isolated strong pixels are kept.
- Jitter gathers from a source snapshot (was a parallel in-place scramble with races).
- Dilate, Constrict, and Kuwahara preserve alpha instead of forcing opaque.
- Zero/negative aperture or point size is a no-op for Dilate, Constrict, Kuwahara, SnnBlur, Median, AdaptiveThreshold, AdaptiveEqualize, and Pointillism (avoids divide-by-zero / hangs).
- Pointillism empty sample windows no longer divide by zero.
- Resize and affine transforms index precomputed filter weights from `Weights.Left` (clipped windows no longer misalign coefficients).
- Kuwahara uses classic overlapping `(radius+1)²` quadrants (center in all four) and picks by luminance variance instead of channel range.
- `NearestNeighborFilter.GetValue(0)` is `1` (was `|value|`, so the center weight was always zero).
- Affine transforms fill unmapped samples with transparent black `(0,0,0,0)` instead of opaque black.
- Drawing `Plot` alpha-composites onto the destination (Xiaolin Wu coverage no longer overwrites).
- BoxBlur vertical pass keeps alpha, matching the horizontal pass.
- BMP: palette images (1/4/8-bit, RLE8) are opaque (the palette's reserved byte was read as alpha); 1-bit decode uses the palette and the correct 4-byte row stride (rows were truncated); 4-bit keeps the last column on odd widths; 32-bit BI_RGB reads BGRX and BITFIELDS honours the masks; 16-bit BI_RGB defaults to 5-5-5; RLE8 handles end-of-line, delta and absolute runs, and RLE4 is supported; top-down (negative height) files decode upright; BITFIELDS masks after a 40-byte header and gaps before the pixel data are skipped.
- PNG: 1/2/4-bit greyscale scales samples to 0–255 (was 0–1/3/15); 16-bit greyscale and greyscale+alpha read the high byte of each sample.
- JPEG: 4:2:0 and 4:2:2 chroma is upsampled with the standard triangle filter instead of pixel replication, and YCbCr→RGB uses 16-bit fixed point. Small or sharp-edged images no longer show blocky colour (the 9×3 `rgb.jpg` was off by up to 94 levels; decoded pixels are now within 3 levels of libjpeg).
- TIFF: LZW switches code width one code early, as the TIFF spec requires ("early change"); LZW files from libtiff/Pillow no longer fail as corrupt.

## [0.6.2]

Managed TGA and TIFF, plus still WebP, AVIF, and JPEG XL in `ChromaFx.Native`. The meta-package stays free of native binaries.

### Added
- `ChromaFx.Native` still-image WebP and AVIF decode/encode (bundled `libwebp` 1.6.0 and `libavif` 1.3.0 with dav1d + libaom). `WebPEncoder` / `AvifEncoder` take `Quality` and `Lossless`. Animated WebP/AVIF are rejected. The meta-package stays free of native binaries.
- Managed TGA (types 1/2/3/9/10/11, including RLE) and baseline TIFF (first IFD, strips or tiles, uncompressed / PackBits / LZW / Deflate, 1/4/8-bit bilevel, greyscale, palette, or RGB, optional alpha). Encoders write uncompressed 32-bit TGA and uncompressed 8-bit RGBA TIFF.
- `ChromaFx.Native` still JPEG XL via bundled `libjxl` 0.12.0. `JxlEncoder` takes `Quality` (lossy distance `(100 - quality) / 10`; quality 100 is lossless) and `Lossless`. Animated JPEG XL is rejected.

### Changed
- `FileFormats` is no longer an enum. It holds `ImageFormatKey` values for the managed codecs (`bmp`, `png`, `jpeg`, `gif`, `tga`, `tiff`). `Save`, `ToByteArray`, and `ToBase64String` take that key and use the last registered format's `CreateEncoder`.
- WebP, AVIF, and JPEG XL ids live on `NativeFormats` in `ChromaFx.Native` (`webp`, `avif`, `jxl`). A key nothing has registered throws `UnknownImageFormatException`.

## [0.6.1]

Bit-identical performance pass plus opt-in `ChromaFx.Native` stubs. Public API is additive only (`IHistogram.Load(ReadOnlySpan<Color>)`, `Distance.EuclideanSquared`); filter/codec internals cut allocations without changing goldens.

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
