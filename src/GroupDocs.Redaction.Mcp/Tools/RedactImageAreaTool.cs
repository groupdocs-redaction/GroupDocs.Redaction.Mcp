using System.ComponentModel;
using System.Drawing;
using GroupDocs.Mcp.Core;
using GroupDocs.Mcp.Core.Licensing;
using GroupDocs.Redaction.Options;
using GroupDocs.Redaction.Redactions;
using ModelContextProtocol.Server;
// Engine 26.6.0 deprecated the System.Drawing-typed ImageAreaRedaction /
// RegionReplacementOptions overloads ("System.Drawing support will be removed in
// a future release"). Use the GroupDocs.Redaction.Options.Drawing types instead.
using GdColor = GroupDocs.Redaction.Options.Drawing.Color;
using GdPoint = GroupDocs.Redaction.Options.Drawing.Point;
using GdSize = GroupDocs.Redaction.Options.Drawing.Size;

namespace GroupDocs.Redaction.Mcp.Tools;

[McpServerToolType]
public static class RedactImageAreaTool
{
    [McpServerTool, Description(
        "Covers a rectangular area of a document page with a solid-color box, permanently hiding image content (e.g. faces, signatures, stamps). " +
        "Coordinates are in pixels from the top-left corner of the page. " +
        "Call this tool whenever the user asks to hide, cover, or black out an image area or region in a document. " +
        "Do NOT pre-check whether files exist — just pass the filename the user provided. " +
        "The tool resolves files from storage and returns an error with available files if a name is not found. " +
        "On failure, the response text starts with 'Image area redaction failed for' followed by the underlying exception type, message, and inner-exception chain.")]
    public static async Task<string> RedactImageArea(
        IFileResolver resolver,
        IFileStorage storage,
        ILicenseManager licenseManager,
        OutputHelper output,
        FileInput file,
        [Description("X coordinate (pixels) of the top-left corner of the area to redact")] int x,
        [Description("Y coordinate (pixels) of the top-left corner of the area to redact")] int y,
        [Description("Width (pixels) of the area to redact")] int width,
        [Description("Height (pixels) of the area to redact")] int height,
        [Description("Fill color for the redaction box — named color (e.g. 'Black', 'Red') or hex (e.g. '#FF0000'). Default: 'Black'")] string color = "Black",
        [Description("Password for protected documents")] string? password = null)
    {
        licenseManager.SetLicense();
        using var resolved = await resolver.ResolveAsync(file);

        try
        {
            var fillColor = ParseColor(color);
            var redaction = new ImageAreaRedaction(
                new GdPoint(x, y),
                new RegionReplacementOptions(fillColor, new GdSize(width, height)));

            var loadOptions = new LoadOptions { Password = password };
            using var redactor = new Redactor(resolved.Stream, loadOptions);

            var result = redactor.Apply(redaction);
            if (result.Status == RedactionStatus.Failed)
            {
                var errors = result.RedactionLog
                    .Select(e => e.Result.ErrorMessage)
                    .Where(m => !string.IsNullOrEmpty(m));
                return $"Image area redaction failed: {string.Join("; ", errors)}";
            }

            var baseName = Path.GetFileNameWithoutExtension(resolved.FileName);
            var ext = Path.GetExtension(resolved.FileName);
            var outputName = $"{baseName}_redacted{ext}";

            using var ms = new MemoryStream();
            redactor.Save(ms, new RasterizationOptions { Enabled = false });

            var savedPath = await storage.WriteFileAsync(outputName, ms.ToArray(), rewrite: true);
            var prefix = licenseManager.IsLicensed ? string.Empty : "[Evaluation mode] Output may include watermarks.\n\n";
            return prefix + await output.BuildFileOutputAsync(savedPath,
                $"Image area redacted at ({x},{y}) size {width}×{height} with {color} (status: {result.Status})");
        }
        catch (Exception ex)
        {
            // Surface the engine exception via the shared descriptive formatter
            // instead of MCP's generic "An error occurred invoking 'redact_image_area'."
            // wrapper (Pitfall #18).
            return ToolError.Format("Image area redaction", resolved.FileName, ex);
        }
    }

    private static GdColor ParseColor(string color)
    {
        // ColorTranslator.FromHtml parses both named colors and hex ('#RRGGBB').
        // Convert its System.Drawing.Color result into the engine's own color type.
        try
        {
            var c = ColorTranslator.FromHtml(color);
            return GdColor.FromArgb(c.A, c.R, c.G, c.B);
        }
        catch { return GdColor.Black; }
    }
}
