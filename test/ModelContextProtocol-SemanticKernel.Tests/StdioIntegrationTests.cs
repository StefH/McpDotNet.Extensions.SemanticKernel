using System.Text.Json;
using FluentAssertions;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol.Transport;
using ModelContextProtocol.SemanticKernel.Types;

namespace ModelContextProtocol.SemanticKernel.Tests;

public sealed class StdioIntegrationTests
{
    [Fact]
    public async Task ListTools()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        await using var mcpClient = await GetStdioEveryThingMcpClientAsync(ct);

        // Act
        var tools = await mcpClient.ListToolsAsync(cancellationToken: ct);

        // Assert
        tools.Should().HaveCount(2);
    }

    [Theory]
    [InlineData("Add", new object[] { 1, 2 }, "The sum of 1 and 2 is 3.")]
    [InlineData("Echo", new object[] { "test" }, "test")]
    public async Task CallTool(string toolName, object[] args, string expectedResult)
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        await using var mcpClient = await GetStdioEveryThingMcpClientAsync(ct);

        var tool = (await mcpClient.ListToolsAsync(cancellationToken: ct)).First(t => t.Name == toolName);
        var toolParameters = ToParameters(tool);
        var arguments = new Dictionary<string, object?>();
        for (var i = 0; i < args.Length; i++)
        {
            arguments[toolParameters[i]] = args[i];
        }

        // Act
        var result = await mcpClient.CallToolAsync(toolName, arguments, cancellationToken: ct);

        // Assert
        string.Concat(result.Content.Select(c => c.Text)).Should().Be(expectedResult);
    }

    private static Task<IMcpClient> GetStdioEveryThingMcpClientAsync(CancellationToken cancellationToken)
    {
        McpClientOptions options = new()
        {
            ClientInfo = new() { Name = "Everything Test", Version = "1.0.0" }
        };

        var stdioClientTransportOptions = new StdioClientTransportOptions
        {
            Command = "mcpserver.everything.stdio",
            Name = "Everything"
        };
        var clientTransport = new StdioClientTransport(stdioClientTransportOptions);

        return McpClientFactory.CreateAsync(clientTransport, options, cancellationToken: cancellationToken);
    }

    private static List<string> ToParameters(McpClientTool tool)
    {
        var inputSchema = tool.JsonSchema.Deserialize<JsonSchema>();
        var properties = inputSchema?.Properties;
        if (properties == null)
        {
            return [];
        }

        return [.. inputSchema!.Required ?? []];
    }
}