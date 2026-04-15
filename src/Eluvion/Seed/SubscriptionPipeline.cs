using System.Threading.Channels;

namespace Eluvion.Seed;

/// <summary>
/// Two-type-parameter pipeline that accumulates Craft/Effect/Trigger steps for a subscription.
/// Each step wraps the previous pipeline as a closure — no loop runs until Yield() is called.
/// </summary>
public sealed class SubscriptionPipeline<TSource, TOut>(
    IObservable<TSource> source,
    Func<TSource, Task<TOut>> pipeline,
    CancellationToken ct
) : ISeed<TOut>
{
    public ISeed<TOut> Effect(IEffect<TOut> effect) =>
        new SubscriptionPipeline<TSource, TOut>(source, async raw =>
        {
            var result = await pipeline(raw);
            await effect.Fire(result);
            return result;
        }, ct);

    public ISeed<TOut> Trigger(ITrigger trigger) =>
        new SubscriptionPipeline<TSource, TOut>(source, async raw =>
        {
            var result = await pipeline(raw);
            await trigger.Act();
            return result;
        }, ct);

    public ISeed<TNew> Craft<TNew>(ICraft<TOut, TNew> craft) =>
        new SubscriptionPipeline<TSource, TNew>(source,
            async raw => await craft.Yield(await pipeline(raw)), ct);

    /// <summary>
    /// Subscribes to the source, runs the accumulated pipeline for each event,
    /// and unsubscribes automatically when done.
    /// </summary>
    public async Task<TOut> Yield()
    {
        var channel = Channel.CreateUnbounded<TSource>();
        using var subscription = source.Subscribe(new Observed<TSource>(channel.Writer));

        TOut last = default!;
        await foreach (var raw in channel.Reader.ReadAllAsync(ct))
            last = await pipeline(raw);

        return last;
    }
}
