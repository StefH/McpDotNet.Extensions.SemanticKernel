using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace ModelContextProtocol.SemanticKernel.Types;

internal class VSCodeConfig : McpConfig<VSCodeServer>
{
    private static readonly Regex InputPlaceholderRegex = new(
        @"\$\{input:([^}]+)\}", RegexOptions.Compiled);

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
    /// <returns>The string with all placeholders replaced.</returns>
    internal string ReplaceInputPlaceholders(string text)
    {
        if (string.IsNullOrEmpty(text) || Inputs.Count == 0)
            return text;

        return InputPlaceholderRegex.Replace(
            text,
            match => {
                var id = match.Groups[1].Value;
                var input = Inputs.FirstOrDefault(i => i.Id == id);

                return input?.Value ?? // If input found, use value if set
                input?.DefaultValue ?? // if no value set, use default value
                text; // if neither is set - return original text, alternative would be throwing an exception 
            });
    }
}