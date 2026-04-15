using System.Threading.Channels;

namespace Eluvion.Seed;

/// <summary>
/// A seed that subscribes to an observable and runs the full Eluvion pipeline for each emitted event.
/// Unsubscribes automatically when the observable completes or the cancellation token is cancelled.
/// </summary>
public sealed class Subscription<T>(IObservable<T> source, CancellationToken ct = default) : ISeed<T>
{
    private readonly ISeed<T> core = new SubscriptionPipeline<T, T>(source, Task.FromResult, ct);

    public ISeed<T> Effect(IEffect<T> effect) => core.Effect(effect);
    public ISeed<T> Trigger(ITrigger trigger) => core.Trigger(trigger);
    public ISeed<TNew> Craft<TNew>(ICraft<T, TNew> craft) => core.Craft(craft);
    public Task<T> Yield() => core.Yield();
}

/// <summary>
/// Internal two-type-parameter pipeline that accumulates Craft/Effect/Trigger steps.
/// Each step wraps the previous pipeline as a closure — no loop runs until Yield() is called.
/// </summary>
internal sealed class SubscriptionPipeline<TSource, TOut>(
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
        using var subscription = source.Subscribe(new ChannelObserver<TSource>(channel.Writer));

        TOut last = default!;
        await foreach (var raw in channel.Reader.ReadAllAsync(ct))
            last = await pipeline(raw);

        return last;
    }
}

file sealed class ChannelObserver<T>(ChannelWriter<T> writer) : IObserver<T>
{
    public void OnNext(T value)           => writer.TryWrite(value);
    public void OnCompleted()             => writer.Complete();
    public void OnError(Exception error)  => writer.Complete(error);
}
