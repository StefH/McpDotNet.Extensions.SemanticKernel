namespace ModelContextProtocol.Schema;

internal readonly partial struct Tool
{
    public readonly partial struct InputSchemaEntity
    {
        public readonly partial struct RequiredArray
        {
            public string[] ToStringArray()
            {
                var length = GetArrayLength();
                if (length <= 0)
                {
                    return [];
                }

                var index = 0;

                var stringValues = new string[length];
                foreach (var element in EnumerateArray())
                {
                    if (element.TryGetString(out var stringValue))
                    {
                        stringValues[index++] = stringValue;
                    }
                }

                if (index < length)
                {
                    Array.Resize(ref stringValues, index);
                }

                return stringValues;
            }
        }
    }
}