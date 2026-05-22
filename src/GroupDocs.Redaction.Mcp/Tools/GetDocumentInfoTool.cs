using System.ComponentModel;
using System.Text;
using System.Text.Json;
using GroupDocs.Mcp.Core;
using GroupDocs.Mcp.Core.Licensing;
using GroupDocs.Redaction.Options;
using ModelContextProtocol.Server;

namespace GroupDocs.Redaction.Mcp.Tools;

[McpServerToolType]
public static class GetDocumentInfoTool
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    [McpServerTool, Description(
        "Returns the file type, page count, size, and per-page dimensions of a document as JSON, without modifying the file. " +
        "Supports PDF, DOCX, XLSX, PPTX, images, and 30+ more document formats. " +
        "Call this tool whenever the user asks to inspect a document, check its page count, or get its details — " +
        "useful as a precondition before redacting (e.g. to read page width/height before choosing redact_image_area coordinates). " +
        "Do NOT pre-check whether the file exists — just pass the filename the user provided. " +
        "Returns a JSON object with fields `fileName`, `fileType`, `pageCount`, `size`, and `pages` (array of `{ number, width, height }`). " +
        "On failure, the response text starts with 'Document-info lookup failed for' followed by the underlying exception type, message, and inner-exception chain.")]
    public static async Task<string> GetDocumentInfo(
        IFileResolver resolver,
        ILicenseManager licenseManager,
        FileInput file,
        [Description("Password for protected documents")] string? password = null)
    {
        licenseManager.SetLicense();
        using var resolved = await resolver.ResolveAsync(file);

        try
        {
            var loadOptions = new LoadOptions { Password = password };
            using var redactor = new Redactor(resolved.Stream, loadOptions);

            var info = redactor.GetDocumentInfo();
            if (info == null)
                return $"Could not retrieve document information for '{resolved.FileName}'.";

            var payload = new
            {
                fileName = resolved.FileName,
                fileType = info.FileType.ToString(),
                pageCount = info.PageCount,
                size = info.Size,
                pages = info.Pages?
                    .Select((p, i) => new { number = i + 1, width = p.Width, height = p.Height })
                    .ToList(),
            };

            // Raw JSON — never piped through OutputHelper.TruncateText (Pitfall #16).
            return JsonSerializer.Serialize(payload, JsonOptions);
        }
        catch (Exception ex)
        {
            // Surface the underlying engine exception instead of letting it bubble
            // to MCP's generic "An error occurred invoking 'get_document_info'."
            // wrapper. Pattern per Pitfall #18.
            return FormatException(ex, resolved.FileName);
        }
    }

    private static string FormatException(Exception ex, string fileName)
    {
        var sb = new StringBuilder();
        sb.Append($"Document-info lookup failed for '{fileName}': ");
        sb.Append($"{ex.GetType().FullName}: {ex.Message}");
        var inner = ex.InnerException;
        for (int depth = 0; inner != null && depth < 5; depth++, inner = inner.InnerException)
            sb.Append($" | inner({depth}): {inner.GetType().FullName}: {inner.Message}");
        return sb.ToString();
    }
}
