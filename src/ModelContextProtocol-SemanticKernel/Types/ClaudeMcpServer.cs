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

    public Dictionary<string, string> ToTransportOptions()
    {
        var transportOptions = new Dictionary<string, string>
        {
            ["command"] = Command
        };

        if (Args != null)
        {
            transportOptions["arguments"] = string.Join(" ", Args);
        }

        if (Env != null)
        {
            foreach (var env in Env)
            {
                transportOptions[$"env:{env.Key}"] = env.Value;
            }
        }

        return transportOptions;
    }
}