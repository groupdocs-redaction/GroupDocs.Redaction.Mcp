---
id: 001
date: 2026-05-22
version: 26.5.0
type: feature
---

# Initial public release of GroupDocs.Redaction MCP Server

## What changed
- NuGet package `GroupDocs.Redaction.Mcp` published with `McpServer` package type.
- Five MCP tools exposed:
  - `RedactText` — redacts text matching a regex pattern in a document and saves the result as `<name>_redacted.<ext>`.
  - `EraseMetadata` — erases document metadata fields (author, title, company, dates, and other properties).
  - `RedactAnnotations` — replaces or deletes annotations and comments in a document.
  - `RedactImageArea` — covers a rectangular page region with a solid-color box (pixel coordinates).
  - `GetDocumentInfo` — returns file type, page count, size, and per-page dimensions as JSON without modifying the document.
- `GetDocumentInfo` added at clone time per the cross-product standard for MCP server repos.
- The four redaction tools (`RedactText`, `EraseMetadata`, `RedactAnnotations`, `RedactImageArea`) ported from the GroupDocs.Redaction framework subproject; a Pitfall #18 catch-and-format exception wrapper added at clone time to each tool.
- Installable via `dnx GroupDocs.Redaction.Mcp@26.5.0 --yes` (.NET 10 SDK required), `dotnet tool install -g GroupDocs.Redaction.Mcp`, or Docker.
- Docker image published to `ghcr.io/groupdocs-redaction/redaction-net-mcp` and `docker.io/groupdocs/redaction-net-mcp`.
- Environment variables: `GROUPDOCS_MCP_STORAGE_PATH`, optional `GROUPDOCS_MCP_OUTPUT_PATH`, `GROUPDOCS_LICENSE_PATH`.
- Native dependencies on Linux: `SkiaSharp.NativeAssets.Linux.NoDependencies 3.119.1` (pulled transitively via `GroupDocs.Redaction.Net100`) + `System.Drawing.EnableUnixSupport` runtime host config option. `System.Drawing.Common 9.0.3` is used for image-area redaction (rectangular box overlay). Dockerfile installs `libgdiplus libfontconfig1` (base set; no `ttf-mscorefonts` required — Redaction does structural redaction and pixel-box overlays, not text-glyph rendering).
- Evaluation mode produces watermarked output; no hard block on unlicensed use.

## Why
Exposes GroupDocs.Redaction for .NET as AI-callable MCP tools for Claude, Cursor,
VS Code / GitHub Copilot, and other MCP-compatible agents, enabling automated
document redaction workflows in AI pipelines.

## Migration / impact
First release — no migration required.
