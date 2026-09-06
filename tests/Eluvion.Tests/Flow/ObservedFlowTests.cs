using Eluvion.Craft;
using Eluvion.Effect;
using Eluvion.Flow;
using Eluvion.Seed;
using Eluvion.Trigger;
using Xunit;

namespace Eluvion.Tests.Flow;

public sealed class ObservedFlowTests
{
    [Fact]
    public async Task Yield_EffectReceivesEachValue()
    {
        var obs = new TestObservable<int>();
        var received = new List<int>();

        var task = new Drained<int>(
            new ObservedFlow<int>(obs).Effect(new AsEffect<int>(received.Add))
        ).Yield();

        obs.Emit(1);
        obs.Emit(2);
        obs.Emit(3);
        obs.Complete();

        await task;

        Assert.Equal([1, 2, 3], received);
    }

    [Fact]
    public async Task Yield_MultipleEffects_FiredInOrder()
    {
        var obs = new TestObservable<int>();
        var log = new List<string>();

        var task = new Drained<int>(
            new ObservedFlow<int>(obs)
                .Effect(new AsEffect<int>(_ => log.Add("first")))
                .Effect(new AsEffect<int>(_ => log.Add("second")))
        ).Yield();

        obs.Emit(1);
        obs.Complete();

        await task;

        Assert.Equal(["first", "second"], log);
    }

    [Fact]
    public async Task Yield_TriggerFiredPerValue()
    {
        var obs = new TestObservable<int>();
        var count = 0;

        var task = new Drained<int>(
            new ObservedFlow<int>(obs).Trigger(new AsTrigger(() => count++))
        ).Yield();

        obs.Emit(0);
        obs.Emit(0);
        obs.Emit(0);
        obs.Complete();

        await task;

        Assert.Equal(3, count);
    }

    [Fact]
    public async Task Yield_CraftRunsPerValue()
    {
        var obs = new TestObservable<int>();

        var task = new Drained<string>(
            new ObservedFlow<int>(obs).Craft(new AsCraft<int, string>(x => $"item-{x}"))
        ).Yield();

        obs.Emit(1);
        obs.Emit(2);
        obs.Complete();

        Assert.Equal(["item-1", "item-2"], await task);
    }

    [Fact]
    public async Task Yield_StopsWhenObservableCompletes()
    {
        var obs = new TestObservable<int>();

        var task = new Drained<int>(new ObservedFlow<int>(obs)).Yield();

        obs.Emit(1);
        obs.Complete();
        obs.Emit(2); // emitted after Complete — must be ignored

        Assert.Equal([1], await task);
    }

    [Fact]
    public async Task Yield_StopsOnCancellation()
    {
        var obs = new TestObservable<int>();
        var cts = new CancellationTokenSource();

        var task = new Drained<int>(new ObservedFlow<int>(obs), cts.Token).Yield();

        await cts.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(() => task);
    }

    [Fact]
    public async Task Yield_UnsubscribesOnComplete()
    {
        var obs = new TestObservable<int>();

        var task = new Drained<int>(new ObservedFlow<int>(obs)).Yield();

        obs.Complete();
        await task;

        Assert.Equal(0, obs.SubscriberCount);
    }

    [Fact]
    public async Task Yield_UnsubscribesOnCancellation()
    {
        var obs = new TestObservable<int>();
        var cts = new CancellationTokenSource();

        var task = new Drained<int>(new ObservedFlow<int>(obs), cts.Token).Yield();

        await cts.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(() => task);
        Assert.Equal(0, obs.SubscriberCount);
    }
}
