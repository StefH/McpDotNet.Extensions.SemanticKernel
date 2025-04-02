// Copyright (c) Stef Heyenrath

using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol.Transport;
using ModelContextProtocol.SemanticKernel.Options;
using ModelContextProtocol.SemanticKernel.Types;
using Stef.Validation;
using Stef.Validation.Options;

namespace ModelContextProtocol.SemanticKernel.Extensions;

/// <summary>
/// Extension methods for KernelPlugin
/// </summary>
public static class KernelExtensions
{
    private static readonly ConcurrentDictionary<string, KernelPlugin> StdioMap = new();
    private static readonly ConcurrentDictionary<string, KernelPlugin> SseMap = new();

    /// <summary>
    /// Adds Stdio Model Content Protocol plugins from a Claude Desktop configuration file (<c>claude_desktop_config.json</c>) and adds it into the plugin collection.
    /// </summary>
    /// <param name="plugins">The plugin collection to which the new plugin should be added.</param>
    /// <param name="loggerFactory">The optional <see cref="ILoggerFactory"/>.</param>
    /// <param name="cancellationToken">The optional <see cref="CancellationToken"/>.</param>
    /// <returns>A list of <see cref="KernelPlugin"/> containing the functions provided in plugins.</returns>
    public static async Task<IReadOnlyList<KernelPlugin>> AddToolsFromClaudeDesktopConfigAsync(this KernelPluginCollection plugins, ILoggerFactory? loggerFactory = null, CancellationToken cancellationToken = default)
    {
        var appDataRoaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var configPath = Path.Combine(appDataRoaming, "Claude", "claude_desktop_config.json");
        if (!File.Exists(configPath))
        {
            return [];
        }

        var config = JsonSerializer.Deserialize<ClaudeConfig>(File.OpenRead(configPath));
        if (config == null)
        {
            return [];
        }

        var registeredPlugins = new List<KernelPlugin>();
        foreach (var kvp in config.McpServers.Where(s => !s.Value.Disabled))
        {
            registeredPlugins.Add(await AddMcpFunctionsFromStdioServerAsync(plugins, options =>
            {
                options.Name = kvp.Key;
                options.TransportOptions = kvp.Value.ToTransportOptions();
            }, cancellationToken));
        }

        return registeredPlugins;
    }

    /// <summary>
    /// Creates a Model Content Protocol plugin from a Stdio server that contains the specified MCP functions and adds it into the plugin collection.
    /// </summary>
    /// <param name="plugins">The plugin collection to which the new plugin should be added.</param>
    /// <param name="serverName">The MCP Server name.</param>
    /// <param name="transportOptions">Additional transport-specific configuration.</param>
    /// <param name="loggerFactory">The optional <see cref="ILoggerFactory"/>.</param>
    /// <param name="cancellationToken">The optional <see cref="CancellationToken"/>.</param>
    /// <returns>A <see cref="KernelPlugin"/> containing the functions.</returns>
    public static Task<KernelPlugin> AddMcpFunctionsFromStdioServerAsync(this KernelPluginCollection plugins, string serverName, Dictionary<string, string> transportOptions, ILoggerFactory? loggerFactory = null, CancellationToken cancellationToken = default)
    {
        return AddMcpFunctionsFromStdioServerAsync(plugins, new ModelContextProtocolSemanticKernelStdioOptions
        {
            Name = serverName,
            TransportOptions = transportOptions,
            LoggerFactory = loggerFactory
        }, cancellationToken);
    }

    /// <summary>
    /// Creates a Model Content Protocol plugin from a Stdio server that contains the specified MCP functions and adds it into the plugin collection.
    /// </summary>
    /// <param name="plugins">The plugin collection to which the new plugin should be added.</param>
    /// <param name="optionsCallback">The <see cref="ModelContextProtocolSemanticKernelStdioOptions"/> callback.</param>
    /// <param name="cancellationToken">The optional <see cref="CancellationToken"/>.</param>
    /// <returns>A <see cref="KernelPlugin"/> containing the functions.</returns>
    public static Task<KernelPlugin> AddMcpFunctionsFromStdioServerAsync(this KernelPluginCollection plugins, Action<ModelContextProtocolSemanticKernelStdioOptions> optionsCallback, CancellationToken cancellationToken = default)
    {
        Guard.NotNull(optionsCallback);

        var options = new ModelContextProtocolSemanticKernelStdioOptions();
        optionsCallback(options);

        return AddMcpFunctionsFromStdioServerAsync(plugins, options, cancellationToken);
    }

