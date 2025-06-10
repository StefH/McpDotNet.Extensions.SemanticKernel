// Copyright (c) Stef Heyenrath

using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using ModelContextProtocol.SemanticKernel.Types;
using System.Text.Json;

namespace ModelContextProtocol.SemanticKernel.Extensions;

/// <summary>
/// Extension methods for KernelPlugin
/// </summary>
public static partial class KernelExtensions
{
    /// <summary>
    /// Adds Model Content Protocol plugins from the Visual Studio settings file (<c>settings.json</c>) and adds it into the plugin collection.
    /// </summary>
    /// <param name="plugins">The plugin collection to which the new plugin should be added.</param>
    /// <param name="path">Path to the workspace settings file, for user settings use alternative overload</param>
    /// <param name="inputs">Provide non-interactive values for inputs, VS Code would prompt user for it</param>
    /// <param name="loggerFactory">The optional <see cref="ILoggerFactory"/>.</param>
    /// <param name="cancellationToken">The optional <see cref="CancellationToken"/>.</param>
    /// <returns>A list of <see cref="KernelPlugin"/> containing the functions provided in plugins.</returns>
    public static async Task<KernelPluginCollection> AddToolsFromVsCodeConfigAsync(
        this KernelPluginCollection plugins,
        string path,
        IEnumerable<VSCodeInput> inputs = null,
        ILoggerFactory? loggerFactory = null,
        CancellationToken cancellationToken = default)
    {
        var config = JsonSerializer.Deserialize<VsCodeSettings>(File.OpenRead(path));
        return await ProcessVsCodeConfiguration(plugins, config?.McpSection, cancellationToken);
    }

    /// <summary>
    /// Adds Model Content Protocol plugins from the Visual Studio settings file (<c>settings.json</c>) and adds it into the plugin collection.
    /// </summary>
    /// <param name="plugins">The plugin collection to which the new plugin should be added.</param>
    /// <param name="instanceType">In case of VSCode Insiders, default can be overriden to auto-detect settings location</param>
    /// <param name="inputs">Provide non-interactive values for inputs, VS Code would prompt user for it</param>
    /// <param name="loggerFactory">The optional <see cref="ILoggerFactory"/>.</param>
    /// <param name="cancellationToken">The optional <see cref="CancellationToken"/>.</param>
    /// <returns>A list of <see cref="KernelPlugin"/> containing the functions provided in plugins.</returns>
    public static async Task<KernelPluginCollection> AddToolsFromVsCodeConfigAsync(
        this KernelPluginCollection plugins,
        VsCodeInstanceType instanceType = VsCodeInstanceType.VisualStudioCode,
        IEnumerable<VSCodeInput> inputs = null,
        ILoggerFactory? loggerFactory = null,
        CancellationToken cancellationToken = default)
    {
        // https://code.visualstudio.com/docs/configure/settings#_user-settingsjson-location
        var instancePath = instanceType switch
        {
            VsCodeInstanceType.VisualStudioCode => "Code",
            VsCodeInstanceType.VisualStudioCodeInsiders => "Code - Insiders", // no docs for this, but it's the name on local machine (windows, including white spaces)
            _ => throw new ArgumentException("Visual Studio Instance Type not supported yet.", nameof(instanceType))
        };

        var appDataRoaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var configPath = Path.Combine(appDataRoaming, instancePath, "User", "settings.json");
        if (!File.Exists(configPath))
        {
            // TODO: Add logging that configuration file was not found
            return [];
        }

        var config = JsonSerializer.Deserialize<VsCodeSettings>(File.OpenRead(configPath));
        return await ProcessVsCodeConfiguration(plugins, config?.McpSection, cancellationToken);
    }

    internal static async Task<KernelPluginCollection> ProcessVsCodeConfiguration(KernelPluginCollection plugins, VSCodeConfig configuration, CancellationToken cancellationToken = default)
    {
        // TODO: Ensure input variables are present
        if(configuration == null || configuration.McpServers == null)
        {
            // if no configuration or settings within the configuration is present, conclude silently
            return plugins;
        }

        foreach (var kvp in configuration.McpServers.Where(s => !s.Value.Disabled))
        {
            if (kvp.Value.Type == VSCodeServerType.Stdio)
            {
                await AddMcpFunctionsFromStdioServerAsync(plugins, options =>
                {
                    options.Name = kvp.Key;
                    options.Command = kvp.Value.Command;
                    options.Arguments = kvp.Value.Args;
 

                    options.EnvironmentVariables = new Dictionary<string, string?>();

                    // Flatten EnvFilePath into EnvironmentVariables first
                    if (kvp.Value.EnvFilePath != null)
                    {
                        var variables = DotNetEnv.Env.Load(kvp.Value.EnvFilePath);
                        foreach (var item in variables)
                        {
                            options.EnvironmentVariables.Add(item.Key, configuration.ReplaceInputPlaceholders(item.Value));
                        }
                    }

                    // Override (or add) values if specified specifically as Env variable
                    foreach (var kv in kvp.Value.Env)
                        options.EnvironmentVariables[kv.Key] = configuration.ReplaceInputPlaceholders(kv.Value);
                },
                cancellationToken);
                continue;
            }
            // TODO: See if args can be safely extracted from args
            //else if (kvp.Value.Type == VSCodeServerType.SSE)
            //{

            //    continue;
            //}
            else
            {
                throw new NotSupportedException($"Server type {kvp.Value.Type} is not supported yet.");
            }
        }
        return plugins;
    }

}