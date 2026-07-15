using System.ComponentModel;
using GroupDocs.Mcp.Core;
using GroupDocs.Mcp.Core.Licensing;
using GroupDocs.Redaction.Options;
using GroupDocs.Redaction.Redactions;
using ModelContextProtocol.Server;

namespace GroupDocs.Redaction.Mcp.Tools;

[McpServerToolType]
public static class RedactAnnotationsTool
{
    [McpServerTool, Description(
        "Redacts or deletes annotations (comments, sticky notes, highlights) in a document and saves the result to storage. " +
        "Can replace matching annotation text with a placeholder, or delete annotations entirely. " +
        "Call this tool whenever the user asks to redact, remove, hide, or clean up comments or annotations in a document. " +
        "Do NOT pre-check whether files exist — just pass the filename the user provided. " +
        "The tool resolves files from storage and returns an error with available files if a name is not found. " +
        "On failure, the response text starts with 'Annotation redaction failed for' followed by the underlying exception type, message, and inner-exception chain.")]
    public static async Task<string> RedactAnnotations(
        IFileResolver resolver,
        IFileStorage storage,
        ILicenseManager licenseManager,
        OutputHelper output,
        FileInput file,
        [Description("Regex pattern to match annotations. Omit to target all annotations.")] string? pattern = null,
        [Description("Replacement text for matched annotation content. Ignored when deleteAll is true (default: '[REDACTED]')")] string replacement = "[REDACTED]",
        [Description("When true, deletes matched annotations entirely instead of replacing their text (default: false)")] bool deleteAll = false,
        [Description("Password for protected documents")] string? password = null)
    {
        licenseManager.SetLicense();
        using var resolved = await resolver.ResolveAsync(file);

        try
        {
            Redaction redaction = deleteAll
                ? (pattern != null ? new DeleteAnnotationRedaction(pattern) : new DeleteAnnotationRedaction())
                : (pattern != null ? new AnnotationRedaction(pattern, replacement) : new AnnotationRedaction(".*", replacement));

            var loadOptions = new LoadOptions { Password = password };
            using var redactor = new Redactor(resolved.Stream, loadOptions);

            var result = redactor.Apply(redaction);
            if (result.Status == RedactionStatus.Failed)
            {
                var errors = result.RedactionLog
                    .Select(e => e.Result.ErrorMessage)
                    .Where(m => !string.IsNullOrEmpty(m));
                return $"Annotation redaction failed: {string.Join("; ", errors)}";
            }

            var baseName = Path.GetFileNameWithoutExtension(resolved.FileName);
            var ext = Path.GetExtension(resolved.FileName);
            var outputName = $"{baseName}_redacted{ext}";

            using var ms = new MemoryStream();
            redactor.Save(ms, new RasterizationOptions { Enabled = false });

            var action = deleteAll ? "deleted" : "replaced";
            var savedPath = await storage.WriteFileAsync(outputName, ms.ToArray(), rewrite: true);
            var prefix = licenseManager.IsLicensed ? string.Empty : "[Evaluation mode] Output may include watermarks.\n\n";
            return prefix + await output.BuildFileOutputAsync(savedPath, $"Annotations {action} (status: {result.Status})");
        }
        catch (Exception ex)
        {
            // Surface the engine exception via the shared descriptive formatter
            // instead of MCP's generic "An error occurred invoking 'redact_annotations'."
            // wrapper (Pitfall #18).
            return ToolError.Format("Annotation redaction", resolved.FileName, ex);
        }
    }
}
