using System.Text;

namespace GroupDocs.Redaction.Mcp.Tools;

// Shared descriptive-error formatter for the tool surface. Engine failures are
// surfaced as text (not MCP's opaque "An error occurred invoking '<tool>'") so
// AI agents and integration tests can read the cause. The text always starts
// with "<op> failed for '<file>': ..." — integration tests match that prefix
// (Pitfall #18). Extracted here because all five tools shared an identical
// per-class FormatException helper.
internal static class ToolError
{
    public static string Format(string op, string file, Exception ex)
    {
        var sb = new StringBuilder();
        sb.Append($"{op} failed for '{file}': {ex.GetType().FullName}: {ex.Message}");
        var inner = ex.InnerException;
        for (int d = 0; inner != null && d < 5; d++, inner = inner.InnerException)
        {
            sb.Append($" | inner({d}): {inner.GetType().FullName}: {inner.Message}");
        }
        return sb.ToString();
    }
}
