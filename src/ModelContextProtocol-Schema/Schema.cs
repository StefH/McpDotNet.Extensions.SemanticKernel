using Corvus.Json;

namespace ModelContextProtocol.Schema;

internal static class SchemaConstants
{
    internal const string Version = "2025-06-18";
}

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/Annotations")]
public readonly partial struct Annotations { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/AudioContent")]
public readonly partial struct AudioContent { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/BaseMetadata")]
public readonly partial struct BaseMetadata { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/BlobResourceContents")]
public readonly partial struct BlobResourceContents { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/BooleanSchema")]
public readonly partial struct BooleanSchema { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/CallToolRequest")]
public readonly partial struct CallToolRequest { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/CallToolResult")]
public readonly partial struct CallToolResult { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/CancelledNotification")]
public readonly partial struct CancelledNotification { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/ClientCapabilities")]
public readonly partial struct ClientCapabilities { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/ClientNotification")]
public readonly partial struct ClientNotification { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/ClientRequest")]
public readonly partial struct ClientRequest { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/ClientResult")]
public readonly partial struct ClientResult { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/CompleteRequest")]
public readonly partial struct CompleteRequest { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/CompleteResult")]
public readonly partial struct CompleteResult { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/ContentBlock")]
public readonly partial struct ContentBlock { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/CreateMessageRequest")]
public readonly partial struct CreateMessageRequest { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/CreateMessageResult")]
public readonly partial struct CreateMessageResult { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/Cursor")]
public readonly partial struct Cursor { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/ElicitRequest")]
public readonly partial struct ElicitRequest { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/ElicitResult")]
public readonly partial struct ElicitResult { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/EmbeddedResource")]
public readonly partial struct EmbeddedResource { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/EmptyResult")]
public readonly partial struct EmptyResult { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/EnumSchema")]
public readonly partial struct EnumSchema { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/GetPromptRequest")]
public readonly partial struct GetPromptRequest { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/GetPromptResult")]
public readonly partial struct GetPromptResult { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/ImageContent")]
public readonly partial struct ImageContent { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/Implementation")]
public readonly partial struct Implementation { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/InitializeRequest")]
public readonly partial struct InitializeRequest { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/InitializeResult")]
public readonly partial struct InitializeResult { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/InitializedNotification")]
public readonly partial struct InitializedNotification { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/JSONRPCError")]
public readonly partial struct JSONRPCError { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/JSONRPCMessage")]
public readonly partial struct JSONRPCMessage { }



[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/JSONRPCNotification")]
public readonly partial struct JSONRPCNotification { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/JSONRPCRequest")]
public readonly partial struct JSONRPCRequest { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/JSONRPCResponse")]
public readonly partial struct JSONRPCResponse { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/ListPromptsRequest")]
public readonly partial struct ListPromptsRequest { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/ListPromptsResult")]
public readonly partial struct ListPromptsResult { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/ListResourceTemplatesRequest")]
public readonly partial struct ListResourceTemplatesRequest { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/ListResourceTemplatesResult")]
public readonly partial struct ListResourceTemplatesResult { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/ListResourcesRequest")]
public readonly partial struct ListResourcesRequest { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/ListResourcesResult")]
public readonly partial struct ListResourcesResult { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/ListRootsRequest")]
public readonly partial struct ListRootsRequest { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/ListRootsResult")]
public readonly partial struct ListRootsResult { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/ListToolsRequest")]
public readonly partial struct ListToolsRequest { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/ListToolsResult")]
public readonly partial struct ListToolsResult { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/LoggingLevel")]
public readonly partial struct LoggingLevel { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/LoggingMessageNotification")]
public readonly partial struct LoggingMessageNotification { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/ModelHint")]
public readonly partial struct ModelHint { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/ModelPreferences")]
public readonly partial struct ModelPreferences { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/Notification")]
public readonly partial struct Notification { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/NumberSchema")]
public readonly partial struct NumberSchema { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/PaginatedRequest")]
public readonly partial struct PaginatedRequest { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/PaginatedResult")]
public readonly partial struct PaginatedResult { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/PingRequest")]
public readonly partial struct PingRequest { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/PrimitiveSchemaDefinition")]
public readonly partial struct PrimitiveSchemaDefinition { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/ProgressNotification")]
public readonly partial struct ProgressNotification { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/ProgressToken")]
public readonly partial struct ProgressToken { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/Prompt")]
public readonly partial struct Prompt { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/PromptArgument")]
public readonly partial struct PromptArgument { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/PromptListChangedNotification")]
public readonly partial struct PromptListChangedNotification { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/PromptMessage")]
public readonly partial struct PromptMessage { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/PromptReference")]
public readonly partial struct PromptReference { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/ReadResourceRequest")]
public readonly partial struct ReadResourceRequest { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/ReadResourceResult")]
public readonly partial struct ReadResourceResult { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/Request")]
public readonly partial struct Request { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/RequestId")]
public readonly partial struct RequestId { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/Resource")]
public readonly partial struct Resource { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/ResourceContents")]
public readonly partial struct ResourceContents { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/ResourceLink")]
public readonly partial struct ResourceLink { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/ResourceListChangedNotification")]
public readonly partial struct ResourceListChangedNotification { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/ResourceTemplate")]
public readonly partial struct ResourceTemplate { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/ResourceTemplateReference")]
public readonly partial struct ResourceTemplateReference { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/ResourceUpdatedNotification")]
public readonly partial struct ResourceUpdatedNotification { }

// 1>CSC : warning CS8785: Generator 'IncrementalSourceGenerator' failed to generate source. It will not contribute to the output and compilation errors may occur as a result.
// Exception was of type 'ArgumentException' with message 'An item with the same key has already been added.
// Key: C:/dev/GitHub/McpDotNet.Extensions.SemanticKernel/src/ModelContextProtocol-Schema/2025-06-18.json#/definitions/Result'.
// ---
// [JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/Result")]
// public readonly partial struct Result { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/Role")]
public readonly partial struct Role { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/Root")]
public readonly partial struct Root { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/RootsListChangedNotification")]
public readonly partial struct RootsListChangedNotification { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/SamplingMessage")]
public readonly partial struct SamplingMessage { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/ServerCapabilities")]
public readonly partial struct ServerCapabilities { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/ServerNotification")]
public readonly partial struct ServerNotification { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/ServerRequest")]
public readonly partial struct ServerRequest { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/ServerResult")]
public readonly partial struct ServerResult { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/SetLevelRequest")]
public readonly partial struct SetLevelRequest { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/StringSchema")]
public readonly partial struct StringSchema { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/SubscribeRequest")]
public readonly partial struct SubscribeRequest { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/TextContent")]
public readonly partial struct TextContent { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/TextResourceContents")]
public readonly partial struct TextResourceContents { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/Tool")]
public readonly partial struct Tool { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/ToolAnnotations")]
public readonly partial struct ToolAnnotations { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/ToolListChangedNotification")]
public readonly partial struct ToolListChangedNotification { }

[JsonSchemaTypeGenerator($"./{SchemaConstants.Version}.json#/definitions/UnsubscribeRequest")]
public readonly partial struct UnsubscribeRequest { }
