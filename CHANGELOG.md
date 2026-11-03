# Changelog

All notable changes to ChromaFx are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html)
(0.x may still include breaking API changes).

## [Unreleased]

## [0.5.0-beta]

NuGet versions are `0.5.0-beta.{CI run}` from the packaging commit onward.

### Added
- `Image` / `Animation` load from `ReadOnlyMemory<byte>`, `byte[]`, and `ReadOnlySpan<byte>` (zero-copy when array-backed; span uses one pooled copy).
- `LoadAsync` / `SaveAsync` (async I/O around sync codecs) for images and animations.
- Terminal rendering under `ChromaFx.Processing.Terminal` (ASCII/ANSI, Kitty, iTerm2).
- Sync `Load(Stream)` buffers non-seekable streams.

### Fixed
- GIF NETSCAPE 2.0 loop block write/parse and `Animation.RepeatCount` round-trip.
- Path `Save` resolves format before creating/truncating the output file.
- Empty `Animation` still-image fallback throws instead of indexing frame 0.

### Changed
- Package line unified to **0.5.0-beta.\*** (was mixed 0.4.0-beta / 1.0.0-beta props).
- CI sets package version on **build** so assembly and nupkg versions match.
- Apache-2.0 license expression on all NuGet packages; SourceLink + `snupkg` symbols.

## [0.4.0-beta] — prior nuget.org line

Earlier prereleases on nuget.org used the `0.4.0-beta.{build}` scheme. See GitHub commits for detail.
