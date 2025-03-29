using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol;
using ModelContextProtocol.Protocol.Types;

const string applicationName = "filesystem-server";
const string version = "0.0.1";

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

await host.RunAsync(cts.Token);