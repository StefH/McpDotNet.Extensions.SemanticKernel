using System.ComponentModel.DataAnnotations;
using ModelContextProtocol.Client;

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

    /// <summary>
    /// The transport mode to use for the connection.
    /// </summary>
    /// <remarks>
    /// The default value is <see cref="HttpTransportMode.Sse"/>, which means that the client uses only HTTP with SSE transport.</remarks>
    public HttpTransportMode TransportMode { get; set; } = HttpTransportMode.Sse;
}