---
id: 003
date: 2026-07-15
version: 26.7.0
type: change
---

# 26.7.0 — Engine upgrade to GroupDocs.Redaction 26.6.0 + shared error formatter

## What changed
- **Upgraded the redaction engine** `GroupDocs.Redaction` 26.5.0 → **26.6.0**
  (resolved under `net10.0` via `GroupDocs.Redaction.Net100` 26.6.0).
- **Bumped the MCP server package** 26.5.1 → **26.7.0** (CalVer; aligns with the
  cross-product 26.7.x release train).
- **Extracted a shared `Tools/ToolError.cs`** (`ToolError.Format(op, file, ex)`).
  All five tools (`redact_text`, `erase_metadata`, `redact_annotations`,
  `redact_image_area`, `get_document_info`) previously carried an identical
  private `FormatException` helper; they now call the single shared formatter.
  The on-the-wire failure text is byte-for-byte unchanged
  (`"<op> failed for '<file>': <Type>: <msg>[ | inner(n): …]"`, Pitfall #18).
- Bumped pinned `@26.7.0` examples in `README.md` and `llms.txt`.

## Why
Routine engine refresh to the latest stable GroupDocs.Redaction, plus removal of
five-way duplicated error-formatting code now that the tool surface is stable at
five tools.

## Migration / impact
- No API or tool-surface change: still five tools, identical parameters and
  response shapes.
- SkiaSharp stays pinned at 3.119.1 — the version `GroupDocs.Redaction.Net100`
  26.6.0 declares transitively (re-verified against the 26.6.0 nuspec; ABI-sensitive).
- The `GdiPlusResolver` and the `SkiaSharp.NativeAssets.Linux.NoDependencies` pin
  remain in place; native prerequisites (libgdiplus) are unchanged.
