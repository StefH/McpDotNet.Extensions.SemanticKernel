using System.ComponentModel;
using Microsoft.Extensions.Configuration;
using ModelContextProtocol.Server;
using ModelContextProtocolServer.OpenXml.Stdio.Services;

namespace ModelContextProtocolServer.OpenXml.Stdio.Tools;

[McpServerToolType]
public static class OpenXmlTools
{
    [McpServerTool, Description("Read a .docx file and return the content as plain text.")]
    public static string ReadWordDocument(IConfiguration config, string filename)
    {
        if (string.IsNullOrEmpty(filename))
        {
            return "Filename is null or empty";
        }

        var allowedPath = config.GetValue<string>("allowedPath")!;
        var filePath = Path.Combine(allowedPath, filename);

        return WordDocumentReader.GetTextFromWordDocument(filePath);
    }
}