using System.ComponentModel;
using Microsoft.Extensions.Configuration;
using ModelContextProtocol.Server;

namespace ModelContextProtocolServer.FileSystem.Stdio;

[McpServerToolType]
public static class FileSystemTools
{
    [McpServerTool, Description("Read the complete contents of a file from the file system.")]
    public static async Task<string> ReadFile(IConfiguration config, [Description("The name of the file.")] string filename)
    {
        var path = config.GetValue<string>("allowedPath")!;
        //return await File.ReadAllTextAsync(Path.Combine(path, filename));
        
        var fileBytes = await File.ReadAllBytesAsync(Path.Combine(path, filename));
        return Convert.ToBase64String(fileBytes);
    }
}