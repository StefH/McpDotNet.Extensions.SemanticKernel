using System.Text.Json.Serialization;

namespace ModelContextProtocol.SemanticKernel.Types;

public class ClaudeConfig
{
    [JsonRequired]
    [JsonPropertyName("mcpServers")] 
    public Dictionary<string, ClaudeMcpServer> McpServers { get; set; } = [];
}