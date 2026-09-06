namespace Eluvion.Flow;

/// <summary>A flow over the given async sequence.</summary>
public sealed class AsFlow<T>(Func<CancellationToken, IAsyncEnumerable<T>> spawn) : IFlow<T>
{
    /// <summary>A flow spawning the values of the given async sequence.</summary>
    public AsFlow(IAsyncEnumerable<T> source) : this(_ => source)
    { }

    public IAsyncEnumerable<T> Yield(CancellationToken ct = default) => spawn(ct);

    public IFlow<T> Trigger(ITrigger trigger) => new FlowLink<T>(this, trigger);

    public IFlow<T> Effect(IEffect<T> effect) => new FlowLink<T>(this, effect);

    public IFlow<TNewSpawn> Craft<TNewSpawn>(ICraft<T, TNewSpawn> craft) =>
        new CraftedFlow<T, TNewSpawn>(this, craft);
}
