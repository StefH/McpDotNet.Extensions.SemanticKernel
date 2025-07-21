using System.Text.Json;

namespace ModelContextProtocol.Schema;

public readonly partial struct Tool
{
    public readonly partial struct RequiredType
    {
        public readonly partial struct PropertiesEntity
        {
            public readonly partial struct AdditionalPropertiesEntity
            {
                /// <summary>
                /// Gets the description from the JSON property named "description".
                /// </summary>
                public string Description =>
                    TryGetProperty("description", out var descriptionValue) && descriptionValue.ValueKind == JsonValueKind.String && descriptionValue.AsString.TryGetString(out var description) ? description : string.Empty;
            }
        }
    }
}