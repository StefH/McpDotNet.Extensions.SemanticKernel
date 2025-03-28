using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using ModelContextProtocol.SemanticKernel.Extensions;

var cts = new CancellationTokenSource();

var builder = Kernel.CreateBuilder();
builder.Services.AddLogging(c => c.AddDebug().SetMinimumLevel(LogLevel.Trace));

builder.Services.AddOpenAIChatCompletion(
    serviceId: "openai",
    modelId: "gpt-4o-mini",
    apiKey: Environment.GetEnvironmentVariable("OPENAI_API_KEY")!);

var kernel = builder.Build();


var everyThingTransportOptions = new Dictionary<string, string>
{
    ["command"] = "npx",
    ["arguments"] = "-y @modelcontextprotocol/server-everything"
};
await kernel.Plugins.AddMcpFunctionsFromStdioServerAsync("Everything", everyThingTransportOptions, cancellationToken: cts.Token);

var githubTransportOptions = new Dictionary<string, string>
{
    ["command"] = "npx",
    ["arguments"] = "-y @modelcontextprotocol/server-github"
};
await kernel.Plugins.AddMcpFunctionsFromStdioServerAsync("GitHub", githubTransportOptions, cancellationToken: cts.Token);

var weatherTransportOptions = new Dictionary<string, string>
{
    ["command"] = @"C:\dev\GitHub\McpDotNet.Extensions.SemanticKernel\wip\ModelContextProtocolServer.OpenXml.Stdio\bin\Release\net8.0\ModelContextProtocolServer.OpenXml.Stdio.exe"
};
await kernel.Plugins.AddMcpFunctionsFromStdioServerAsync("Weather", weatherTransportOptions, cancellationToken: cts.Token);


var executionSettings = new OpenAIPromptExecutionSettings
{
    Temperature = 0,
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
};

var result = await kernel.InvokePromptAsync("Which tools are currently registered?", new(executionSettings)).ConfigureAwait(false);
Console.WriteLine($"\n\nTools:\n{result}");

//var prompt1 = "Please call the echo tool with the string 'Hello Stef!' and give me the response as-is.";
//var result1 = await kernel.InvokePromptAsync(prompt1, new(executionSettings)).ConfigureAwait(false);
//Console.WriteLine($"\n\n{prompt1}\n{result1}");

//var prompt2 = "Summarize the last 3 commits to the StefH/FluentBuilder repository.";
//var result2 = await kernel.InvokePromptAsync(prompt2, new(executionSettings)).ConfigureAwait(false);
//Console.WriteLine($"\n\n{prompt2}\n{result2}");

var prompt3 = "Give me all weather alerts for FL";
var result3 = await kernel.InvokePromptAsync(prompt3, new(executionSettings)).ConfigureAwait(false);
Console.WriteLine($"\n\n{prompt3}\n{result3}");

await cts.CancelAsync().ConfigureAwait(false);
cts.Dispose();