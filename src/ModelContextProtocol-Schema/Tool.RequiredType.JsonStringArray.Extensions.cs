namespace ModelContextProtocol.Schema;

public readonly partial struct Tool
{
    public readonly partial struct RequiredType
    {
        public readonly partial struct JsonStringArray
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
                foreach (var element in this)
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