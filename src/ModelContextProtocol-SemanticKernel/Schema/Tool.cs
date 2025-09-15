using Corvus.Json;

// ReSharper disable once CheckNamespace
namespace ModelContextProtocol.Schema;

internal static class SchemaConstants
{
    internal const string Version = "2025-06-18";
}

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/Tool")]
internal readonly partial struct Tool;