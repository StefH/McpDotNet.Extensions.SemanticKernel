using System.Text.Json.Serialization;

namespace ModelContextProtocol.SemanticKernel.Types;

internal class ClaudeConfig : McpConfig<ClaudeMcpServer>
{
    [JsonRequired]
    [JsonPropertyName("mcpServers")]
    public override Dictionary<string, ClaudeMcpServer> McpServers { get; set; } = [];
}