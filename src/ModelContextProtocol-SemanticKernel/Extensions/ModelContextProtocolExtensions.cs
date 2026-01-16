// Copyright (c) Microsoft.

using System.Text.Json;
using Corvus.Json;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using static ModelContextProtocol.Schema.Tool.InputSchemaEntity.PropertiesEntity;

namespace ModelContextProtocol.SemanticKernel.Extensions;

/// <summary>
/// Extension methods for ModelContextProtocol
/// </summary>
internal static class ModelContextProtocolExtensions
{
    /// <summary>
    /// Map the tools exposed on this <see cref="IMcpClient"/> to a collection of <see cref="KernelFunction"/> instances for use with the Semantic Kernel.
    /// <param name="mcpClient">The <see cref="IMcpClient"/>.</param>
    /// <param name="cancellationToken">The optional <see cref="CancellationToken"/>.</param>
    /// </summary>
    internal static async Task<IReadOnlyList<KernelFunction>> MapToFunctionsAsync(this McpClient mcpClient, CancellationToken cancellationToken = default)
    {
        var functions = new List<KernelFunction>();
        foreach (var tool in await mcpClient.ListToolsAsync(cancellationToken: cancellationToken).ConfigureAwait(false))
        {
            functions.Add(tool.ToKernelFunction(mcpClient));
        }

        return functions;
    }

    private static KernelFunction ToKernelFunction(this McpClientTool tool, McpClient mcpClient)
    {
        async Task<string> InvokeToolAsync(Kernel kernel, KernelFunction function, KernelArguments arguments, CancellationToken ct)
        {
            try
            {
                var toolArguments = new Dictionary<string, JsonElement>();
                foreach (var param in function.Metadata.Parameters)
                {
                    if (arguments.TryGetValue(param.Name, out var value))
                    {
                        toolArguments[param.Name] = function.ToArgumentValue(param.Name, value);
                    }
                    else
                    {
                        toolArguments[param.Name] = function.ToArgumentValue(param.Name, null);
                    }
                }

                // Call the tool through ModelContextProtocol
                var callToolRequest = new CallToolRequestParams
                {
                    Name = tool.Name,
                    Arguments = toolArguments
                };
                var result = await mcpClient.CallToolAsync(callToolRequest, ct).ConfigureAwait(false);

                // Extract the text content from the result
                return result.GetAllText();
            }
            catch (Exception ex)
            {
                await Console.Error.WriteLineAsync($"Error invoking tool '{tool.Name}': {ex.Message}");

                // Rethrowing to allow the kernel to handle the exception
                throw;
            }
        }

        return KernelFunctionFactory.CreateFromMethod(
            method: InvokeToolAsync,
            functionName: tool.Name,
            description: tool.Description,
            parameters: tool.ToParameters(),
            returnParameter: ToReturnParameter()
        );
    }

    private static JsonElement ToArgumentValue(this KernelFunction function, string name, object? value)
    {
        var parameterType = function.Metadata.Parameters.FirstOrDefault(p => p.Name == name)?.ParameterType ?? typeof(object);

        var jsonTypeInfo = McpJsonUtilities.DefaultOptions.GetTypeInfo(parameterType);
        return JsonSerializer.SerializeToElement(value, jsonTypeInfo);
    }

    private static IEnumerable<KernelParameterMetadata>? ToParameters(this McpClientTool tool)
    {
        var inputSchema = Schema.Tool.InputSchemaEntity.FromJson(tool.JsonSchema);
        var properties = inputSchema.Properties;
        if (properties == default)
        {
            yield break;
        }

        foreach (var property in properties)
        {
            var name = property.Key.GetString();

            var isRequired = inputSchema.Required.IsValid() && inputSchema.Required.Any(requiredPropertyName => requiredPropertyName.EqualsString(name));
            var metadata = new KernelParameterMetadata(name)
            {
                Description = property.Value.Description,
                IsRequired = isRequired,
                ParameterType = ConvertParameterDataType(property.Value, isRequired)
            };

            yield return metadata;
        }
    }

    private static KernelReturnParameterMetadata ToReturnParameter()
    {
        return new KernelReturnParameterMetadata
        {
            ParameterType = typeof(string)
        };
    }

    private static Type ConvertParameterDataType(AdditionalPropertiesEntity property, bool required)
    {
        var type = property.Type ?? typeof(object);

        //switch (property.ValueKind)
        //{
        //    case JsonValueKind.String:
        //        var typeString = property.GetString();
        //        type = FromString(typeString);
        //        break;

        //    case JsonValueKind.Array:
        //        var value = property.EnumerateArray()
        //            .Select(e => e.GetString())
        //            .FirstOrDefault(v => !string.Equals(v, "nullable", StringComparison.OrdinalIgnoreCase));
        //        type = FromString(value);
        //        break;

        //    default:
        //        type = typeof(string);
        //        break;
        //}

        if (type.IsGenericType)
        {
            return type;
        }

        return !required && type.IsValueType ? typeof(Nullable<>).MakeGenericType(type) : type;
    }

    private static Type FromString(string? typeString)
    {
        return typeString switch
        {
            "string" => typeof(string),
            "integer" => typeof(int),
            "number" => typeof(double),
            "boolean" => typeof(bool),
            "array" => typeof(List<string>),
            "object" => typeof(Dictionary<string, object>),
            _ => typeof(string)
        };
    }
}