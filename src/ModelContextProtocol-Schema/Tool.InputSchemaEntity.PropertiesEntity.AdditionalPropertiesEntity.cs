// ReSharper disable once CheckNamespace
using System.Diagnostics.CodeAnalysis;
using Corvus.Json;

namespace ModelContextProtocol.Schema;

#if TOOL_EXTENSION_PUBLIC
public 
#else
internal 
#endif
readonly partial struct Tool
{
    public readonly partial struct InputSchemaEntity
    {
        public readonly partial struct PropertiesEntity
        {
            public readonly partial struct AdditionalPropertiesEntity
            {
                /// <summary>
                /// Gets the description from the JSON property named "description".
                /// </summary>
                public bool TryGetDescription([NotNullWhen(true)] out string? description)
                {
                    if (TryGetProperty("description", out var descriptionValue) &&
                        descriptionValue.AsString.TryGetString(out description))
                    {
                        return true;
                    }

                    description = null;
                    return false;
                }

                /// <summary>
                /// Gets the .NET type represented by the schema's "type" property, if available.
                /// </summary>
                /// <remarks>If the schema defines multiple types, such as ["integer", "null"],
                /// this property returns a nullable type when appropriate. For array types, the returned value reflects
                /// the type of the array elements if specified; otherwise, it defaults to a list of strings. Returns
                /// null if the type cannot be determined from the schema.</remarks>
                public bool TryGetType([NotNullWhen(true)] out Type? type)
                {
                    if (!TryGetProperty("type", out var typeValue))
                    {
                        type = null;
                        return false;
                    }

                    if (typeValue.AsArray.IsValid())
                    {
                        // Handle multiple types (e.g., ["integer","null"])
                        var types = typeValue.AsArray.EnumerateArray()
                            .Select(e => e.AsString.TryGetString(out var arrayItem) ? arrayItem : null)
                            .OfType<string>()
                            .ToArray();

                        if (types.Length == 0)
                        {
                            type = null;
                            return false;
                        }

                        if (types.Length == 1)
                        {
                            type = FromString(types[0]);
                            return true;
                        }

                        if (types.Length > 1 && types.Contains("null"))
                        {
                            var nonNullType = FromString(types.First(t => t != "null"));
                            type = typeof(Nullable<>).MakeGenericType(nonNullType);
                            return true;
                        }

                        type = FromString(types.First());
                        return true;
                    }

                    if (typeValue.AsString.TryGetString(out var typeAsString))
                    {
                        // Handle array type
                        if (typeAsString == "array")
                        {
                            if (TryGetProperty("items", out var itemsValue))
                            {
                                if (itemsValue.AsObject.TryGetProperty("type", out var itemTypeValue) &&
                                    itemTypeValue.AsString.TryGetString(out var itemTypeAsString))
                                {
                                    var itemType = FromString(itemTypeAsString);
                                    type = typeof(List<>).MakeGenericType(itemType);
                                    return true;
                                }
                            }

                            type = typeof(List<string>);
                            return true;
                        }

                        type = FromString(typeAsString);
                        return true;
                    }

                    if (typeValue.AsObject.IsValid())
                    {
                        type = typeof(Dictionary<string, object>);
                        return true;
                    }

                    type = null;
                    return false;
                }

                private static Type FromString(string typeString)
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
        }
    }
}