using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace ModelContextProtocol.SemanticKernel.Options;

public class ModelContextProtocolSemanticKernelStdioOptions : ModelContextProtocolSemanticKernelOptions
{
    /// <summary>
    /// Command.
    /// </summary>
    [Required]
    public string Command { get; set; } = null!;

    /// <summary>
    /// Arguments to pass to the server process.
    /// </summary>
    public IList<string>? Arguments { get; set; }

    /// <summary>
    /// Environment variables to set for the server process.
    /// </summary>
    public IDictionary? EnvironmentVariables { get; set; }
}