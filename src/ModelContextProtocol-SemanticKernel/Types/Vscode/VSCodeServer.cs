using System.Text.Json.Serialization;

namespace ModelContextProtocol.SemanticKernel.Types;

public class VSCodeServer : IMcpServer
{
    [JsonRequired]
    [JsonPropertyName("command")]
    public string Command { get; set; }

    [JsonPropertyName("args")]
    public List<string>? Args { get; set; }

    [JsonPropertyName("env")]
    public Dictionary<string, string>? Env { get; set; }

    [JsonPropertyName("envFile")]
    public string? EnvFilePath { get; set; } 

    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public VSCodeServerType? Type { get; set; } = VSCodeServerType.Stdio;

    /// <summary>
    /// TBD: VS Code does not provide an option to disable servers in the MCP configuration, but property might still be useful for in-code setting
    /// </summary>
    public bool Disabled { get; set; } = false;
}