// ReSharper disable once CheckNamespace
using System.Diagnostics.CodeAnalysis;
using Corvus.Json;
using ModelContextProtocol.Schema.Dynamic;

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
                            return TryGetFromString(types[0], out type);
                        }

                        if (types.Length > 1 && types.Contains("null") && TryGetFromString(types.First(t => t != "null"), out var nonNullType))
                        {
                            type = nonNullType.IsValueType ? typeof(Nullable<>).MakeGenericType(nonNullType) : nonNullType;
                            return true;
                        }

                        type = null;
                        return false;
                    }

                    if (typeValue.AsString.TryGetString(out var typeAsString))
                    {
                        return TryGetFromString(typeAsString, out type);
                    }

                    type = null;
                    return false;
                }

                private bool TryGetFromString(string typeAsString, [NotNullWhen(true)] out Type? type)
                {
                    if (typeAsString == "string")
                    {
                        type = typeof(string);
                        return true;
                    }

                    if (typeAsString == "integer")
                    {
                        type = typeof(int);
                        return true;
                    }

                    if (typeAsString == "number")
                    {
                        type = typeof(double);
                        return true;
                    }

                    if (typeAsString == "boolean")
                    {
                        type = typeof(bool);
                        return true;
                    }

                    if (typeAsString == "array")
                    {
                        if (TryGetProperty("items", out var itemsValue))
                        {
                            if (itemsValue.AsObject.TryGetProperty("type", out var itemTypeValue) &&
                                itemTypeValue.AsString.TryGetString(out var itemTypeAsString))
                            {
                                if (TryGetFromString(itemTypeAsString, out var itemType))
                                {
                                    type = typeof(List<>).MakeGenericType(itemType);
                                    return true;
                                }
                            }
                        }

                        type = typeof(List<string>);
                        return true;
                    }

                    if (typeAsString == "object")
                    {
                        if (TryGetProperty("properties", out var propertiesValue) && propertiesValue.AsObject.IsValid())
                        {
                            var dynamicProperties = new List<DynamicPropertyWithValue>();

                            foreach (var property in propertiesValue.AsObject.AsPropertyBacking())
                            {
                                var dynamicPropertyName = property.Name.GetString();
                                if (property.Value.AsObject.TryGetProperty("type", out var propertyType) &&
                                    propertyType.AsString.TryGetString(out var propertyAsString) &&
                                    TryGetFromString(propertyAsString, out var dynamicPropertyType))
                                {
                                    dynamicProperties.Add(new DynamicPropertyWithValue(dynamicPropertyName, type: dynamicPropertyType));
                                }
                            }

                            type = DynamicClassFactory.CreateType(dynamicProperties);
                            return true;
                        }

                        type = typeof(Dictionary<string, object>);
                        return true;
                    }

                    type = null;
                    return false;
                }
            }
        }
    }
}