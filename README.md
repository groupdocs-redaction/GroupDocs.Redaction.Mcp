# GroupDocs.Redaction MCP Server

MCP server that exposes [GroupDocs.Redaction](https://products.groupdocs.com/redaction) as AI-callable tools
for Claude, Cursor, GitHub Copilot, and other MCP agents.

## Installation

Requires [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).

**Run directly with `dnx` (recommended — no install step):**

```bash
dnx GroupDocs.Redaction.Mcp --yes
```

Pulls the latest stable release on every invocation. To pin to a specific
version (recommended for shared configs and CI), append `@<version>`:

```bash
dnx GroupDocs.Redaction.Mcp@26.5.0 --yes
```

**Or install as a global dotnet tool:**

```bash
dotnet tool install -g GroupDocs.Redaction.Mcp
groupdocs-redaction-mcp
```

**Or run via Docker:**

```bash
docker run --rm -i \
  -v $(pwd)/documents:/data \
  ghcr.io/groupdocs-redaction/redaction-net-mcp:latest
```

## Native prerequisites

The underlying GroupDocs engine uses `System.Drawing` (GDI+) for image-area
redaction (drawing solid-color boxes over page regions). When you run the server
**natively** (via `dnx` or the global dotnet tool) on Linux or macOS, install
the native `libgdiplus` library first:

| Platform | Setup |
|---|---|
| Windows | Nothing — GDI+ is built into the OS. |
| Linux | `sudo apt-get install -y libgdiplus libfontconfig1` |
| macOS | `brew install mono-libgdiplus` |
| Docker | Nothing — the image already bundles `libgdiplus`. |

Skipping this on Linux/macOS surfaces as `DllNotFoundException: libgdiplus` in
the tool response. The simplest zero-setup option on Linux/macOS is the
**Docker image**.

## Available MCP Tools

| Tool | Description |
|---|---|
| `RedactText` | Redact text matching a regex pattern in a document; saves result as `<name>_redacted.<ext>` |
| `EraseMetadata` | Erase document metadata fields (author, title, company, dates, and other properties) |
| `RedactAnnotations` | Replace or delete annotations and comments in a document |
| `RedactImageArea` | Cover a rectangular page region with a solid-color box (pixel coordinates) |
| `GetDocumentInfo` | Return file type, page count, size, and per-page dimensions as JSON (no modification) |

## Example prompts

- "Redact all occurrences of 'John Smith' in contract.docx"
- "Remove all metadata from report.pdf before sharing it externally"
- "Delete all comments and annotations from review.docx"
- "Cover the signature area on page 1 of agreement.pdf with a black box from (100,200) to (400,300)"
- "How many pages does confidential.pdf have?"

## Configuration

| Variable | Description | Default |
|---|---|---|
| `GROUPDOCS_MCP_STORAGE_PATH` | Base folder for input and output files | current directory |
| `GROUPDOCS_MCP_OUTPUT_PATH` | *(Optional)* separate folder for output files | `GROUPDOCS_MCP_STORAGE_PATH` |
| `GROUPDOCS_LICENSE_PATH` | Path to GroupDocs license file | (evaluation mode) |

## Usage with Claude Desktop

```json
{
  "mcpServers": {
    "groupdocs-redaction": {
      "type": "stdio",
      "command": "dnx",
      "args": ["GroupDocs.Redaction.Mcp", "--yes"],
      "env": {
        "GROUPDOCS_MCP_STORAGE_PATH": "/path/to/documents"
      }
    }
  }
}
```

> To pin to a specific version, replace `"GroupDocs.Redaction.Mcp"` with
> `"GroupDocs.Redaction.Mcp@26.5.0"` in `args`. Pinning is recommended for
> shared / committed configs to avoid surprise upgrades.

## Usage with VS Code / GitHub Copilot

NuGet.org generates a ready-to-use `mcp.json` snippet on the [package page](https://www.nuget.org/packages/GroupDocs.Redaction.Mcp).
Copy it directly into your `.vscode/mcp.json`.

Alternatively, add manually to `.vscode/mcp.json`:

```json
{
  "inputs": [
    {
      "type": "promptString",
      "id": "storage_path",
      "description": "Base folder for input and output files.",
      "password": false
    }
  ],
  "servers": {
    "groupdocs-redaction": {
      "type": "stdio",
      "command": "dnx",
      "args": ["GroupDocs.Redaction.Mcp", "--yes"],
      "env": {
        "GROUPDOCS_MCP_STORAGE_PATH": "${input:storage_path}"
      }
    }
  }
}
```

> Same pinning rule as above — swap `"GroupDocs.Redaction.Mcp"` for
> `"GroupDocs.Redaction.Mcp@26.5.0"` to lock to a specific release.

## Usage with Docker Compose

```bash
cd docker
docker compose up
```

Edit `docker/docker-compose.yml` to point volumes at your local documents folder.

## Documentation & guides

Step-by-step deployment guides and a published-package integration test suite
live in the companion repo
[**GroupDocs.Redaction.Mcp.Tests**](https://github.com/groupdocs-redaction/GroupDocs.Redaction.Mcp.Tests):

- [Install from NuGet](https://github.com/groupdocs-redaction/GroupDocs.Redaction.Mcp.Tests/blob/master/how-to/01-install-from-nuget.md) — `dnx`, global tool, pinned vs always-latest
- [Run via Docker](https://github.com/groupdocs-redaction/GroupDocs.Redaction.Mcp.Tests/blob/master/how-to/02-run-via-docker.md)
- [Verify on the MCP registry](https://github.com/groupdocs-redaction/GroupDocs.Redaction.Mcp.Tests/blob/master/how-to/03-verify-mcp-registry.md)
- [Use with Claude Desktop](https://github.com/groupdocs-redaction/GroupDocs.Redaction.Mcp.Tests/blob/master/how-to/04-use-with-claude-desktop.md)
- [Use with VS Code / GitHub Copilot](https://github.com/groupdocs-redaction/GroupDocs.Redaction.Mcp.Tests/blob/master/how-to/05-use-with-vscode-copilot.md)
- [Run the integration tests](https://github.com/groupdocs-redaction/GroupDocs.Redaction.Mcp.Tests/blob/master/how-to/06-run-integration-tests.md)

That repo also exercises every advertised tool against the **published** NuGet
artifact on Linux, macOS, and Windows in CI — so the snippets above are
verified end-to-end on every release.

## License

MIT — see [LICENSE](LICENSE)

<!-- mcp-name: io.github.groupdocs-redaction/groupdocs-redaction-mcp -->
