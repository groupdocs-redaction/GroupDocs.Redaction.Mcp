using System.ComponentModel;
using System.Text;
using GroupDocs.Mcp.Core;
using GroupDocs.Mcp.Core.Licensing;
using GroupDocs.Redaction.Options;
using GroupDocs.Redaction.Redactions;
using ModelContextProtocol.Server;

namespace GroupDocs.Redaction.Mcp.Tools;

[McpServerToolType]
public static class RedactTextTool
{
    [McpServerTool, Description(
        "Redacts text matching a regex pattern from a document and saves the redacted file to storage. " +
        "Use to permanently hide sensitive data like SSNs, emails, phone numbers, or any custom pattern. " +
        "Call this tool immediately whenever the user asks to redact, hide, remove, or black out text in a document. " +
        "Do NOT pre-check whether files exist — just pass the filename the user provided. " +
        "The tool resolves files from storage and returns an error with available files if a name is not found. " +
        "On failure, the response text starts with 'Text redaction failed for' followed by the underlying exception type, message, and inner-exception chain.")]
    public static async Task<string> RedactText(
        IFileResolver resolver,
        IFileStorage storage,
        ILicenseManager licenseManager,
        OutputHelper output,
        FileInput file,
        [Description("Regex pattern to match text for redaction, e.g. '\\d{3}-\\d{2}-\\d{4}' for SSN")] string pattern,
        [Description("Replacement text (default: '[REDACTED]')")] string replacement = "[REDACTED]",
        [Description("Password for protected documents")] string? password = null)
    {
        licenseManager.SetLicense();
        using var resolved = await resolver.ResolveAsync(file);

        try
        {
            var loadOptions = new LoadOptions { Password = password };
            using var redactor = new Redactor(resolved.Stream, loadOptions);

            var result = redactor.Apply(new RegexRedaction(pattern, new ReplacementOptions(replacement)));
            if (result.Status == RedactionStatus.Failed)
            {
                var errors = result.RedactionLog
                    .Select(e => e.Result.ErrorMessage)
                    .Where(m => !string.IsNullOrEmpty(m));
                return $"Text redaction failed: {string.Join("; ", errors)}";
            }

            var baseName = Path.GetFileNameWithoutExtension(resolved.FileName);
            var ext = Path.GetExtension(resolved.FileName);
            var outputName = $"{baseName}_redacted{ext}";

            using var ms = new MemoryStream();
            redactor.Save(ms, new RasterizationOptions { Enabled = false });

            var savedPath = await storage.WriteFileAsync(outputName, ms.ToArray(), rewrite: true);
            var prefix = licenseManager.IsLicensed ? string.Empty : "[Evaluation mode] Output may include watermarks.\n\n";
            return prefix + await output.BuildFileOutputAsync(savedPath, $"Redacted document (status: {result.Status})");
        }
        catch (Exception ex)
        {
            // Surface the underlying engine exception instead of letting it bubble
            // to MCP's generic "An error occurred invoking 'redact_text'." wrapper.
            // Pattern per Pitfall #18.
            return FormatException(ex, resolved.FileName);
        }
    }

    private static string FormatException(Exception ex, string fileName)
    {
        var sb = new StringBuilder();
        sb.Append($"Text redaction failed for '{fileName}': ");
        sb.Append($"{ex.GetType().FullName}: {ex.Message}");
        var inner = ex.InnerException;
        for (int depth = 0; inner != null && depth < 5; depth++, inner = inner.InnerException)
            sb.Append($" | inner({depth}): {inner.GetType().FullName}: {inner.Message}");
        return sb.ToString();
    }
}
