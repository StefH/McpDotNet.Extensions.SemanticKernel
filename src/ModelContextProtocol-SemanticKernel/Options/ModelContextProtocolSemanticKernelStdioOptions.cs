using System.ComponentModel.DataAnnotations;

namespace ModelContextProtocol.SemanticKernel.Options;

public class ModelContextProtocolSemanticKernelStdioOptions : ModelContextProtocolSemanticKernelOptions
{
    /// <summary>
    /// Additional transport-specific configuration.
    /// </summary>
    [Required]
    public Dictionary<string, string>? TransportOptions { get; init; }
}