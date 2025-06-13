using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;

namespace ModelContextProtocol.SemanticKernel.Types;

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
    public string Id { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Value will always be null from configuration, as VS Code would prompt user for it.
    /// </summary>
    /// <remarks>
    /// Pass List of Values to <see cref="KernelExtensions.AddToolsFromVsCodeConfigAsync(KernelPluginCollection, IEnumerable{VSCodeInput}, ILoggerFactory?, CancellationToken)"/>
    /// </remarks>
    public string? Value { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("default")]
    public string? DefaultValue { get; set; }

    [JsonPropertyName("password")]
    public bool IsSecret { get; set; } = false;
}