# Claude Code

```bash
claude mcp add groupdocs-redaction -- dnx GroupDocs.Redaction.Mcp --yes
```

With storage folder and license:

```bash
claude mcp add groupdocs-redaction -e GROUPDOCS_MCP_STORAGE_PATH=/path/to/documents -e GROUPDOCS_LICENSE_PATH=/path/to/GroupDocs.Total.lic -- dnx GroupDocs.Redaction.Mcp --yes
```

Pin a version by replacing `GroupDocs.Redaction.Mcp` with `GroupDocs.Redaction.Mcp@26.7.1`.
