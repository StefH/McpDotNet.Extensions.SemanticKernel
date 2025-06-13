using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using ModelContextProtocol.SemanticKernel.Extensions;
using ModelContextProtocol.SemanticKernel.Types;

namespace Examples.Vscode;

internal static class Example
{
    internal static async Task Run()
    {

        var builder = Kernel.CreateBuilder();
        builder.Services.AddLogging(c => c.AddDebug().SetMinimumLevel(LogLevel.Trace));

        builder.Services.AddOpenAIChatCompletion(
            serviceId: "openai",
            modelId: "gpt-4o-mini",
            apiKey: Environment.GetEnvironmentVariable("OPENAI_API_KEY")!);

        var kernel = builder.Build();


        var path = "<path to workspace configuration>";
        var inputs = new List<VSCodeInput>
        {
            new VSCodeInput { Id = "githubToken", Value = Environment.GetEnvironmentVariable("GITHUB_PAT"), IsSecret = true },
            new VSCodeInput { Id = "githubOwner", Value = "StefH" },
            new VSCodeInput { Id = "githubRepo", DefaultValue = "McpDotNet.Extensions.SemanticKernel" },
        };

        // Option 1: Load from VS Code settings (Workspace) file manually
        await kernel.Plugins.AddToolsFromVsCodeConfigAsync(path, inputs);

        // Option 2: Load from VS Code settings (User) file automatically
        await kernel.Plugins.AddToolsFromVsCodeConfigAsync(VsCodeInstanceType.VisualStudioCode, inputs);
        await kernel.Plugins.AddToolsFromVsCodeConfigAsync(VsCodeInstanceType.VisualStudioCodeInsiders, inputs);
    }
}
