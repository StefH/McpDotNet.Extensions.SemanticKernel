using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol;
using ModelContextProtocol.Protocol.Types;

namespace ModelContextProtocolServer.Stdio;

public static class StdioServer
{
    public static Task RunAsync(string applicationName, string version, params string[] args)
    {
        var builder = Host.CreateEmptyApplicationBuilder(settings: new HostApplicationBuilderSettings
        {
            ApplicationName = applicationName,
            Args = args
        });

        builder.Configuration
            .AddCommandLine(args)
            .AddEnvironmentVariables();

        builder.Services
            .AddMcpServer(o => o.ServerInfo = new Implementation
            {
                Name = applicationName,
                Version = version
            })
            .WithStdioServerTransport()
            .WithToolsFromAssembly();

        var host = builder.Build();

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

        return host.RunAsync(cts.Token);
    }
}