// Copyright (c) Stef Heyenrath

using System.Collections;

namespace ModelContextProtocol.SemanticKernel.Extensions;

internal static class DictionaryExtensions
{
#if NETSTANDARD2_0
    /// <summary>
    /// Converts a dictionary to a read-only dictionary.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    /// <param name="dictionary">The dictionary to convert.</param>
    /// <returns>A read-only dictionary containing the same key-value pairs as the input dictionary.</returns>
    internal static IReadOnlyDictionary<TKey, TValue> AsReadOnly<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
    {
        return new System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>(dictionary);
    }
#endif

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
            { } nonGenericDictionary => dictionary.Cast<DictionaryEntry>().Where(entry => entry.Key is string).ToDictionary(entry => (string)entry.Key, entry => entry.Value?.ToString()),
            _ => null
        };
    }
}