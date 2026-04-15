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
