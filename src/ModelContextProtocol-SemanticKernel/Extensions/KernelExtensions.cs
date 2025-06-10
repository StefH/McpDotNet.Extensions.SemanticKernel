// Copyright (c) Stef Heyenrath

using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using ModelContextProtocol.Client;
using ModelContextProtocol.SemanticKernel.Options;
using Stef.Validation;
using Stef.Validation.Options;

namespace ModelContextProtocol.SemanticKernel.Extensions;

/// <summary>
/// Extension methods for KernelPlugin
/// </summary>
public static partial class KernelExtensions
{
    private static readonly ConcurrentDictionary<string, KernelPlugin> StdioMap = new();
    private static readonly ConcurrentDictionary<string, KernelPlugin> SseMap = new();

    /// <summary>
    /// Creates a Model Content Protocol plugin from a Stdio server that contains the specified MCP functions and adds it into the plugin collection.
    /// </summary>
    /// <param name="plugins">The plugin collection to which the new plugin should be added.</param>
    /// <param name="serverName">The MCP Server name.</param>
    /// <param name="command">Command.</param>
    /// <param name="arguments">Arguments to pass to the server process (optional).</param>
    /// <param name="environmentVariables">Environment variables to set for the server process (optional).</param>
    /// <param name="loggerFactory">The optional <see cref="ILoggerFactory"/>.</param>
    /// <param name="cancellationToken">The optional <see cref="CancellationToken"/>.</param>
    /// <returns>A <see cref="KernelPluginCollection"/> containing the functions.</returns>
    public static Task<KernelPluginCollection> AddMcpFunctionsFromStdioServerAsync(
        this KernelPluginCollection plugins,
        string serverName,
        string command,
        IList<string>? arguments = null,
        IDictionary? environmentVariables = null,
        ILoggerFactory? loggerFactory = null,
        CancellationToken cancellationToken = default)
    {
        return AddMcpFunctionsFromStdioServerAsync(plugins, new ModelContextProtocolSemanticKernelStdioOptions
        {
            Name = serverName,
            Command = command,
            Arguments = arguments,
            EnvironmentVariables = environmentVariables,
            LoggerFactory = loggerFactory
        }, cancellationToken);
    }

    /// <summary>
    /// Creates a Model Content Protocol plugin from a Stdio server that contains the specified MCP functions and adds it into the plugin collection.
    /// </summary>
    /// <param name="plugins">The plugin collection to which the new plugin should be added.</param>
    /// <param name="optionsCallback">The <see cref="ModelContextProtocolSemanticKernelStdioOptions"/> callback.</param>
    /// <param name="cancellationToken">The optional <see cref="CancellationToken"/>.</param>
    /// <returns>A <see cref="KernelPluginCollection"/> containing the functions.</returns>
    public static Task<KernelPluginCollection> AddMcpFunctionsFromStdioServerAsync(
        this KernelPluginCollection plugins,
        Action<ModelContextProtocolSemanticKernelStdioOptions> optionsCallback,
        CancellationToken cancellationToken = default)
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
    /// <returns>A <see cref="KernelPluginCollection"/> containing the functions.</returns>
    public static async Task<KernelPluginCollection> AddMcpFunctionsFromStdioServerAsync(
        this KernelPluginCollection plugins,
        ModelContextProtocolSemanticKernelStdioOptions options,
        CancellationToken cancellationToken = default)
    {
        Guard.NotNull(plugins);
        Guard.NotNull(options);

        DataAnnotationOptionsValidator<ModelContextProtocolSemanticKernelStdioOptions>.ValidateAndThrow(options);

        var serverName = options.Name;
        var key = ToSafePluginName(serverName);

        if (StdioMap.TryGetValue(key, out var stdioKernelPlugin))
        {
            try
            {
                plugins.AddFromFunctions(key, stdioKernelPlugin.ToArray());
            }
            catch
            {
                // one or more functions have been added to plugins.
            }

            return plugins;
        }

        var mcpClient = await GetStdioClientAsync(serverName, options, cancellationToken).ConfigureAwait(false);
        var functions = await mcpClient.MapToFunctionsAsync(cancellationToken).ConfigureAwait(false);

        cancellationToken.Register(() =>
        {
            mcpClient.DisposeAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        });

        StdioMap[key] = plugins.AddFromFunctions(key, functions);
        return plugins;
    }

