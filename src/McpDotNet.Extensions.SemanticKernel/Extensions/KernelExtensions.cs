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
    private static readonly ConcurrentDictionary<string, KernelPlugin> Map = new();

    /// <summary>
    /// Creates a Model Content Protocol plugin from a Stdio server that contains the specified MCP functions and adds it into the plugin collection.
    /// </summary>
    /// <param name="plugins">The plugin collection to which the new plugin should be added.</param>
    /// <param name="serverName">The MCP Server name</param>
    /// <param name="transportOptions">Additional transport-specific configuration.</param>
    /// <param name="loggerFactory">Optional <see cref="ILoggerFactory"/>.</param>
    /// <returns>A Microsoft.SemanticKernel.KernelPlugin containing the functions provided in functions.</returns>
    public static async Task<KernelPlugin> AddMcpFunctionsFromStdioServerAsync(this KernelPluginCollection plugins, string serverName, Dictionary<string, string> transportOptions, ILoggerFactory? loggerFactory = null)
    {
        Guard.NotNull(plugins);
        Guard.NotNullOrWhiteSpace(serverName);
        Guard.NotNull(transportOptions);

        if (Map.TryGetValue(serverName, out var kernelPlugin))
        {
            return kernelPlugin;
        }

        var mcpClient = await GetStdioClientAsync(serverName, transportOptions, loggerFactory).ConfigureAwait(false);
        var functions = await mcpClient.MapToFunctionsAsync().ConfigureAwait(false);

        var plugin = plugins.AddFromFunctions(serverName, functions);

        return Map[serverName] = plugin;
    }

    private static async Task<IMcpClient> GetStdioClientAsync(string serverName, Dictionary<string, string> transportOptions, ILoggerFactory? loggerFactory = null)
    {
        McpClientOptions options = new()
        {
            ClientInfo = new()
            {
                Name = $"{serverName} Client",
                Version = "1.0.0"
            }
        };

        var config = new McpServerConfig
        {
            Id = serverName.ToLowerInvariant(),
            Name = serverName,
            TransportType = TransportTypes.StdIo,
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