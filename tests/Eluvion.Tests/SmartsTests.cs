using Eluvion.Craft;
using Eluvion.Effect;
using Eluvion.Flow;
using Eluvion.Seed;
using Eluvion.Tests.Flow;
using Eluvion.Trigger;
using Tonga.Scalar;
using Xunit;

namespace Eluvion.Tests;

public sealed class SmartsTests
{
    // ---- entry points ----

    [Fact]
    public async Task AsSeed_OnAValue()
        => Assert.Equal(42, await 42.AsSeed().Yield());

    [Fact]
    public async Task AsSeed_OnATask_UnwrapsIt()
        => Assert.Equal(42, await Task.FromResult(42).AsSeed().Yield());

    [Fact]
    public async Task AsSeed_OnAFactory_UnwrapsIt()
        => Assert.Equal(42, await ((Func<int>)(() => 42)).AsSeed().Yield());

    [Fact]
    public async Task AsSeed_OnAnAsyncFactory_UnwrapsIt()
        => Assert.Equal(42, await ((Func<Task<int>>)(() => Task.FromResult(42))).AsSeed().Yield());

    [Fact]
    public async Task AsSeed_OnAScalar_UnwrapsIt()
        => Assert.Equal(42, await new AsScalar<int>(42).AsSeed().Yield());

    [Fact]
    public async Task AsCraft_OnAFunction()
        => Assert.Equal("42", await ((Func<int, string>)(x => x.ToString())).AsCraft().Yield(42));

    [Fact]
    public async Task AsEffect_OnAnAction()
    {
        var seen = 0;
        await ((Action<int>)(x => seen = x)).AsEffect().Fire(42);
        Assert.Equal(42, seen);
    }

    [Fact]
    public async Task AsTrigger_OnAnAction()
    {
        var called = false;
        await ((Action)(() => called = true)).AsTrigger().Act();
        Assert.True(called);
    }

    [Fact]
    public async Task AsFlow_OnAnAsyncSequence()
        => Assert.Equal([1, 2, 3], await new AsAsync<int>([1, 2, 3]).AsFlow().Drained().Yield());

    [Fact]
    public async Task AsFlow_OnAnObservable()
    {
        var obs = new TestObservable<int>();
        var task = obs.AsFlow().Drained().Yield();

        obs.Emit(1);
        obs.Emit(2);
        obs.Complete();

        Assert.Equal([1, 2], await task);
    }

    [Fact]
    public async Task Spread_OnASeedOfMany()
        => Assert.Equal([1, 2, 3], await new AsSeed<IEnumerable<int>>(new List<int> { 1, 2, 3 })
            .Spread()
            .Drained()
            .Yield());

    // ---- exits ----

    [Fact]
    public async Task LastOf_OnAFlow()
        => Assert.Equal(3, (await new AsAsync<int>([1, 2, 3]).AsFlow().LastOf().Yield()).Value());

    [Fact]
    public async Task FirstOf_OnAFlow()
        => Assert.Equal(1, (await new AsAsync<int>([1, 2, 3]).AsFlow().FirstOf().Yield()).Value());

    // ---- the whole chain, without a single new ----

    [Fact]
    public async Task AChainOfSmartsOnly()
    {
        var seen = new List<string>();
        var fired = 0;

        var last =
            await new AsAsync<int>([1, 2, 3]).AsFlow()
                .Craft(x => $"item-{x}")
                .Effect(seen.Add)
                .Trigger(() => fired++)
                .LastOf()
                .Yield();

        Assert.Equal(["item-1", "item-2", "item-3"], seen);
        Assert.Equal(3, fired);
        Assert.Equal("item-3", last.Value());
    }
}
