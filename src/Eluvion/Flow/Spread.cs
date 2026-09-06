using System.Runtime.CompilerServices;

namespace Eluvion.Flow;

/// <summary>A flow spawning every item a seed yielded, one after another.</summary>
public sealed class Spread<T>(ISeed<IEnumerable<T>> origin) : IFlow<T>
{
    public async IAsyncEnumerable<T> Yield([EnumeratorCancellation] CancellationToken ct = default)
    {
        foreach (var spawned in await origin.Yield())
        {
            ct.ThrowIfCancellationRequested();
            yield return spawned;
        }
    }

    public IFlow<T> Trigger(ITrigger trigger) => new FlowLink<T>(this, trigger);

    public IFlow<T> Effect(IEffect<T> effect) => new FlowLink<T>(this, effect);

    public IFlow<TNewSpawn> Craft<TNewSpawn>(ICraft<T, TNewSpawn> craft) =>
        new CraftedFlow<T, TNewSpawn>(this, craft);
}

public static partial class FlowSmarts
{
    /// <summary>A flow spawning every item this seed yielded.</summary>
    public static IFlow<T> Spread<T>(this ISeed<IEnumerable<T>> origin) => new Spread<T>(origin);
}
