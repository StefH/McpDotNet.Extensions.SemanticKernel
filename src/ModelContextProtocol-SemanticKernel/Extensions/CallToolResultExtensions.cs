// ReSharper disable once CheckNamespace
namespace ModelContextProtocol.Protocol;

internal static class CallToolResultExtensions
{
    internal static string GetAllText(this CallToolResult result)
    {
        return string.Concat(result.Content.OfType<TextContentBlock>().Select(c => c.Text));
    }
}