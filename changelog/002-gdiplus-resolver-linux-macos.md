---
id: 002
date: 2026-05-24
version: 26.5.1
type: fix
---

# 26.5.1 — Image-area redaction now works on Linux & macOS

## What changed
- **Fixed `redact_image_area` on Linux/macOS (including the Docker image).** Added
  `GdiPlusResolver` (registered from `Program.cs` at startup) that maps the
  `gdiplus.dll` P/Invoke — emitted by `System.Drawing.Common`'s GDI+ interop, which
  GroupDocs.Redaction reaches through Aspose.Pdf — to the platform's libgdiplus
  (`libgdiplus.so` / `libgdiplus.dylib`). It's a no-op on Windows (built-in GDI+).
- Engine (`GroupDocs.Redaction`) stays at 26.5.0; only the MCP package version is bumped.

## Why
On non-Windows, the .NET native-library loader probes `gdiplus.dll`,
`libgdiplus.dll`, and the `.so`/`.dylib` variants of those names — but **never** the
installed `libgdiplus.so` / `libgdiplus.dylib`. So `redact_image_area` threw
`System.DllNotFoundException: Unable to load shared library 'gdiplus.dll'` on Linux
and macOS even when libgdiplus was installed (apt `libgdiplus` / brew
`mono-libgdiplus`), and inside the Docker image. The resolver intercepts that load
and redirects it to the real libgdiplus. The other four tools never touched this
path and were unaffected.

This surfaced via the integration suite: `RedactImageAreaTests` failed on the
ubuntu and macOS CI legs (Windows passed — GDI+ is built in).

## Migration / impact
- libgdiplus must still be present on the host: the Docker image installs it; native
  (`dnx` / global tool) users install it per the README
  (`apt-get install libgdiplus` / `brew install mono-libgdiplus`). The resolver only
  fixes the *name mapping*, not a missing library.
- No API or tool-surface change. `GdiPlusResolver` must run before the first GDI+
  P/Invoke — do not remove the `GdiPlusResolver.Register();` call at the top of
  `Program.cs`.
