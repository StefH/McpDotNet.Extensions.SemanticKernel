using FluentAssertions;
using Microsoft.SemanticKernel;
using ModelContextProtocol.SemanticKernel.Extensions;

namespace ModelContextProtocol.SemanticKernel.Tests;

public sealed class VsCodeToolsTests
{
    [Fact]
    public async Task WorkspacePathDiscoveryTest()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var path = Path.Combine("TestContext.Current.TestDirectory", "settings.json");
        // TODO: see how to mock file-system

        // Act
        var tools = await new KernelPluginCollection()
            .AddToolsFromVsCodeConfigAsync(path,
                                           loggerFactory: null,
                                           cancellationToken: ct);

        Assert.Fail();
    }

    [Fact]
    public async Task UserSettingsVsCodeDiscoveryTest()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        // TODO: see how to mock file-system

        // Act
        var tools = await new KernelPluginCollection()
            .AddToolsFromVsCodeConfigAsync(instanceType: Types.VsCodeInstanceType.VisualStudioCode, 
                                           loggerFactory: null,
                                           cancellationToken: ct);

        Assert.Fail();
    }

    [Fact]
    public async Task UserSettingsVsCodeInsidersDiscoveryTest()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        // TODO: see how to mock file-system

        // Act
        var tools = await new KernelPluginCollection()
            .AddToolsFromVsCodeConfigAsync(instanceType: Types.VsCodeInstanceType.VisualStudioCodeInsiders,
                                           loggerFactory: null, 
                                           cancellationToken: ct);

        Assert.Fail();
    }

    // TODO: Test passing input's to configuration file
}
