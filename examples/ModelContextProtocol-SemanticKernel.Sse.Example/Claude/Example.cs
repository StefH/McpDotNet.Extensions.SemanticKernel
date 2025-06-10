using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using ModelContextProtocol.SemanticKernel.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WireMock.Server;

namespace Examples.Claude;

internal static class Example
{
    internal static async Task RunAsync()
    {
        var builder = Kernel.CreateBuilder();
        builder.Services.AddLogging(c => c.AddDebug().SetMinimumLevel(LogLevel.Trace));

        builder.Services.AddOpenAIChatCompletion(
            serviceId: "openai",
            modelId: "gpt-4o-mini",
            apiKey: Environment.GetEnvironmentVariable("OPENAI_API_KEY")!);

        var kernel = builder.Build();

        await kernel.Plugins.AddToolsFromClaudeDesktopConfigAsync();
    }
}