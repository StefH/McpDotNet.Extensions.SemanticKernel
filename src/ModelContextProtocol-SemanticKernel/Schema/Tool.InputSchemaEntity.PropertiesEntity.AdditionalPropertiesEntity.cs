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
            }
        }
    }
}