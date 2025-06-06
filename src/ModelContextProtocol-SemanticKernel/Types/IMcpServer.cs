
namespace ModelContextProtocol.SemanticKernel.Types;

public interface IMcpServer
{
    List<string>? Args { get; set; }
    string Command { get; set; }
    bool Disabled { get; set; }
    Dictionary<string, string>? Env { get; set; }
}