using System.ComponentModel;
using ModelContextProtocol.Server;

namespace ModelContextProtocolServer.OpenXml.Sse.Tools;

[McpServerToolType]
public static class EchoTool
{
    [McpServerTool, Description("Echoes the input back to the client.")]
    public static string Echo(string message)
    {
        return "hello " + message;
    }
}
