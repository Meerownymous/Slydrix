using System.Runtime.CompilerServices;

namespace Eluvion.Flow;

/// <summary>A flow chained with a side effect on every value it spawns.</summary>
public sealed class FlowLink<T>(IFlow<T> origin, Func<T, Task> act) : IFlow<T>
{
    /// <summary>A flow firing the given trigger after every spawned value.</summary>
    public FlowLink(IFlow<T> origin, ITrigger trigger) : this(
        origin, async _ => await trigger.Act()
    )
    { }

    /// <summary>A flow applying the given effect to every spawned value.</summary>
    public FlowLink(IFlow<T> origin, IEffect<T> effect) : this(origin, effect.Fire)
    { }

    public async IAsyncEnumerable<T> Yield([EnumeratorCancellation] CancellationToken ct = default)
    {
        await foreach (var spawned in origin.Yield(ct))
        {
            await act(spawned);
            yield return spawned;
        }
    }

    public IFlow<T> Trigger(ITrigger trigger) => new FlowLink<T>(this, trigger);

    public IFlow<T> Effect(IEffect<T> effect) => new FlowLink<T>(this, effect);

    public IFlow<TNewSpawn> Craft<TNewSpawn>(ICraft<T, TNewSpawn> craft) =>
        new CraftedFlow<T, TNewSpawn>(this, craft);
}
