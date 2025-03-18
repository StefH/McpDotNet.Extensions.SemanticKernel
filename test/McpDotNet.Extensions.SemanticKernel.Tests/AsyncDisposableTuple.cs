using System.Diagnostics.CodeAnalysis;

namespace McpDotNet.Extensions.SemanticKernel.Tests;

[ExcludeFromCodeCoverage]
internal class AsyncDisposableTuple<T1, T2>(T1 item1, T2 item2) : Tuple<T1, T2>(item1, item2), IAsyncDisposable
{
    public async ValueTask DisposeAsync()
    {
        await TryDisposeAsync(Item1);
        await TryDisposeAsync(Item2);
    }

    public void Deconstruct(out T1 item1, out T2 item2)
    {
        item1 = Item1;
        item2 = Item2;
    }

    private static async Task TryDisposeAsync<T>(T item)
    {
        switch (item)
        {
            case IAsyncDisposable asyncDisposable:
                await asyncDisposable.DisposeAsync();
                break;

            case IDisposable disposable:
                disposable.Dispose();
                break;
        }
    }
}