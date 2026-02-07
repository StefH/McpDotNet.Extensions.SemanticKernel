using System.Collections;

namespace ModelContextProtocol.SemanticKernel.Extensions;

internal static class DictionaryExtensions
{
    /// <summary>
    /// Converts a non-generic dictionary to a generic dictionary with string keys and values.
    /// </summary>
    /// <param name="dictionary">The non-generic dictionary to convert.</param>
    /// <returns>A generic dictionary with string keys and values.</returns>
    internal static Dictionary<string, string?>? ToStringNullableStringDictionary(this IDictionary dictionary)
    {
        return dictionary switch
        {
            Dictionary<string, string?> genericStringNullableStringDictionary => genericStringNullableStringDictionary,
            not null => dictionary.Cast<DictionaryEntry>().Where(entry => entry.Key is string).ToDictionary(entry => (string)entry.Key, entry => entry.Value?.ToString()),
            _ => null
        };
    }
}