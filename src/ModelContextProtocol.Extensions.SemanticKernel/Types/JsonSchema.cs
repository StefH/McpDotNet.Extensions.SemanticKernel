﻿// Copyright (c) https://github.com/PederHP/mcpdotnet

namespace ModelContextProtocol.Extensions.SemanticKernel.Types;

/// <summary>
/// Represents a JSON schema for a tool's input arguments.
/// <see href="https://github.com/modelcontextprotocol/specification/blob/main/schema/2024-11-05/schema.json">See the schema for details</see>
/// </summary>
internal class JsonSchema
{
    /// <summary>
    /// The type of the schema, should be "object".
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("type")]
    public string Type { get; set; } = "object";

    /// <summary>
    /// Map of property names to property definitions.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("properties")]
    public Dictionary<string, JsonSchemaProperty>? Properties { get; set; }

    /// <summary>
    /// List of required property names.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("required")]
    public List<string>? Required { get; set; }
}
