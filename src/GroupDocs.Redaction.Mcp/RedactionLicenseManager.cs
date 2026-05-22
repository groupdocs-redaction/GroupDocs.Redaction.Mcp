using GroupDocs.Mcp.Core;
using GroupDocs.Mcp.Core.Licensing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GroupDocs.Redaction.Mcp;

public class RedactionLicenseManager : LicenseManager
{
    public RedactionLicenseManager(IOptions<McpConfig> config, ILogger<LicenseManager> logger)
        : base(config, logger) { }

    protected override void SetLicenseFromPath(string licensePath)
    {
        new GroupDocs.Redaction.License().SetLicense(licensePath);
    }
}
