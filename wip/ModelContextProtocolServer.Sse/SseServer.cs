using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol;
using ModelContextProtocol.Protocol.Types;

namespace ModelContextProtocolServer.Sse;

public static class SseServer
{
    public static Task RunAsync(params string[] args)
    {
        return RunAsync("/sse", args);
    }

    public static Task RunAsync(string sseEndpoint, params string[] args)
    {
        var assembly = Assembly.GetEntryAssembly();
        var applicationName = assembly?.GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? $"mcpserver.{Guid.NewGuid()}.sse";
        var version = assembly?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion.Split('+')[0] ?? "1.0.0";

        return RunAsync(applicationName, version, sseEndpoint, args);
    }

    public static Task RunAsync(string applicationName, string version, string sseEndpoint, params string[] args)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ApplicationName = applicationName,
            Args = args
        });

        builder.Services
            .AddMcpServer(o => o.ServerInfo = new Implementation
            {
                Name = applicationName,
                Version = version
            })
            .WithToolsFromAssembly(Assembly.GetEntryAssembly());

        builder.Configuration
            .AddCommandLine(args)
            .AddEnvironmentVariables();

        var app = builder.Build();
        app.MapMcpSse(sseEndpoint);

        var cts = new CancellationTokenSource();

        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
        {
            cts.Cancel();
        };

        return app.RunAsync(cts.Token);
    }
}