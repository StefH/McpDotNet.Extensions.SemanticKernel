using System.Runtime.Serialization;

namespace ModelContextProtocol.SemanticKernel.Types;

public enum VsCodeInstanceType
{
    [EnumMember(Value = "vscode")]
    VisualStudioCode,
    [EnumMember(Value = "vscode-insiders")]
    VisualStudioCodeInsiders
}