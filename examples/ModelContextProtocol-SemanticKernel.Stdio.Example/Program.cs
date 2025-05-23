using System.Collections;
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
    modelId: "gpt-4o",
    apiKey: Environment.GetEnvironmentVariable("OPENAI_API_KEY")!);

var kernel = builder.Build();

// await kernel.Plugins.AddToolsFromClaudeDesktopConfigAsync(cancellationToken: cts.Token);

//IDictionary every = new Dictionary<string, string> { { "e", "x" } };
//await kernel.Plugins.AddMcpFunctionsFromStdioServerAsync("Everything", "npx", ["-y", "@modelcontextprotocol/server-everything"], every, cancellationToken: cts.Token);

//await kernel.Plugins.AddMcpFunctionsFromStdioServerAsync("GitHub", "npx", ["-y", "@modelcontextprotocol/server-github"], cancellationToken: cts.Token);

await kernel.Plugins.AddMcpFunctionsFromStdioServerAsync(
    "AzureDevOpsStef",
    "dotnet run --project",
    [@"C:\dev\GitHub\mcpserver.azuredevops\src\mcpserver.azuredevops.stdio\mcpserver.azuredevops.stdio.csproj"],
    new Dictionary<string, string>
    {
        { "AZURE_DEVOPS_ORG_URL", "https://dev.azure.com/mstack" },
        { "AZURE_DEVOPS_AUTH_METHOD", "pat" },
        { "AZURE_DEVOPS_PAT", Environment.GetEnvironmentVariable("MCP_PAT")! }
    },
    cancellationToken: cts.Token);

// await kernel.Plugins.AddMcpFunctionsFromStdioServerAsync("OpenXML", "mcpserver.openxml.stdio", ["allowedPath=c:\\Temp"], cancellationToken: cts.Token);

//await kernel.Plugins.AddMcpFunctionsFromStdioServerAsync("gordon", "docker", ["ai", "mcpserver"], new Dictionary<string,string> { { "a", "b" } }, cancellationToken: cts.Token);

var markitdownArguments = new[]
{
    "run",

    // Automatically remove the container after it exits (no need to clean up manually).
    "--rm",

    // -i: Keep STDIN open (interactive mode)
    "-i",

    // Mounts the local host directory C:\temp into the container at /workdir.
    "-v",
    "C:\\\\temp:/workdir",

    // The name (and tag) of the Docker image to use.
    "sheyenrath/markitdown-mcp:latest"
};
//await kernel.Plugins.AddMcpFunctionsFromStdioServerAsync("MarkItDown-MCP", "docker", markitdownArguments, Environment.GetEnvironmentVariables(), cancellationToken: cts.Token);

var executionSettings = new OpenAIPromptExecutionSettings
{
    Temperature = 0.1,
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
};

//var result = await kernel.InvokePromptAsync("Which tools are currently registered?", new(executionSettings)).ConfigureAwait(false);
//Console.WriteLine($"\n\nTools:\n{result}");

var promptReadFile = "Convert the file '/workdir/CV.docx' to Markdown.";
var resultReadFile = await kernel.InvokePromptAsync(promptReadFile, new(executionSettings)).ConfigureAwait(false);
Console.WriteLine($"\n\n{promptReadFile}\n{resultReadFile}");

//var prompt1 = "Please call the echo tool with the string 'Hello Stef!' and give me the response as-is.";
//var result1 = await kernel.InvokePromptAsync(prompt1, new(executionSettings)).ConfigureAwait(false);
//Console.WriteLine($"\n\n{prompt1}\n{result1}");

//var prompt2 = "Summarize the last 3 commits to the StefH/FluentBuilder repository.";
//var result2 = await kernel.InvokePromptAsync(prompt2, new(executionSettings)).ConfigureAwait(false);
//Console.WriteLine($"\n\n{prompt2}\n{result2}");

//var promptAzureDevops = "For the Azure Devops project 'mstack-skills' and repository 'mstack-skills-blazor', get 2 latest commits with all details.";
//var resultAzureDevops = await kernel.InvokePromptAsync(promptAzureDevops, new(executionSettings)).ConfigureAwait(false);
//Console.WriteLine($"\n\n{promptAzureDevops}\n{resultAzureDevops}");

await cts.CancelAsync().ConfigureAwait(false);
cts.Dispose();