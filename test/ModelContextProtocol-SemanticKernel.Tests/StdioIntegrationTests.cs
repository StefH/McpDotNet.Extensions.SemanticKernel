using System.Text.Json;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
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
        tools.Select(t => t.Name).Should().BeEquivalentTo("add", "echo", "add_complex");
    }

    [Theory]
    [InlineData("add", new object[] { 1, 2 }, "The sum of 1 and 2 is 3.")]
    [InlineData("echo", new object[] { "test" }, "test")]
    public async Task CallTool(string toolName, object[] args, string expectedResult)
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        await using var mcpClient = await GetStdioEveryThingMcpClientAsync(ct);

        var tool = (await mcpClient.ListToolsAsync(cancellationToken: ct)).First(t => t.Name == toolName);
        var parameterNames = ToParameterNames(tool);
        var arguments = new Dictionary<string, object?>();
        for (var i = 0; i < args.Length; i++)
        {
            arguments[parameterNames[i]] = args[i];
        }

        // Act
        var result = await mcpClient.CallToolAsync(tool.Name, arguments, cancellationToken: ct);

        // Assert
        result.GetAllText().Should().Be(expectedResult);
        result.StructuredContent.Should().BeNull();
    }

    [Fact]
    public async Task CallToolWithComplexArguments()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        await using var mcpClient = await GetStdioEveryThingMcpClientAsync(ct);

        var tool = (await mcpClient.ListToolsAsync(cancellationToken: ct)).First(t => t.Name == "add_complex");
        var parameterNames = ToParameterNames(tool);

        var arguments = new Dictionary<string, object?>
        {
            [parameterNames[0]] = new { Real = 1, Imaginary = 2 },
            [parameterNames[1]] = new { Real = 9, Imaginary = -7 }
        };

        // Act
        var result = await mcpClient.CallToolAsync(tool.Name, arguments, cancellationToken: ct);

        // Assert
        var expectedJson = "{\u0022real\u0022:10,\u0022imaginary\u0022:-5}";
        result.GetAllText().Should().Be(expectedJson);
        JsonNode.DeepEquals(result.StructuredContent, JsonNode.Parse(expectedJson)).Should().BeTrue();
    }

    private static Task<McpClient> GetStdioEveryThingMcpClientAsync(CancellationToken cancellationToken)
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

        return McpClient.CreateAsync(clientTransport, options, cancellationToken: cancellationToken);
    }

    private static List<string> ToParameterNames(McpClientTool tool)
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