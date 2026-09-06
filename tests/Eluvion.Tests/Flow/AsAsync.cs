namespace Eluvion.Tests.Flow;

/// <summary>A synchronous sequence handed out asynchronously, for tests.</summary>
public sealed class AsAsync<T>(IEnumerable<T> origin) : IAsyncEnumerable<T>
{
    public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken ct = default)
    {
        foreach (var item in origin)
        {
            await Task.Yield();
            ct.ThrowIfCancellationRequested();
            yield return item;
        }
    }
}
