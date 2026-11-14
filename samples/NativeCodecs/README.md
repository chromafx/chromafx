# Native codecs sample

Still **WebP / AVIF / JPEG XL** encode → decode smoke test via opt-in `ChromaFx.Native`.

```bash
# Build RID binaries once (same as NativeCodecTests)
./native/build.sh

dotnet run --project samples/NativeCodecs
dotnet run --project samples/NativeCodecs -- photo.png -o ./out
dotnet run --project samples/NativeCodecs -- --help
```

Writes a source PNG, lossless encodes for each format, then decoded PNGs and prints max channel delta. Exits non-zero if every codec fails (usually missing `native/build.sh` output).
