# ChromaFX.Legacy

> ⚠️ **Legacy branch of ChromaFX.**
> This branch preserves compatibility with the original **Structure.Sketching** API.
> It is **not actively developed**. For new projects, please use the [`main`](https://github.com/chromafx/chromafx) branch instead.

---

## About
**ChromaFX.Legacy** is a hard fork of the abandoned [Structure.Sketching] library.
This branch exists to provide:

- A **drop-in replacement** for existing codebases that relied on Structure.Sketching.
- Access to bugfixes and minimal maintenance where absolutely necessary.
- A transition path to the new **ChromaFX** API.

---

## Installation

From NuGet:

```powershell
dotnet add package ChromaFx.Legacy
```

---

## Status

- ✅ API compatible with Structure.Sketching (v0.2.x)
- ❌ No new features planned
- 🚀 Migration encouraged to [ChromaFX (main)](https://github.com/chromafx/chromafx)

---

## Migration

If you’re starting fresh, **don’t use this package**. Use the new [ChromaFX](https://github.com/chromafx/chromafx) instead.

If you have existing code that uses Structure.Sketching:
1. Replace your NuGet reference with `ChromaFx.Legacy`.
2. Your code should compile without changes.
3. When ready, plan a gradual migration to the new ChromaFX API.

---

## License
Apache 2.0 (inherited from Structure.Sketching).
