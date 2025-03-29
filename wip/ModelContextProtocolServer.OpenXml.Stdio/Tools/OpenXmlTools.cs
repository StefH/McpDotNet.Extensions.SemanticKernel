using System.ComponentModel;
using Microsoft.Extensions.Configuration;
using ModelContextProtocol.Server;

namespace ModelContextProtocolServer.OpenXml.Stdio.Tools;

[McpServerToolType]
public static class OpenXmlTools
{
    [McpServerTool, Description("Read a .docx file and return the content as plain text.")]
    public static string ReadWordDocument(IConfiguration config, [Description("The name of the .docx file.")] string filename)
    {
        var allowedPath = config.GetValue<string>("allowedPath")!;
        var filePath = Path.Combine(allowedPath, filename);

        return WordReader.ReadWordDocument(filePath);
    }
}