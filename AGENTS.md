# AGENTS.md — Guide for AI coding agents

Brief orientation for AI coding agents (Claude Code, Copilot, Cursor, Aider, Amp, Codex) working in this repository.

## What this repo is

A standalone **MCP server** for [GroupDocs.Redaction for .NET](https://products.groupdocs.com/redaction) — exposes document redaction operations as AI-callable tools via the Model Context Protocol.

Published to NuGet as `GroupDocs.Redaction.Mcp` with the `McpServer` package type, and to `ghcr.io/groupdocs-redaction/redaction-net-mcp` + `docker.io/groupdocs/redaction-net-mcp` as a container image.

## MCP tools exposed

| Tool | Description |
|---|---|
| `RedactText` | Redact text matching a regex pattern in a document; saves result as `<name>_redacted.<ext>` |
| `EraseMetadata` | Erase document metadata fields (author, title, company, dates, and other properties) |
| `RedactAnnotations` | Replace or delete annotations and comments in a document |
| `RedactImageArea` | Cover a rectangular page region with a solid-color box (pixel coordinates) |
| `GetDocumentInfo` | Return file type, page count, size, and per-page dimensions as JSON (no modification) |

All tools accept `FileInput` (resolved via `IFileResolver`). The four redaction tools (`RedactText`, `EraseMetadata`, `RedactAnnotations`, `RedactImageArea`) write output files to storage; `GetDocumentInfo` is read-only and returns JSON.

## Folder layout

```
src/                                             ← all projects + sln + Directory.Build.props
  GroupDocs.Redaction.Mcp/
    Program.cs                                   ← host bootstrap + stdio transport
    RedactionLicenseManager.cs                   ← applies GroupDocs.Total license
    Tools/
      RedactTextTool.cs                          ← [McpServerTool] — RedactText
      EraseMetadataTool.cs                       ← [McpServerTool] — EraseMetadata
      RedactAnnotationsTool.cs                   ← [McpServerTool] — RedactAnnotations
      RedactImageAreaTool.cs                     ← [McpServerTool] — RedactImageArea
      GetDocumentInfoTool.cs                     ← [McpServerTool] — GetDocumentInfo
    .mcp/
      server.json                                ← NuGet.org reads this to generate mcp.json snippet
    GroupDocs.Redaction.Mcp.csproj               ← PackageTypes=McpServer + ToolCommandName
  GroupDocs.Redaction.Mcp.Tests/
  GroupDocs.Redaction.Mcp.sln
  Directory.Build.props
build/
  dependencies.props                             ← single source of truth for all versions
changelog/                                       ← one MD file per change (see changelog/README.md)
docker/
  Dockerfile                                     ← multi-stage, runtime on aspnet:10.0
  docker-compose.yml
.github/workflows/                               ← build_packages.yml, run_tests.yml, publish_prod.yml, publish_docker.yml
```

## Dependencies

- `GroupDocs.Mcp.Core` + `GroupDocs.Mcp.Local.Storage` — infrastructure NuGet packages from the [GroupDocs.Mcp.Core](https://github.com/groupdocs/GroupDocs.Mcp.Core) repo
- `GroupDocs.Redaction` — the actual redaction engine (via `GroupDocs.Redaction.Net100`)
- `ModelContextProtocol` — MCP SDK for .NET
- `Microsoft.Extensions.Hosting` — host builder for the stdio server

## Commands you can run

```bash
# Restore + build
dotnet restore
dotnet build src/GroupDocs.Redaction.Mcp.sln -c Release

# Run tests
dotnet test src/GroupDocs.Redaction.Mcp.sln -c Release

# Run the server locally (stdio)
dotnet run --project src/GroupDocs.Redaction.Mcp

# Local pack (writes to ./build_out) — validates server.json version matches dependencies.props
pwsh ./build.ps1

# Build + run the Docker image
docker build -f docker/Dockerfile -t redaction-net-mcp:local .
docker run --rm -i -v $(pwd)/documents:/data redaction-net-mcp:local
```

## Version scheme

CalVer `YY.MM.N`. The version lives in **two** places that MUST stay in lockstep:
1. `build/dependencies.props` → `<GroupDocsRedactionMcp>`
2. `src/GroupDocs.Redaction.Mcp/.mcp/server.json` → both top-level `"version"` and `packages[0].version`

`build.ps1` enforces this at pack time (`Assert-ServerJsonVersionMatchesDependencies`) — if they drift, the build fails.

## Pre-shipped pitfall remediations

The following cross-product pitfalls were addressed at clone time and are already in the codebase:

- **Pitfall #18 (unhandled exceptions in tool methods)**: all five tools have a top-level try/catch that formats exceptions as structured MCP error responses rather than letting them propagate as unformatted stack traces. Do not remove these wrappers.
- **Pitfall #16 (N/A)**: Pitfall #16 applies to tools that return in-memory content rather than writing files. All four redaction tools write output files to storage, so Pitfall #16 does not apply here.

## Native-deps note

On Linux, `System.Drawing.Common` (used by `RedactImageArea` for pixel-box overlay) requires `libgdiplus` + `libfontconfig1`. The Dockerfile installs the base set only (`libgdiplus libfontconfig1`) — `ttf-mscorefonts` is not installed because Redaction does structural redaction and pixel-box overlays, not text-glyph rendering. `SkiaSharp.NativeAssets.Linux.NoDependencies 3.119.1` is included transitively via `GroupDocs.Redaction.Net100`. The `System.Drawing.EnableUnixSupport` runtime host config option is set in the csproj.

## House rules

1. **Tools must have rich `[Description("...")]` strings** — these are what AI agents read via the MCP protocol. Write them as task-oriented sentences, not method-signature summaries.
2. **Never add new env vars beyond** `GROUPDOCS_MCP_STORAGE_PATH`, `GROUPDOCS_MCP_OUTPUT_PATH`, `GROUPDOCS_LICENSE_PATH` without updating `server.json`, `docker-compose.yml`, and `README.md` together.
3. **Tests use xUnit + Moq** — mock `IFileResolver`, `IFileStorage`, `ILicenseManager`, `OutputHelper`.
4. **Changelog entries required** — any PR that changes behaviour adds `changelog/NNN-slug.md`.
5. **Do not edit `obj/` or `build_out/`** — build artifacts.
6. **Target framework is `net10.0` only** — required by `dnx` and the MCP SDK.

## Release flow

See [RELEASE.md](RELEASE.md) for the exact per-release checklist.

## What NOT to change

- Do not hardcode the version in `.csproj` — it flows from `$(GroupDocsRedactionMcp)` in `dependencies.props`.
- Do not remove the `<PackageTypes>McpServer</PackageTypes>` or `<ToolCommandName>groupdocs-redaction-mcp</ToolCommandName>` from the csproj — NuGet.org discoverability and `dnx` invocation depend on them.
- Do not change the `.mcp/server.json` schema URL without cross-checking with the NuGet MCP docs.
