// Copyright (c) Stef Heyenrath

using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
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
	public static async Task<IReadOnlyList<KernelPlugin>> AddToolsFromClaudeDesktopConfigAsync(
		this KernelPluginCollection plugins,
		ILoggerFactory? loggerFactory = null,
		CancellationToken cancellationToken = default)
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
				options.Command = kvp.Value.Command;
				options.Arguments = kvp.Value.Args;
				options.EnvironmentVariables = kvp.Value.Env;
			}, cancellationToken));
		}

		return registeredPlugins;
	}

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
	/// <returns>A <see cref="KernelPlugin"/> containing the functions.</returns>
	public static Task<KernelPlugin> AddMcpFunctionsFromStdioServerAsync(
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
	/// <returns>A <see cref="KernelPlugin"/> containing the functions.</returns>
	public static Task<KernelPlugin> AddMcpFunctionsFromStdioServerAsync(
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
	/// <returns>A <see cref="KernelPlugin"/> containing the functions.</returns>
	public static async Task<KernelPlugin> AddMcpFunctionsFromStdioServerAsync(
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

			return stdioKernelPlugin;
		}

		var mcpClient = await GetStdioClientAsync(serverName, options, cancellationToken).ConfigureAwait(false);
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
	/// <param name="clientOptions">The <see cref="McpClientOptions"/>.</param>
	/// <param name="sseOptions">The <see cref="SseClientTransportOptions"/>.</param>
	/// <param name="httpClient">The <see cref="HttpClient"/>.</param>
	/// <returns>A <see cref="KernelPlugin"/> containing the functions.</returns>
	public static Task<KernelPlugin> AddMcpFunctionsFromSseServerAsync(
		this KernelPluginCollection plugins,
		string serverName,
		string endpoint,
		ILoggerFactory? loggerFactory = null,
		CancellationToken cancellationToken = default,
		McpClientOptions? clientOptions = default,
		SseClientTransportOptions? sseOptions = default,
		HttpClient? httpClient = default)
	{
		return AddMcpFunctionsFromSseServerAsync(plugins, serverName, new Uri(endpoint), loggerFactory, cancellationToken, clientOptions, sseOptions, httpClient);
	}

	/// <summary>
	/// Creates a Model Content Protocol plugin from an SSE server that contains the specified MCP functions and adds it into the plugin collection.
	/// </summary>
	/// <param name="plugins">The plugin collection to which the new plugin should be added.</param>
	/// <param name="serverName">The MCP Server name.</param>
	/// <param name="endpoint">The endpoint (location).</param>
	/// <param name="loggerFactory">The optional <see cref="ILoggerFactory"/>.</param>
	/// <param name="cancellationToken">The optional <see cref="CancellationToken"/>.</param>
	/// <param name="clientOptions">The <see cref="McpClientOptions"/>.</param>
	/// <param name="sseOptions">The <see cref="SseClientTransportOptions"/>.</param>
	/// <param name="httpClient">The <see cref="HttpClient"/>.</param>
	/// <returns>A <see cref="KernelPlugin"/> containing the functions.</returns>
	public static Task<KernelPlugin> AddMcpFunctionsFromSseServerAsync(
		this KernelPluginCollection plugins,
		string serverName,
		Uri endpoint,
		ILoggerFactory? loggerFactory = null,
		CancellationToken cancellationToken = default,
		McpClientOptions? clientOptions = default,
		SseClientTransportOptions? sseOptions = default,
		HttpClient? httpClient = default)
	{
		Guard.NotNull(plugins);
		Guard.NotNullOrWhiteSpace(serverName);
		Guard.NotNull(endpoint);

		return AddMcpFunctionsFromSseServerAsync(plugins, new ModelContextProtocolSemanticKernelSseOptions
		{
			Name = serverName,
			Endpoint = endpoint,
			LoggerFactory = loggerFactory
		}, cancellationToken, clientOptions, sseOptions, httpClient);
	}

	/// <summary>
	/// Creates a Model Content Protocol plugin from an SSE server that contains the specified MCP functions and adds it into the plugin collection.
	/// </summary>
	/// <param name="plugins">The plugin collection to which the new plugin should be added.</param>
	/// <param name="optionsCallback">The <see cref="ModelContextProtocolSemanticKernelSseOptions"/> callback.</param>
	/// <param name="cancellationToken">The optional <see cref="CancellationToken"/>.</param>	
	/// <param name="clientOptions">The <see cref="McpClientOptions"/>.</param>
	/// <param name="sseOptions">The <see cref="SseClientTransportOptions"/>.</param>
	/// <param name="httpClient">The <see cref="HttpClient"/>.</param>
	/// <returns>A <see cref="KernelPlugin"/> containing the functions.</returns>
	public static Task<KernelPlugin> AddMcpFunctionsFromSseServerAsync(
		this KernelPluginCollection plugins,
		Action<ModelContextProtocolSemanticKernelSseOptions> optionsCallback,
		CancellationToken cancellationToken = default,
		McpClientOptions? clientOptions = default,
		SseClientTransportOptions? sseOptions = default,
		HttpClient? httpClient = default)
	{
		Guard.NotNull(plugins);
		Guard.NotNull(optionsCallback);

		var options = new ModelContextProtocolSemanticKernelSseOptions();
		optionsCallback(options);

		return AddMcpFunctionsFromSseServerAsync(plugins, options, cancellationToken, clientOptions, sseOptions, httpClient);
	}

	/// <summary>
	/// Creates a Model Content Protocol plugin from an SSE server that contains the specified MCP functions and adds it into the plugin collection.
	/// </summary>
	/// <param name="plugins">The plugin collection to which the new plugin should be added.</param>
	/// <param name="options">The <see cref="ModelContextProtocolSemanticKernelSseOptions"/>.</param>
	/// <param name="cancellationToken">The optional <see cref="CancellationToken"/>.</param>
	/// <param name="clientOptions">The <see cref="McpClientOptions"/>.</param>
	/// <param name="sseOptions">The <see cref="SseClientTransportOptions"/>.</param>
	/// <param name="httpClient">The <see cref="HttpClient"/>.</param>
	/// <returns>A <see cref="KernelPlugin"/> containing the functions.</returns>
	public static async Task<KernelPlugin> AddMcpFunctionsFromSseServerAsync(
		this KernelPluginCollection plugins,
		ModelContextProtocolSemanticKernelSseOptions options,
		CancellationToken cancellationToken = default,
		McpClientOptions? clientOptions = default,
		SseClientTransportOptions? sseOptions = default,
		HttpClient? httpClient = default)
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

			return sseKernelPlugin;
		}

		var mcpClient = await GetSseClientAsync(serverName, options, cancellationToken, clientOptions, sseOptions, httpClient).ConfigureAwait(false);
		var functions = await mcpClient.MapToFunctionsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

		cancellationToken.Register(() => mcpClient.DisposeAsync().ConfigureAwait(false).GetAwaiter().GetResult());

		sseKernelPlugin = plugins.AddFromFunctions(key, functions);
		return SseMap[key] = sseKernelPlugin;
	}

	/// <summary>
	/// Creates a <see cref="IMcpClient"/> instance
	/// </summary>
	/// <param name="serverName">The name of the server</param>
	/// <param name="options">The <see cref="ModelContextProtocolSemanticKernelSseOptions"/>.</param>
	/// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
	/// <param name="clientOptions">The <see cref="McpClientOptions"/>.</param>
	/// <param name="sseOptions">The <see cref="SseClientTransportOptions"/>.</param>
	/// <param name="httpClient">The <see cref="HttpClient"/>.</param>
	/// <returns>A <see cref="IMcpClient"/></returns>
	public static async Task<IMcpClient> GetSseClientAsync(string serverName, ModelContextProtocolSemanticKernelSseOptions options, CancellationToken cancellationToken, McpClientOptions? clientOptions = default, SseClientTransportOptions? sseOptions = default, HttpClient? httpClient = default)
	{
		clientOptions = clientOptions ?? GetMcpClientOptions(serverName, "sse");
		var clientTransport = GetSseClientTransport(clientOptions, options, sseOptions, httpClient, cancellationToken);
		return await GetSseClientAsync(serverName, clientOptions, options, clientTransport, cancellationToken);
	}

	/// <summary>
	/// Creates a <see cref="IMcpClient"/> instance
	/// </summary>
	/// <param name="serverName">The name of the server</param>
	/// <param name="clientOptions">The <see cref="McpClientOptions"/>.</param>
	/// <param name="options">The <see cref="ModelContextProtocolSemanticKernelSseOptions"/>.</param>
	/// <param name="sseOptions">The <see cref="SseClientTransportOptions"/>.</param>
	/// <param name="httpClient">The <see cref="HttpClient"/>.</param>
	/// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
	/// <returns>A <see cref="IMcpClient"/></returns>
	public static async Task<IMcpClient> GetSseClientAsync(string serverName, McpClientOptions clientOptions, ModelContextProtocolSemanticKernelSseOptions options, SseClientTransport clientTransport, CancellationToken cancellationToken)
	{
		var loggerFactory = options.LoggerFactory ?? NullLoggerFactory.Instance;
		return await McpClientFactory.CreateAsync(clientTransport, clientOptions, loggerFactory: loggerFactory, cancellationToken: cancellationToken);
	}

	/// <summary>
	/// Create a Sse Client Transport
	/// </summary>
	/// <param name="clientOptions">The <see cref="McpClientOptions"/>.</param>
	/// <param name="options">The <see cref="ModelContextProtocolSemanticKernelSseOptions"/>.</param>
	/// <param name="sseOptions">The <see cref="SseClientTransportOptions"/>.</param>
	/// <param name="httpClient">The <see cref="HttpClient"/>.</param>
	/// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
	/// <returns>A <see cref="SseClientTransport"/></returns>
	public static SseClientTransport GetSseClientTransport(McpClientOptions clientOptions, ModelContextProtocolSemanticKernelSseOptions options, SseClientTransportOptions? sseOptions, HttpClient? httpClient, CancellationToken cancellationToken)
	{
		var loggerFactory = options.LoggerFactory ?? NullLoggerFactory.Instance;

		sseOptions = sseOptions ?? new SseClientTransportOptions
		{
			Endpoint = options.Endpoint,
			AdditionalHeaders = options.AdditionalHeaders,
			Name = clientOptions.ClientInfo?.Name
		};

		if (httpClient is null)
			return new SseClientTransport(sseOptions, loggerFactory);

		return new SseClientTransport(sseOptions, httpClient, loggerFactory);
	}

	/// <summary>
	/// Generates MCP Client Options
	/// </summary>
	/// <param name="serverName">Name of the MCP server</param>
	/// <param name="type">Type of MCP server</param>
	/// <returns></returns>
	public static McpClientOptions GetMcpClientOptions(string serverName, string type)
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

	// A plugin name can contain only ASCII letters, digits, and underscores.
	private static string ToSafePluginName(string serverName)
	{
		return Regex.Replace(serverName, @"[^\w]", "_");
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
			EnvironmentVariables = environmentVariables,
			Name = clientOptions.ClientInfo?.Name
		};
		var clientTransport = new StdioClientTransport(stdioOptions, loggerFactory);

		return await McpClientFactory.CreateAsync(clientTransport, clientOptions, loggerFactory: loggerFactory, cancellationToken: cancellationToken);
	}
}