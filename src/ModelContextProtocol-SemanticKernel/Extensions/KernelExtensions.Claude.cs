// Copyright (c) Stef Heyenrath

using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using ModelContextProtocol.SemanticKernel.Types;

namespace ModelContextProtocol.SemanticKernel.Extensions;

/// <summary>
/// Extension methods for KernelPlugin
/// </summary>
public static partial class KernelExtensions
{
    /// <summary>
    /// Adds Stdio Model Content Protocol plugins from a Claude Desktop configuration file (<c>claude_desktop_config.json</c>) and adds it into the plugin collection.
    /// </summary>
    /// <param name="plugins">The plugin collection to which the new plugin should be added.</param>
    /// <param name="loggerFactory">The optional <see cref="ILoggerFactory"/>.</param>
    /// <param name="cancellationToken">The optional <see cref="CancellationToken"/>.</param>
    /// <returns><see cref="KernelPluginCollection"/> containing the functions provided in plugins.</returns>
    public static async Task<KernelPluginCollection> AddToolsFromClaudeDesktopConfigAsync(
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

        foreach (var kvp in config.McpServers.Where(s => !s.Value.Disabled))
        {
            await AddMcpFunctionsFromStdioServerAsync(plugins, options =>
            {
                options.Name = kvp.Key;
                options.Command = kvp.Value.Command;
                options.Arguments = kvp.Value.Args;
                options.EnvironmentVariables = kvp.Value.Env;
            }, cancellationToken);
        }

        return plugins;
    }
}