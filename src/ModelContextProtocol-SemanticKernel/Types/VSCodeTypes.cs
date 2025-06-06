using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;

namespace ModelContextProtocol.SemanticKernel.Types;

// Wrapper for deserialization
public class VisualStudioSettings
{
    [JsonPropertyName("mcp")]
    public VSCodeConfig? McpSection { get; set; } = null!;
}

public class VSCodeConfig : McpConfig<VSCodeServer>
{
    [JsonRequired]
    [JsonPropertyName("servers")]
    public override Dictionary<string, VSCodeServer> McpServers { get; set; } = [];


    [JsonPropertyName("input")]
    public List<VSCodeInput> Inputs { get; set; } = [];

    /// <summary>
    /// Replaces all occurrences of ${input:<input.id>} in the input string with the corresponding input's DefaultValue.
    /// Throws ArgumentException if an input id is not found in the provided inputs.
    /// </summary>
    /// <param name="text">The input string containing placeholders.</param>
    /// <param name="inputs">The list of VSCodeInput to use for replacement.</param>
    /// <returns>The string with all placeholders replaced.</returns>
    internal string ReplaceInputPlaceholders(string text)
    {
        if (string.IsNullOrEmpty(text) || this.Inputs.Count == 0)
            return text;

        return System.Text.RegularExpressions.Regex.Replace(
            text,
            @"\$\{input:([^}]+)\}",
            match => {
                var id = match.Groups[1].Value;
                var input = Inputs.Find(i => i.Id == id);
                if (input == null)
                    throw new ArgumentException($"No input found with id '{id}'", nameof(Inputs));
                return input.DefaultValue ?? string.Empty;
            });
    }
}

public class VSCodeServer : IMcpServer
{
    [JsonRequired]
    [JsonPropertyName("command")]
    public string Command { get; set; } = null!;

    [JsonPropertyName("args")]
    public List<string>? Args { get; set; }

    [JsonPropertyName("env")]
    public Dictionary<string, string>? Env { get; set; }

    [JsonPropertyName("envFile")]
    public string? EnvFilePath { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public VSCodeServerType? Type { get; set; } = VSCodeServerType.Stdio;

    /// <summary>
    /// TBD: VS Code does not provide an option to disable servers in the MCP configuration, but property might still be useful for in-code setting
    /// </summary>
    public bool Disabled { get; set; } = false;
}

public enum VSCodeServerType
{
    [EnumMember(Value = "stdio")]
    Stdio,
    [EnumMember(Value = "sse")]
    SSE
}

public enum VsCodeInstanceType
{
    [EnumMember(Value = "vscode")]
    VSCode,
    [EnumMember(Value = "vscode-insiders")]
    VSCodeInsiders
}

/// <summary>
/// User inputs. Used for defining user input prompts, such as free string input or a choice from several options.
/// </summary>
/// <remarks>
/// Visual Studio Code would show a prompt on initialization to ask for values. Instead we pass them in, as we run non-interactively
/// </remarks>
public class VSCodeInput
{
    /// <summary>
    /// The input's id is used to associate an input with a variable of the form ${input:id}.
    /// </summary>
    [JsonRequired]
    [JsonPropertyName("id")]
    public string Id { get; set; } = null!;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    public string Value { get; set; } = null!;

    [JsonPropertyName("default")]
    public string DefaultValue { get; set; }

    [JsonPropertyName("password")]
    public bool IsSecret { get; set; } = false;
}

