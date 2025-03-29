using ModelContextProtocol;
using ModelContextProtocolServer.OpenXml.Sse;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddMcpServer()
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