    /// <summary>
    /// Creates a Model Content Protocol plugin from a Stdio server that contains the specified MCP functions and adds it into the plugin collection.
    /// </summary>
    /// <param name="plugins">The plugin collection to which the new plugin should be added.</param>
    /// <param name="options">The <see cref="ModelContextProtocolSemanticKernelStdioOptions"/>.</param>
    /// <param name="cancellationToken">The optional <see cref="CancellationToken"/>.</param>
    /// <returns>A <see cref="KernelPlugin"/> containing the functions.</returns>
    public static async Task<KernelPlugin> AddMcpFunctionsFromStdioServerAsync(this KernelPluginCollection plugins, ModelContextProtocolSemanticKernelStdioOptions options, CancellationToken cancellationToken = default)
    {
        Guard.NotNull(plugins);
        Guard.NotNull(options);

        DataAnnotationOptionsValidator<ModelContextProtocolSemanticKernelStdioOptions>.ValidateAndThrow(options);

        var serverName = options.Name;
        var key = ToSafePluginName(serverName);

        if (StdioMap.TryGetValue(key, out var stdioKernelPlugin))
        {
            return stdioKernelPlugin;
        }

        var mcpClient = await GetClientAsync(serverName, null, options.TransportOptions, options.LoggerFactory, cancellationToken).ConfigureAwait(false);
        var functions = await mcpClient.MapToFunctionsAsync(cancellationToken).ConfigureAwait(false);

        cancellationToken.Register(() =>
        {
            mcpClient.DisposeAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        });

        stdioKernelPlugin = plugins.AddFromFunctions(key, functions);
        return StdioMap[key] = stdioKernelPlugin;
    }

    /// <summary>
    /// Creates a Model Content Protocol plugin from an SSE server that contains the specified MCP functions and adds it into the plugin collection.
    /// </summary>
    /// <param name="plugins">The plugin collection to which the new plugin should be added.</param>
    /// <param name="serverName">The MCP Server name.</param>
    /// <param name="endpoint">The endpoint (location).</param>
    /// <param name="loggerFactory">The optional <see cref="ILoggerFactory"/>.</param>
    /// <param name="cancellationToken">The optional <see cref="CancellationToken"/>.</param>
    /// <returns>A <see cref="KernelPlugin"/> containing the functions.</returns>
    public static Task<KernelPlugin> AddMcpFunctionsFromSseServerAsync(this KernelPluginCollection plugins, string serverName, string endpoint, ILoggerFactory? loggerFactory = null, CancellationToken cancellationToken = default)
    {
        Guard.NotNull(plugins);
        Guard.NotNullOrWhiteSpace(serverName);
        Guard.NotNullOrWhiteSpace(endpoint);

        return AddMcpFunctionsFromSseServerAsync(plugins, new ModelContextProtocolSemanticKernelSseOptions
        {
            Name = serverName,
            Endpoint = endpoint,
            LoggerFactory = loggerFactory
        }, cancellationToken);
    }

    /// <summary>
    /// Creates a Model Content Protocol plugin from an SSE server that contains the specified MCP functions and adds it into the plugin collection.
    /// </summary>
    /// <param name="plugins">The plugin collection to which the new plugin should be added.</param>
    /// <param name="optionsCallback">The <see cref="ModelContextProtocolSemanticKernelSseOptions"/> callback.</param>
    /// <param name="cancellationToken">The optional <see cref="CancellationToken"/>.</param>
    /// <returns>A <see cref="KernelPlugin"/> containing the functions.</returns>
    public static Task<KernelPlugin> AddMcpFunctionsFromSseServerAsync(this KernelPluginCollection plugins, Action<ModelContextProtocolSemanticKernelSseOptions> optionsCallback, CancellationToken cancellationToken = default)
    {
        Guard.NotNull(plugins);
        Guard.NotNull(optionsCallback);

        var options = new ModelContextProtocolSemanticKernelSseOptions();
        optionsCallback(options);

        return AddMcpFunctionsFromSseServerAsync(plugins, options, cancellationToken);
    }

    /// <summary>
    /// Creates a Model Content Protocol plugin from an SSE server that contains the specified MCP functions and adds it into the plugin collection.
    /// </summary>
    /// <param name="plugins">The plugin collection to which the new plugin should be added.</param>
    /// <param name="options">The <see cref="ModelContextProtocolSemanticKernelSseOptions"/>.</param>
    /// <param name="cancellationToken">The optional <see cref="CancellationToken"/>.</param>
    /// <returns>A <see cref="KernelPlugin"/> containing the functions.</returns>
    public static async Task<KernelPlugin> AddMcpFunctionsFromSseServerAsync(this KernelPluginCollection plugins, ModelContextProtocolSemanticKernelSseOptions options, CancellationToken cancellationToken = default)
    {
        Guard.NotNull(plugins);
        Guard.NotNull(options);

        DataAnnotationOptionsValidator<ModelContextProtocolSemanticKernelSseOptions>.ValidateAndThrow(options);

        var serverName = options.Name;
        var key = ToSafePluginName(serverName);

        if (SseMap.TryGetValue(key, out var sseKernelPlugin))
        {
            return sseKernelPlugin;
        }

        var mcpClient = await GetClientAsync(serverName, options.Endpoint, null, options.LoggerFactory, cancellationToken).ConfigureAwait(false);
        var functions = await mcpClient.MapToFunctionsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        cancellationToken.Register(() => mcpClient.DisposeAsync().ConfigureAwait(false).GetAwaiter().GetResult());

        sseKernelPlugin = plugins.AddFromFunctions(key, functions);
        return SseMap[key] = sseKernelPlugin;
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

    // A plugin name can contain only ASCII letters, digits, and underscores.
    private static string ToSafePluginName(string serverName)
    {
        return Regex.Replace(serverName, @"[^\w]", "_");
    }
}