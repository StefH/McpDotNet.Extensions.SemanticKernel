using System.Text.Json.Serialization;

namespace ModelContextProtocol.SemanticKernel.Types;

/// <summary>
/// Wrapper for deserialization
/// </summary>
internal class VsCodeSettings
{
    [JsonPropertyName("mcp")]
    public VSCodeConfig? McpSection { get; set; } = null!;
}