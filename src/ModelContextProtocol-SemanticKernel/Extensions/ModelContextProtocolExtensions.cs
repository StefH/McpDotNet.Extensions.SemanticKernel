// Copyright (c) Microsoft.

using System.Text.Json;
using Microsoft.SemanticKernel;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

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
    internal static async Task<IReadOnlyList<KernelFunction>> MapToFunctionsAsync(this IMcpClient mcpClient, CancellationToken cancellationToken = default)
    {
        var functions = new List<KernelFunction>();
        foreach (var tool in await mcpClient.ListToolsAsync(cancellationToken: cancellationToken).ConfigureAwait(false))
        {
            functions.Add(tool.ToKernelFunction(mcpClient));
        }

        return functions;
    }

    private static KernelFunction ToKernelFunction(this McpClientTool tool, IMcpClient mcpClient)
    {
        async Task<string> InvokeToolAsync(Kernel kernel, KernelFunction function, KernelArguments arguments, CancellationToken ct)
        {
            try
            {
                // Convert arguments to dictionary format expected by ModelContextProtocol
                Dictionary<string, object?> mcpArguments = [];
                foreach (var arg in arguments)
                {
                    if (arg.Value is not null)
                    {
                        mcpArguments[arg.Key] = function.ToArgumentValue(arg.Key, arg.Value);
                    }
                }

                // Call the tool through ModelContextProtocol
                var result = await mcpClient.CallToolAsync(
                    tool.Name,
                    mcpArguments.AsReadOnly(),
                    cancellationToken: ct
                ).ConfigureAwait(false);

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

    private static object ToArgumentValue(this KernelFunction function, string name, object value)
    {
        var parameterType = function.Metadata.Parameters.FirstOrDefault(p => p.Name == name)?.ParameterType;

        if (parameterType == null)
        {
            return value;
        }

        if (Nullable.GetUnderlyingType(parameterType) == typeof(int))
        {
            return Convert.ToInt32(value);
        }

        if (Nullable.GetUnderlyingType(parameterType) == typeof(double))
        {
            return Convert.ToDouble(value);
        }

        if (Nullable.GetUnderlyingType(parameterType) == typeof(bool))
        {
            return Convert.ToBoolean(value);
        }

        if (parameterType == typeof(List<string>))
        {
            return (value as IEnumerable<object>)?.ToList() ?? value;
        }

        if (parameterType == typeof(Dictionary<string, object>))
        {
            return (value as Dictionary<string, object>)?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) ?? value;
        }

        return value;
    }

    private static IEnumerable<KernelParameterMetadata>? ToParameters(this McpClientTool tool)
    {
        var inputSchema = Schema.Tool.InputSchemaEntity.FromJson(tool.JsonSchema);
        var properties = inputSchema.Properties;
        if (properties == default)
        {
            yield break;
        }

        var requiredProperties = inputSchema.Required.ToStringArray();
        foreach (var property in properties)
        {
            var isRequired = requiredProperties.Contains(property.Key.GetString());
            var metadata = new KernelParameterMetadata(property.Key.GetString())
            {
                Description = property.Value.Description,
                IsRequired = isRequired,
                ParameterType = ConvertParameterDataType(property.Value.AsJsonElement, isRequired)
            };

            yield return metadata;
        }
    }

    private static KernelReturnParameterMetadata ToReturnParameter()
    {
        return new KernelReturnParameterMetadata
        {
            ParameterType = typeof(string),
        };
    }

    private static Type ConvertParameterDataType(JsonElement property, bool required)
    {
        Type type;

        switch (property.ValueKind)
        {
            case JsonValueKind.String:
                var typeString = property.GetString();
                type = FromString(typeString);
                break;

            case JsonValueKind.Array:
                var value = property.EnumerateArray()
                    .Select(e => e.GetString())
                    .FirstOrDefault(v => !string.Equals(v, "nullable", StringComparison.OrdinalIgnoreCase));
                type = FromString(value);
                break;

            default:
                type = typeof(string);
                break;
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