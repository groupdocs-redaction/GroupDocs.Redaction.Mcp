# GroupDocs.Redaction MCP Server

MCP server that exposes [GroupDocs.Redaction](https://products.groupdocs.com/redaction) as AI-callable tools for Claude, Cursor, GitHub Copilot, and other MCP agents.

## Quick start

```bash
docker run --rm -i \
  -v $(pwd)/documents:/data \
  groupdocs/redaction-net-mcp:latest
```

## Use with Claude Desktop

```json
{
  "mcpServers": {
    "groupdocs-redaction": {
      "command": "docker",
      "args": ["run", "--rm", "-i", "-v", "/path/to/documents:/data", "groupdocs/redaction-net-mcp:latest"]
    }
  }
}
```

## Tools

- **RedactText** — Redact text matching a regex pattern in a document; saves result as `<name>_redacted.<ext>`
- **EraseMetadata** — Erase document metadata fields (author, title, company, dates, and other properties)
- **RedactAnnotations** — Replace or delete annotations and comments in a document
- **RedactImageArea** — Cover a rectangular page region with a solid-color box (pixel coordinates)
- **GetDocumentInfo** — Return file type, page count, size, and per-page dimensions as JSON (no modification)

## Tags & environment

- Tags: `latest` + an immutable version tag per release matching NuGet (e.g. `26.7.1`).
  Platforms: `linux/amd64`, `linux/arm64`. Also on GHCR: `ghcr.io/groupdocs-redaction/redaction-net-mcp`.
- `GROUPDOCS_MCP_STORAGE_PATH` (default `/data`), `GROUPDOCS_MCP_OUTPUT_PATH` (optional),
  `GROUPDOCS_LICENSE_PATH` — mount your license and point at it to leave evaluation mode
  (see the Licensing section in the GitHub README for the exact evaluation limits).

Full docs, one-click installs for other clients, and licensing details:
**https://github.com/groupdocs-redaction/GroupDocs.Redaction.Mcp**
