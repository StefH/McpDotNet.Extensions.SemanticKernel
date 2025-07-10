namespace ModelContextProtocol.SemanticKernel.Types;

public abstract class McpConfig<ServerType>
    where ServerType : IMcpServer
{
    public abstract Dictionary<string, ServerType> McpServers { get; set; }
}