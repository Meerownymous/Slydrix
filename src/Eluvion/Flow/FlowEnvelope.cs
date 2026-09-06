namespace Eluvion.Flow;

/// <summary>A flow delegating to the given flow.</summary>
public abstract class FlowEnvelope<T>(IFlow<T> origin) : IFlow<T>
{
    /// <summary>A flow delegating to the given async sequence.</summary>
    public FlowEnvelope(Func<CancellationToken, IAsyncEnumerable<T>> spawn) : this(new AsFlow<T>(spawn))
    { }

    public IAsyncEnumerable<T> Yield(CancellationToken ct = default) => origin.Yield(ct);

    public IFlow<T> Trigger(ITrigger trigger) => new FlowLink<T>(this, trigger);

    public IFlow<T> Effect(IEffect<T> effect) => new FlowLink<T>(this, effect);

    public IFlow<TNewSpawn> Craft<TNewSpawn>(ICraft<T, TNewSpawn> craft) =>
        new CraftedFlow<T, TNewSpawn>(this, craft);
}