    /// <summary>
    /// Creates a Model Content Protocol plugin from an SSE server that contains the specified MCP functions and adds it into the plugin collection.
    /// </summary>
    /// <param name="plugins">The plugin collection to which the new plugin should be added.</param>
    /// <param name="serverName">The MCP Server name.</param>
    /// <param name="endpoint">The endpoint (location).</param>
    /// <param name="loggerFactory">The optional <see cref="ILoggerFactory"/>.</param>
    /// <param name="httpClient">The optional <see cref="HttpClient"/>.</param>
    /// <param name="cancellationToken">The optional <see cref="CancellationToken"/>.</param>
    /// <returns>A <see cref="KernelPluginCollection"/> containing the functions.</returns>
    public static Task<KernelPluginCollection> AddMcpFunctionsFromSseServerAsync(
        this KernelPluginCollection plugins,
        string serverName,
        string endpoint,
        ILoggerFactory? loggerFactory = null,
        HttpClient? httpClient = null,
        CancellationToken cancellationToken = default)
    {
        return AddMcpFunctionsFromSseServerAsync(plugins, serverName, new Uri(endpoint), loggerFactory, httpClient, cancellationToken);
    }

    /// <summary>
    /// Creates a Model Content Protocol plugin from an SSE server that contains the specified MCP functions and adds it into the plugin collection.
    /// </summary>
    /// <param name="plugins">The plugin collection to which the new plugin should be added.</param>
    /// <param name="serverName">The MCP Server name.</param>
    /// <param name="endpoint">The endpoint (location).</param>
    /// <param name="loggerFactory">The optional <see cref="ILoggerFactory"/>.</param>
    /// <param name="httpClient">The optional <see cref="HttpClient"/>.</param>
    /// <param name="cancellationToken">The optional <see cref="CancellationToken"/>.</param>
    /// <returns>A <see cref="KernelPluginCollection"/> containing the functions.</returns>
    public static Task<KernelPluginCollection> AddMcpFunctionsFromSseServerAsync(
        this KernelPluginCollection plugins,
        string serverName,
        Uri endpoint,
        ILoggerFactory? loggerFactory = null,
        HttpClient? httpClient = null,
        CancellationToken cancellationToken = default)
    {
        Guard.NotNull(plugins);
        Guard.NotNullOrWhiteSpace(serverName);
        Guard.NotNull(endpoint);

        return AddMcpFunctionsFromSseServerAsync(plugins, new ModelContextProtocolSemanticKernelSseOptions
        {
            Name = serverName,
            Endpoint = endpoint,
            LoggerFactory = loggerFactory,
        }, httpClient, cancellationToken);
    }

    /// <summary>
    /// Creates a Model Content Protocol plugin from an SSE server that contains the specified MCP functions and adds it into the plugin collection.
    /// </summary>
    /// <param name="plugins">The plugin collection to which the new plugin should be added.</param>
    /// <param name="optionsCallback">The <see cref="ModelContextProtocolSemanticKernelSseOptions"/> callback.</param>
    /// <param name="httpClient">The optional <see cref="HttpClient"/>.</param>
    /// <param name="cancellationToken">The optional <see cref="CancellationToken"/>.</param>
    /// <returns>A <see cref="KernelPluginCollection"/> containing the functions.</returns>
    public static Task<KernelPluginCollection> AddMcpFunctionsFromSseServerAsync(
        this KernelPluginCollection plugins,
        Action<ModelContextProtocolSemanticKernelSseOptions> optionsCallback,
        HttpClient? httpClient = null,
        CancellationToken cancellationToken = default)
    {
        Guard.NotNull(plugins);
        Guard.NotNull(optionsCallback);

        var options = new ModelContextProtocolSemanticKernelSseOptions();
        optionsCallback(options);

        return AddMcpFunctionsFromSseServerAsync(plugins, options, httpClient, cancellationToken);
    }

