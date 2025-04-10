using System.Text.Json.Serialization;

namespace ModelContextProtocol.SemanticKernel.Types;

public class ClaudeMcpServer
{
    [JsonPropertyName("command")] 
    public string Command { get; set; } = null!;

    [JsonPropertyName("args")]
    public List<string>? Args { get; set; }

    [JsonPropertyName("env")]
    public Dictionary<string, string>? Env { get; set; }

    [JsonPropertyName("disabled")]
    public bool Disabled { get; set; }
}