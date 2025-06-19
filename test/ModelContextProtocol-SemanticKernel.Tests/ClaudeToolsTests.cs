using FluentAssertions;
using Microsoft.SemanticKernel;
using ModelContextProtocol.SemanticKernel.Extensions;

namespace ModelContextProtocol.SemanticKernel.Tests;

public sealed class ClaudeToolsTests
{
    [Fact]
    public async Task ListTools()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        // TODO: see how to mock file-system

        // Act
        var tools = await new KernelPluginCollection()
            .AddToolsFromClaudeDesktopConfigAsync(null, ct);

       

        // Assert
        tools.Should().HaveCount(1);
    }
}
