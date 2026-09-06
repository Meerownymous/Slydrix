using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace Eluvion.Flow;

/// <summary>
/// A flow spawning every value an observable emits. The subscription is released
/// when the observable completes, when the token is cancelled, or when the
/// consumer stops enumerating.
/// </summary>
public sealed class ObservedFlow<T>(IObservable<T> source) : IFlow<T>
{
    public async IAsyncEnumerable<T> Yield([EnumeratorCancellation] CancellationToken ct = default)
    {
        var channel = Channel.CreateUnbounded<T>();
        using var subscription = source.Subscribe(new Observed<T>(channel.Writer));

        await foreach (var spawned in channel.Reader.ReadAllAsync(ct))
            yield return spawned;
    }

    public IFlow<T> Trigger(ITrigger trigger) => new FlowLink<T>(this, trigger);

    public IFlow<T> Effect(IEffect<T> effect) => new FlowLink<T>(this, effect);

    public IFlow<TNewSpawn> Craft<TNewSpawn>(ICraft<T, TNewSpawn> craft) =>
        new CraftedFlow<T, TNewSpawn>(this, craft);
}
