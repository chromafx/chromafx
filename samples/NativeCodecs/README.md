# Native codecs sample

**WebP / AVIF / JPEG XL** still + animated round-trip smoke via opt-in `ChromaFx.Native` (NuGet **0.6.7+**).

```bash
# Build RID binaries once (same as NativeCodecTests)
./native/build.sh

dotnet run --project samples/NativeCodecs
dotnet run --project samples/NativeCodecs -- photo.png -o ./out
dotnet run --project samples/NativeCodecs -- --help
```

Writes a source PNG, lossless still encodes + a short animation per format, then prints sizes / frame counts. Exits non-zero if every codec fails (usually missing `native/build.sh` output).
