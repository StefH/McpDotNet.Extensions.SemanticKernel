using System.Runtime.Serialization;

namespace ModelContextProtocol.SemanticKernel.Types;

public enum VSCodeServerType
{
    [EnumMember(Value = "stdio")]
    Stdio,
    [EnumMember(Value = "sse")]
    SSE
}