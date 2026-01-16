// ReSharper disable once CheckNamespace
using Corvus.Json;

namespace ModelContextProtocol.Schema;

internal readonly partial struct Tool
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
                public string Description =>
                    TryGetProperty("description", out var descriptionValue) && descriptionValue.AsString.TryGetString(out var description) ? description : string.Empty;

                public Type? Type
                {
                    get
                    {
                        if (!TryGetProperty("type", out var typeValue))
                        {
                            return null;
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
                                return null;
                            }

                            if (types.Length == 1)
                            {
                                return FromString(types[0]);
                            }

                            if (types.Length > 1 && types.Contains("null"))
                            {
                                var nonNullType = FromString(types.First(t => t != "null"));
                                return typeof(Nullable<>).MakeGenericType(nonNullType);
                            }

                            return FromString(types.First());
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
                                        return typeof(List<>).MakeGenericType(itemType);
                                    }
                                }

                                return typeof(List<string>);
                            }

                            return FromString(typeAsString);
                        }

                        return null;
                    }
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

/*
{{"description":"Filter on projects (names).","type":"array","items":{"type":"string"},"default":null}}

{{"description":"Number of team projects to return.","type":["integer","null"],"default":null}}
 * 
 * 
 * 
 * return typeString switch
        {
            "string" => typeof(string),
            "integer" => typeof(int),
            "number" => typeof(double),
            "boolean" => typeof(bool),
            "array" => typeof(List<string>),
            "object" => typeof(Dictionary<string, object>),
            _ => typeof(string)
        };
*/