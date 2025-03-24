using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using ModelContextProtocol.Client;
using ModelContextProtocol.Configuration;
using ModelContextProtocol.Protocol.Transport;
using Stef.Validation;

namespace ModelContextProtocol.SemanticKernel.Extensions;

/// <summary>
/// Extension methods for KernelPlugin
/// </summary>
public static class KernelExtensions
{
    private static readonly ConcurrentDictionary<string, KernelPlugin> StdioMap = new();
    private static readonly ConcurrentDictionary<string, KernelPlugin> SseMap = new();

    /// <summary>
    /// Creates a Model Content Protocol plugin from a Stdio server that contains the specified MCP functions and adds it into the plugin collection.
    /// </summary>
    /// <param name="plugins">The plugin collection to which the new plugin should be added.</param>
    /// <param name="serverName">The MCP Server name.</param>
    /// <param name="transportOptions">Additional transport-specific configuration.</param>
    /// <param name="loggerFactory">The optional <see cref="ILoggerFactory"/>.</param>
    /// <param name="cancellationToken">The optional <see cref="CancellationToken"/>.</param>
    /// <returns>A Microsoft.SemanticKernel.KernelPlugin containing the functions provided in functions.</returns>
    public static async Task<KernelPlugin> AddMcpFunctionsFromStdioServerAsync(this KernelPluginCollection plugins, string serverName, Dictionary<string, string> transportOptions, ILoggerFactory? loggerFactory = null, CancellationToken cancellationToken = default)
    {
        Guard.NotNull(plugins);
        Guard.NotNullOrWhiteSpace(serverName);
        Guard.NotNull(transportOptions);

        if (StdioMap.TryGetValue(serverName, out var stdioKernelPlugin))
        {
            return stdioKernelPlugin;
        }

        var mcpClient = await GetClientAsync(serverName, null, transportOptions, loggerFactory, cancellationToken).ConfigureAwait(false);
        var functions = await mcpClient.MapToFunctionsAsync(cancellationToken).ConfigureAwait(false);

        stdioKernelPlugin = plugins.AddFromFunctions(serverName, functions);
        return StdioMap[serverName] = stdioKernelPlugin;
    }

    /// <summary>
    /// Creates a Model Content Protocol plugin from an SSE server that contains the specified MCP functions and adds it into the plugin collection.
    /// </summary>
    /// <param name="plugins">The plugin collection to which the new plugin should be added.</param>
    /// <param name="serverName">The MCP Server name.</param>
    /// <param name="endpoint">The endpoint (location).</param>
    /// <param name="loggerFactory">The optional <see cref="ILoggerFactory"/>.</param>
    /// <param name="cancellationToken">The optional <see cref="CancellationToken"/>.</param>
    /// <returns>A Microsoft.SemanticKernel.KernelPlugin containing the functions provided in functions.</returns>
    public static async Task<KernelPlugin> AddMcpFunctionsFromSseServerAsync(this KernelPluginCollection plugins, string serverName, string endpoint, ILoggerFactory? loggerFactory = null, CancellationToken cancellationToken = default)
    {
        Guard.NotNull(plugins);
        Guard.NotNullOrWhiteSpace(serverName);
        Guard.NotNull(endpoint);

        if (SseMap.TryGetValue(serverName, out var sseKernelPlugin))
        {
            return sseKernelPlugin;
        }

        var mcpClient = await GetClientAsync(serverName, endpoint, null, loggerFactory, cancellationToken).ConfigureAwait(false);
        var functions = await mcpClient.MapToFunctionsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        sseKernelPlugin = plugins.AddFromFunctions(serverName, functions);
        return SseMap[serverName] = sseKernelPlugin;
    }

    private static async Task<IMcpClient> GetClientAsync(string serverName, string? endpoint, Dictionary<string, string>? transportOptions, ILoggerFactory? loggerFactory, CancellationToken cancellationToken)
    {
        var transportType = !string.IsNullOrEmpty(endpoint) ? TransportTypes.Sse : TransportTypes.StdIo;

        McpClientOptions options = new()
        {
            ClientInfo = new()
            {
                Name = $"{serverName} {transportType}Client",
                Version = "1.0.0"
            }
        };

        var config = new McpServerConfig
        {
            Id = serverName.ToLowerInvariant(),
            Name = serverName,
            Location = endpoint,
            TransportType = transportType,
            TransportOptions = transportOptions
        };

        return await McpClientFactory.CreateAsync(config, options, loggerFactory: loggerFactory ?? NullLoggerFactory.Instance, cancellationToken: cancellationToken);
    }
}