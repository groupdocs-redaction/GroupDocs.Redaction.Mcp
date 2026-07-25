# Codex CLI (OpenAI)

```bash
codex mcp add groupdocs-redaction -- dnx GroupDocs.Redaction.Mcp --yes
```

Or add to `~/.codex/config.toml`:

```toml
[mcp_servers.groupdocs-redaction]
command = "dnx"
args = ["GroupDocs.Redaction.Mcp", "--yes"]

[mcp_servers.groupdocs-redaction.env]
GROUPDOCS_MCP_STORAGE_PATH = "/path/to/documents"
# GROUPDOCS_LICENSE_PATH = "/path/to/GroupDocs.Total.lic"   # omit for evaluation mode
```

Pin a version by replacing `GroupDocs.Redaction.Mcp` with `GroupDocs.Redaction.Mcp@26.7.1`.
