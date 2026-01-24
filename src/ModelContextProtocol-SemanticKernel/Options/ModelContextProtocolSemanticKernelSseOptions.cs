using System.ComponentModel.DataAnnotations;

namespace ModelContextProtocol.SemanticKernel.Options;

public class ModelContextProtocolSemanticKernelSseOptions : ModelContextProtocolSemanticKernelOptions
{
    /// <summary>
    /// The base URL of the server.
    /// </summary>
    [Required]
    public Uri Endpoint { get; set; } = null!;

    /// <summary>
    /// Additional headers.
    /// </summary>
    public Dictionary<string, string>? AdditionalHeaders { get; set; }
}