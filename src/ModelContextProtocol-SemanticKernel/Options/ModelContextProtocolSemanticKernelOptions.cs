// Copyright (c) Stef Heyenrath

using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace ModelContextProtocol.SemanticKernel.Options;

public abstract class ModelContextProtocolSemanticKernelOptions
{
    /// <summary>
    /// Display name for the MCP server.
    /// </summary>
    [Required]
    public string Name { get; init; } = null!;

    /// <summary>
    /// A logger factory for creating loggers for clients.
    /// </summary>
    public ILoggerFactory? LoggerFactory { get; init; } = new NullLoggerFactory();
}