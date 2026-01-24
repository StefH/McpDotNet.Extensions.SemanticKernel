namespace ModelContextProtocol.Schema.Dynamic;

internal readonly struct DynamicPropertyWithValue
{
    public readonly string Name { get; }

    public readonly object? Value { get; }

    public readonly Type Type { get; }

    public DynamicPropertyWithValue(string name, object? value = null, Type? type = null)
    {
        Name = name;
        Value = value;
        Type = type ?? value?.GetType() ?? typeof(object);
    }
}