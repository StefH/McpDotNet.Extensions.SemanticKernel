// Copyright (c) Stef Heyenrath

using System.ComponentModel.DataAnnotations;

namespace ModelContextProtocol.SemanticKernel.Options;

public class ModelContextProtocolSemanticKernelSseOptions : ModelContextProtocolSemanticKernelOptions
{
    /// <summary>
    /// The base URL of the server.
    /// </summary>
    [Required]
    public string Endpoint { get; init; } = null!;
}