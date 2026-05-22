using GroupDocs.Mcp.Core;
using GroupDocs.Mcp.Core.Licensing;
using GroupDocs.Redaction.Mcp.Tools;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace GroupDocs.Redaction.Mcp.Tests;

public class RedactImageAreaToolTests
{
    private readonly Mock<IFileResolver> _resolver = new();
    private readonly Mock<ILicenseManager> _licenseManager = new();
    private readonly Mock<IFileStorage> _storage = new();
    private readonly OutputHelper _output;

    public RedactImageAreaToolTests()
    {
        _output = new OutputHelper(_storage.Object, Microsoft.Extensions.Options.Options.Create(new McpConfig()));
    }

    [Fact]
    public async Task RedactImageArea_WhenResolverThrows_PropagatesException()
    {
        _resolver
            .Setup(r => r.ResolveAsync(It.IsAny<FileInput>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FileNotFoundException("missing.pdf"));

        await Assert.ThrowsAsync<FileNotFoundException>(() =>
            RedactImageAreaTool.RedactImageArea(
                _resolver.Object, _storage.Object, _licenseManager.Object, _output,
                new FileInput { FilePath = "missing.pdf" }, x: 0, y: 0, width: 10, height: 10));
    }

    [Fact]
    public async Task RedactImageArea_WhenResolverThrows_DoesNotWriteToStorage()
    {
        _resolver
            .Setup(r => r.ResolveAsync(It.IsAny<FileInput>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FileNotFoundException("missing.pdf"));

        await Assert.ThrowsAsync<FileNotFoundException>(() =>
            RedactImageAreaTool.RedactImageArea(
                _resolver.Object, _storage.Object, _licenseManager.Object, _output,
                new FileInput { FilePath = "missing.pdf" }, x: 0, y: 0, width: 10, height: 10));

        _storage.Verify(
            s => s.WriteFileAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RedactImageArea_SetsLicense_BeforeResolving()
    {
        var sequence = new List<string>();

        _licenseManager.Setup(l => l.SetLicense()).Callback(() => sequence.Add("license"));
        _resolver
            .Setup(r => r.ResolveAsync(It.IsAny<FileInput>(), It.IsAny<CancellationToken>()))
            .Callback(() => sequence.Add("resolve"))
            .ThrowsAsync(new InvalidOperationException("short-circuit"));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            RedactImageAreaTool.RedactImageArea(
                _resolver.Object, _storage.Object, _licenseManager.Object, _output,
                new FileInput { FilePath = "anything.pdf" }, x: 0, y: 0, width: 10, height: 10));

        Assert.Equal(new[] { "license", "resolve" }, sequence);
    }

    [Fact]
    public async Task RedactImageArea_PassesFileInputToResolver_Unchanged()
    {
        var input = new FileInput { FilePath = "doc.pdf" };
        FileInput? captured = null;

        _resolver
            .Setup(r => r.ResolveAsync(It.IsAny<FileInput>(), It.IsAny<CancellationToken>()))
            .Callback<FileInput, CancellationToken>((fi, _) => captured = fi)
            .ThrowsAsync(new InvalidOperationException("short-circuit"));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            RedactImageAreaTool.RedactImageArea(
                _resolver.Object, _storage.Object, _licenseManager.Object, _output,
                input, x: 0, y: 0, width: 10, height: 10));

        Assert.Same(input, captured);
    }
}
