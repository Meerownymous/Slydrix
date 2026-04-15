using Eluvion.Craft;
using Eluvion.Effect;
using Eluvion.Seed;
using Eluvion.Trigger;
using Xunit;

namespace Eluvion.Tests.Seed;

public sealed class SubscriptionTests
{
    [Fact]
    public async Task Yield_EffectReceivesEachItem()
    {
        var obs = new TestObservable<int>();
        var received = new List<int>();

        var task = new Subscription<int>(obs)
            .Effect(new AsEffect<int>(received.Add))
            .Yield();

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

        var task = new Subscription<int>(obs)
            .Effect(new AsEffect<int>(_ => log.Add("first")))
            .Effect(new AsEffect<int>(_ => log.Add("second")))
            .Yield();

        obs.Emit(1);
        obs.Complete();

        await task;

        Assert.Equal(["first", "second"], log);
    }

    [Fact]
    public async Task Yield_TriggerFiredPerItem()
    {
        var obs = new TestObservable<int>();
        var count = 0;

        var task = new Subscription<int>(obs)
            .Trigger(new AsTrigger(() => count++))
            .Yield();

        obs.Emit(0);
        obs.Emit(0);
        obs.Emit(0);
        obs.Complete();

        await task;

        Assert.Equal(3, count);
    }

    [Fact]
    public async Task Yield_CraftRunsPerItem()
    {
        var obs = new TestObservable<int>();
        var received = new List<string>();

        var task = new Subscription<int>(obs)
            .Craft(new AsCraft<int, string>(x => x.ToString()))
            .Effect(new AsEffect<string>(received.Add))
            .Yield();

        obs.Emit(1);
        obs.Emit(2);
        obs.Complete();

        await task;

        Assert.Equal(["1", "2"], received);
    }

    [Fact]
    public async Task Yield_ReturnsLastEmittedValue()
    {
        var obs = new TestObservable<int>();

        var task = new Subscription<int>(obs).Yield();

        obs.Emit(10);
        obs.Emit(20);
        obs.Emit(30);
        obs.Complete();

        Assert.Equal(30, await task);
    }

    [Fact]
    public async Task Yield_StopsWhenObservableCompletes()
    {
        var obs = new TestObservable<int>();
        var received = new List<int>();

        var task = new Subscription<int>(obs)
            .Effect(new AsEffect<int>(received.Add))
            .Yield();

        obs.Emit(1);
        obs.Complete();
        obs.Emit(2); // emitted after Complete — must be ignored

        await task;

        Assert.Equal([1], received);
    }

    [Fact]
    public async Task Yield_StopsOnCancellation()
    {
        var obs = new TestObservable<int>();
        var cts = new CancellationTokenSource();

        var task = new Subscription<int>(obs, cts.Token).Yield();

        await cts.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(() => task);
    }

    [Fact]
    public async Task Yield_UnsubscribesOnComplete()
    {
        var obs = new TestObservable<int>();

        var task = new Subscription<int>(obs).Yield();

        obs.Complete();
        await task;

        Assert.Equal(0, obs.SubscriberCount);
    }

    [Fact]
    public async Task Yield_UnsubscribesOnCancellation()
    {
        var obs = new TestObservable<int>();
        var cts = new CancellationTokenSource();

        var task = new Subscription<int>(obs, cts.Token).Yield();

        await cts.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(() => task);
        Assert.Equal(0, obs.SubscriberCount);
    }

    [Fact]
    public async Task Yield_CraftThenEffect_EffectReceivesCraftedValue()
    {
        var obs = new TestObservable<int>();
        var received = new List<string>();

        var task = new Subscription<int>(obs)
            .Craft(new AsCraft<int, string>(x => $"item-{x}"))
            .Effect(new AsEffect<string>(received.Add))
            .Yield();

        obs.Emit(1);
        obs.Emit(2);
        obs.Complete();

        await task;

        Assert.Equal(["item-1", "item-2"], received);
    }

    // ---- helpers ----

    private sealed class TestObservable<T> : IObservable<T>
    {
        private readonly List<IObserver<T>> _observers = [];
        private bool _completed;

        public int SubscriberCount => _observers.Count;

        public IDisposable Subscribe(IObserver<T> observer)
        {
            _observers.Add(observer);
            return new Unsubscriber(_observers, observer);
        }

        public void Emit(T value)
        {
            if (_completed) return;
            foreach (var o in _observers.ToList())
                o.OnNext(value);
        }

        public void Complete()
        {
            _completed = true;
            foreach (var o in _observers.ToList())
                o.OnCompleted();
        }

        private sealed class Unsubscriber(List<IObserver<T>> list, IObserver<T> item) : IDisposable
        {
            public void Dispose() => list.Remove(item);
        }
    }
}
