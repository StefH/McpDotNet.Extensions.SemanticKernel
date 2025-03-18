using System.Collections.Concurrent;
using McpDotNet.Client;
using McpDotNet.Configuration;
using McpDotNet.Protocol.Transport;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Stef.Validation;

namespace McpDotNet.Extensions.SemanticKernel.Extensions;

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
    /// <param name="loggerFactory">Optional <see cref="ILoggerFactory"/>.</param>
    /// <returns>A Microsoft.SemanticKernel.KernelPlugin containing the functions provided in functions.</returns>
    public static async Task<KernelPlugin> AddMcpFunctionsFromStdioServerAsync(this KernelPluginCollection plugins, string serverName, Dictionary<string, string> transportOptions, ILoggerFactory? loggerFactory = null)
    {
        Guard.NotNull(plugins);
        Guard.NotNullOrWhiteSpace(serverName);
        Guard.NotNull(transportOptions);

        if (StdioMap.TryGetValue(serverName, out var stdioKernelPlugin))
        {
            return stdioKernelPlugin;
        }

        var mcpClient = await GetClientAsync(serverName, null, transportOptions, loggerFactory).ConfigureAwait(false);
        var functions = await mcpClient.MapToFunctionsAsync().ConfigureAwait(false);

        stdioKernelPlugin = plugins.AddFromFunctions(serverName, functions);
        return StdioMap[serverName] = stdioKernelPlugin;
    }

    /// <summary>
    /// Creates a Model Content Protocol plugin from an SSE server that contains the specified MCP functions and adds it into the plugin collection.
    /// </summary>
    /// <param name="plugins">The plugin collection to which the new plugin should be added.</param>
    /// <param name="serverName">The MCP Server name.</param>
    /// <param name="endpoint">The endpoint (location).</param>
    /// <param name="loggerFactory">Optional <see cref="ILoggerFactory"/>.</param>
    /// <returns>A Microsoft.SemanticKernel.KernelPlugin containing the functions provided in functions.</returns>
    public static async Task<KernelPlugin> AddMcpFunctionsFromStdioServerAsync(this KernelPluginCollection plugins, string serverName, string endpoint, ILoggerFactory? loggerFactory = null)
    {
        Guard.NotNull(plugins);
        Guard.NotNullOrWhiteSpace(serverName);
        Guard.NotNull(endpoint);

        if (SseMap.TryGetValue(serverName, out var sseKernelPlugin))
        {
            return sseKernelPlugin;
        }

        var mcpClient = await GetClientAsync(serverName, endpoint, null, loggerFactory).ConfigureAwait(false);
        var functions = await mcpClient.MapToFunctionsAsync().ConfigureAwait(false);

        sseKernelPlugin = plugins.AddFromFunctions(serverName, functions);
        return SseMap[serverName] = sseKernelPlugin;
    }

    private static async Task<IMcpClient> GetClientAsync(string serverName, string? endpoint, Dictionary<string, string>? transportOptions, ILoggerFactory? loggerFactory = null)
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

        var factory = new McpClientFactory(
            [config],
            options,
            loggerFactory ?? NullLoggerFactory.Instance
        );

        return await factory.GetClientAsync(config.Id).ConfigureAwait(false);
    }

}