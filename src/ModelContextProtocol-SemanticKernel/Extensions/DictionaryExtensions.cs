// Copyright (c) Stef Heyenrath
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
    public static IReadOnlyDictionary<TKey, TValue> AsReadOnly<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
    {
        return new System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>(dictionary);
    }
#endif
}