# Pixel art sample

Smoke-tests opt-in `ChromaFx.PixelArt` upscalers (Scale2x / Scale3x, HQX, Eagle, SaI) and `CleanEdgeRotate`.

```bash
# Generated 16×16 demo sprite → PNGs under the app dir
dotnet run --project samples/PixelArt

# Upscale a file into ./out
dotnet run --project samples/PixelArt -- sprite.png -o ./out

dotnet run --project samples/PixelArt -- --help
```

Writes `00-source.png` plus one PNG per algorithm. Small sources show EPX / HQX differences best.
