# Text art sample

Smoke-tests ASCII, ANSI half-blocks, fixed-width HTML art (`ToHtmlArt`), and Kitty/iTerm2 via encode-PNG-then-`FromPng`.

```bash
# Generated demo (defaults)
dotnet run --project samples/TextArt

# Load a file; override sizes
dotnet run --project samples/TextArt -- photo.jpg -w 80 --ansi-width 40 --ansi-height 20 -o out.html

# Help
dotnet run --project samples/TextArt -- --help
```

| Option | Default |
|--------|---------|
| image path | generated 64×64 demo |
| `-w` / `--width` | ASCII (and HTML) width 48 |
| `--height` | HTML height 24 |
| `--ansi-width` / `--ansi-height` | 24×12 |
| `--html-width` / `--html-height` | 48×24 |
| `-o` / `--out` | `text-art.html` next to the binary |
| `--chars` | ` .:-=+*#%@` |
| `--no-invert` | ASCII invert on by default |

Use Kitty, iTerm2/WezTerm, or another truecolor terminal for the best output.