    /// <summary>
    /// Creates a Model Content Protocol plugin from an SSE server that contains the specified MCP functions and adds it into the plugin collection.
    /// </summary>
    /// <param name="plugins">The plugin collection to which the new plugin should be added.</param>
    /// <param name="options">The <see cref="ModelContextProtocolSemanticKernelSseOptions"/>.</param>
    /// <param name="httpClient">The optional <see cref="HttpClient"/>.</param>
    /// <param name="cancellationToken">The optional <see cref="CancellationToken"/>.</param>
    /// <returns>A <see cref="KernelPluginCollection"/> containing the functions.</returns>
    public static async Task<KernelPluginCollection> AddMcpFunctionsFromSseServerAsync(this KernelPluginCollection plugins, ModelContextProtocolSemanticKernelSseOptions options, HttpClient? httpClient = null, CancellationToken cancellationToken = default)
    {
        Guard.NotNull(plugins);
        Guard.NotNull(options);

        DataAnnotationOptionsValidator<ModelContextProtocolSemanticKernelSseOptions>.ValidateAndThrow(options);

        var serverName = options.Name;
        var key = ToSafePluginName(serverName);

        if (SseMap.TryGetValue(key, out var sseKernelPlugin))
        {
            try
            {
                plugins.AddFromFunctions(key, sseKernelPlugin.ToArray());
            }
            catch
            {
                // one or more functions have been added to plugins.
            }

            return plugins;
        }

        var mcpClient = await GetSseClientAsync(serverName, options, httpClient, cancellationToken).ConfigureAwait(false);
        var functions = await mcpClient.MapToFunctionsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        cancellationToken.Register(() => mcpClient.DisposeAsync().ConfigureAwait(false).GetAwaiter().GetResult());

        SseMap[key] = plugins.AddFromFunctions(key, functions);
        return plugins;
    }

    private static async Task<IMcpClient> GetStdioClientAsync(string serverName, ModelContextProtocolSemanticKernelStdioOptions options, CancellationToken cancellationToken)
    {
        var clientOptions = GetMcpClientOptions(serverName, "stdio");

        var loggerFactory = options.LoggerFactory ?? NullLoggerFactory.Instance;

        var environmentVariables = options.EnvironmentVariables switch
        {
            Dictionary<string, string> genericDictionary => genericDictionary,
            { } dictionary => dictionary.ToStringStringDictionary(),
            _ => null
        };

        var stdioOptions = new StdioClientTransportOptions
        {
            Command = options.Command,
            Arguments = options.Arguments,
            EnvironmentVariables = environmentVariables?.ToStringNullableStringDictionary(),
            Name = clientOptions.ClientInfo?.Name
        };
        var clientTransport = new StdioClientTransport(stdioOptions, loggerFactory);

        return await McpClientFactory.CreateAsync(clientTransport, clientOptions, loggerFactory: loggerFactory, cancellationToken: cancellationToken);
    }

    private static async Task<IMcpClient> GetSseClientAsync(string serverName, ModelContextProtocolSemanticKernelSseOptions options, HttpClient? httpClient, CancellationToken cancellationToken)
    {
        var clientOptions = GetMcpClientOptions(serverName, "sse");

        var loggerFactory = options.LoggerFactory ?? NullLoggerFactory.Instance;

        var sseOptions = new SseClientTransportOptions
        {
            Endpoint = options.Endpoint,
            AdditionalHeaders = options.AdditionalHeaders,
            Name = clientOptions.ClientInfo?.Name
        };

        var clientTransport = httpClient == null ? new SseClientTransport(sseOptions, loggerFactory) : new SseClientTransport(sseOptions, httpClient, loggerFactory);

        return await McpClientFactory.CreateAsync(clientTransport, clientOptions, loggerFactory: loggerFactory, cancellationToken: cancellationToken);
    }

    // A plugin name can contain only ASCII letters, digits, and underscores.
    private static string ToSafePluginName(string serverName)
    {
        return Regex.Replace(serverName, @"[^\w]", "_");
    }

    private static McpClientOptions GetMcpClientOptions(string serverName, string type)
    {
        var assembly = Assembly.GetEntryAssembly();
        var name = assembly?.GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? $"ModelContextProtocol-SemanticKernel.{type}Client";
        var version = assembly?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion.Split('+')[0] ?? "1.0.0";

        return new McpClientOptions
        {
            ClientInfo = new()
            {
                Name = $"{serverName} {name}",
                Version = version
            }
        };
    }
}