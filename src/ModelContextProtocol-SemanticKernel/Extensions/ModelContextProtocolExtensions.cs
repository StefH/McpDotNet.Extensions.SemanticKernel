using System.Text.Json;
using Corvus.Json;
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
                    else if (param.IsRequired)
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
        var nonNullableParameterType = parameterType.GetNonNullableType();

        object? changedValue = value;

        if (value is string valueAsAstring && nonNullableParameterType != typeof(string))
        {
            if (nonNullableParameterType == typeof(int) && int.TryParse(valueAsAstring, out var intValue))
            {
                changedValue = intValue;
            }
            else if (nonNullableParameterType == typeof(double) && double.TryParse(valueAsAstring, out var doubleValue))
            {
                changedValue = doubleValue;
            }
            else if (nonNullableParameterType == typeof(bool) && bool.TryParse(valueAsAstring, out var boolValue))
            {
                changedValue = boolValue;
            }
            else
            {
                try
                {
                    changedValue = JsonSerializer.Deserialize(valueAsAstring, nonNullableParameterType);
                }
                catch
                {
                    // Ignore deserialization errors and keep the original string value
                }
            }
        }

        var jsonTypeInfo = McpJsonUtilities.DefaultOptions.GetTypeInfo(parameterType);
        return JsonSerializer.SerializeToElement(changedValue, jsonTypeInfo);
    }

    private static bool IsNullableType(this Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
    }

    private static Type GetNonNullableType(this Type type)
    {
        return type.IsNullableType() ? type.GetGenericArguments()[0] : type;
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
                Description = property.Value.TryGetDescription(out var description) ? description : string.Empty,
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
        var propertyType = property.TryGetType(out var type) ? type : typeof(object);

        if (propertyType.IsGenericType)
        {
            return propertyType;
        }

        return !required && propertyType.IsValueType ? typeof(Nullable<>).MakeGenericType(propertyType) : propertyType;
    }
}