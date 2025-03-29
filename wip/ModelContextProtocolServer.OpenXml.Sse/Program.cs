using ModelContextProtocol;
using ModelContextProtocol.Protocol.Types;
using ModelContextProtocolServer.OpenXml.Sse;

const string applicationName = "mcpserver.openxml.sse";
const string version = "0.0.1";

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
    .WithToolsFromAssembly();

builder.Configuration
    .AddCommandLine(args)
    .AddEnvironmentVariables();

var app = builder.Build();

// app.MapGet("/", () => "Hello World!");
app.MapMcpSse();

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

await app.RunAsync(cts.Token);