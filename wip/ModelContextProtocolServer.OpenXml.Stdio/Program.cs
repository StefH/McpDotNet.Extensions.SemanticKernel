using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol;
using ModelContextProtocol.Protocol.Types;

const string applicationName = "openxml-server";
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

builder.Services.AddSingleton(_ =>
{
    var client = new HttpClient { BaseAddress = new Uri("https://api.weather.gov") };
    client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("weather-tool", "1.0"));
    return client;
});

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