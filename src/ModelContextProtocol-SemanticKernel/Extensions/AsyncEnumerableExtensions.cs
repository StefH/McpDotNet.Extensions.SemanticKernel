// Copyright (c) Stef Heyenrath.

namespace ModelContextProtocol.SemanticKernel.Extensions;

internal static class AsyncEnumerableExtensions
{
    internal static async Task<IReadOnlyList<T>> ToListAsync<T>(this IAsyncEnumerable<T> asyncEnumerable, CancellationToken cancellationToken = default)
    {
        var list = new List<T>();
        await foreach (var item in asyncEnumerable.WithCancellation(cancellationToken))
        {
            list.Add(item);
        }

        return list;
    }
}