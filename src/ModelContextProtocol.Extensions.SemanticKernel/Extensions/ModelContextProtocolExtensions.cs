﻿// Copyright (c) Microsoft. All rights reserved.

using System.Text.Json;
using Microsoft.SemanticKernel;
using ModelContextProtocol.Client;
using ModelContextProtocol.Extensions.SemanticKernel.Types;
using ModelContextProtocol.Protocol.Types;

namespace ModelContextProtocol.Extensions.SemanticKernel.Extensions;

/// <summary>
/// Extension methods for ModelContextProtocol
/// </summary>
internal static class ModelContextProtocolExtensions
{
    /// <summary>
    /// Map the tools exposed on this <see cref="IMcpClient"/> to a collection of <see cref="KernelFunction"/> instances for use with the Semantic Kernel.
    /// </summary>
    internal static async Task<IReadOnlyList<KernelFunction>> MapToFunctionsAsync(this IMcpClient mcpClient)
    {
        var tools = new List<Tool>();
        await foreach (var item in mcpClient.ListToolsAsync())
        {
            tools.Add(item);
        }

        return tools.Select(t => t.ToKernelFunction(mcpClient)).ToList();
    }

    private static KernelFunction ToKernelFunction(this Tool tool, IMcpClient mcpClient)
    {
        async Task<string> InvokeToolAsync(Kernel kernel, KernelFunction function, KernelArguments arguments, CancellationToken cancellationToken)
        {
            try
            {
                // Convert arguments to dictionary format expected by ModelContextProtocol
                Dictionary<string, object> mcpArguments = [];
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
                    mcpArguments,
                    cancellationToken: cancellationToken
                ).ConfigureAwait(false);

                // Extract the text content from the result
                return string.Join("\n", result.Content
                    .Where(c => c.Type == "text")
                    .Select(c => c.Text));
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

    private static List<KernelParameterMetadata>? ToParameters(this Tool tool)
    {
        var inputSchema = JsonSerializer.Deserialize<JsonSchema>(tool.InputSchema.GetRawText());
        var properties = inputSchema?.Properties;
        if (properties == null)
        {
            return null;
        }

        HashSet<string> requiredProperties = [.. inputSchema!.Required ?? []];
        return properties.Select(kvp =>
            new KernelParameterMetadata(kvp.Key)
            {
                Description = kvp.Value.Description,
                ParameterType = ConvertParameterDataType(kvp.Value, requiredProperties.Contains(kvp.Key)),
                IsRequired = requiredProperties.Contains(kvp.Key)
            }).ToList();
    }

    private static KernelReturnParameterMetadata ToReturnParameter()
    {
        return new KernelReturnParameterMetadata
        {
            ParameterType = typeof(string),
        };
    }

    private static Type ConvertParameterDataType(JsonSchemaProperty property, bool required)
    {
        var type = property.Type switch
        {
            "string" => typeof(string),
            "integer" => typeof(int),
            "number" => typeof(double),
            "boolean" => typeof(bool),
            "array" => typeof(List<string>),
            "object" => typeof(Dictionary<string, object>),
            _ => typeof(object)
        };

        return !required && type.IsValueType ? typeof(Nullable<>).MakeGenericType(type) : type;
    }
}