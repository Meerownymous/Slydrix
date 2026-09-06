using System.Runtime.CompilerServices;

namespace Eluvion.Flow;

/// <summary>A flow whose every value has been transformed by a craft.</summary>
public sealed class CraftedFlow<TIn, TOut>(IFlow<TIn> origin, ICraft<TIn, TOut> craft) : IFlow<TOut>
{
    public async IAsyncEnumerable<TOut> Yield([EnumeratorCancellation] CancellationToken ct = default)
    {
        await foreach (var spawned in origin.Yield(ct))
            yield return await craft.Yield(spawned);
    }

    public IFlow<TOut> Trigger(ITrigger trigger) => new FlowLink<TOut>(this, trigger);

    public IFlow<TOut> Effect(IEffect<TOut> effect) => new FlowLink<TOut>(this, effect);

    public IFlow<TNewSpawn> Craft<TNewSpawn>(ICraft<TOut, TNewSpawn> next) =>
        new CraftedFlow<TOut, TNewSpawn>(this, next);
}
