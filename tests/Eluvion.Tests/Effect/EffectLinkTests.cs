using Eluvion.Craft;
using Eluvion.Effect;
using Eluvion.Trigger;
using Xunit;

namespace Eluvion.Tests.Effect;

public sealed class EffectLinkTests
{
    [Fact]
    public async Task Act_FirstEffectExecutes()
    {
        var called = false;
        await new EffectLink<int>(
            new AsEffect<int>(_ => called = true),
            new AsEffect<int>(_ => { })
        ).Fire(0);
        Assert.True(called);
    }

    [Fact]
    public async Task Act_SecondEffectExecutes()
    {
        var called = false;
        await new EffectLink<int>(
            new AsEffect<int>(_ => { }),
            new AsEffect<int>(_ => called = true)
        ).Fire(0);
        Assert.True(called);
    }

    [Fact]
    public async Task Act_BothEffectsReceiveSameInput()
    {
        var sum = 0;
        await new EffectLink<int>(
            new AsEffect<int>(ipt => sum += ipt),
            new AsEffect<int>(ipt => sum += ipt)
        ).Fire(21);
        Assert.Equal(42, sum);
    }

    [Fact]
    public async Task Act_FirstExecutesBeforeSecond()
    {
        var order = new List<int>();
        await new EffectLink<int>(
            new AsEffect<int>(_ => order.Add(1)),
            new AsEffect<int>(_ => order.Add(2))
        ).Fire(0);
        Assert.Equal(1, order[0]);
    }

    [Fact]
    public async Task Trigger_ChainedTriggerExecutes()
    {
        var called = false;
        await new EffectLink<int>(new AsEffect<int>(_ => { }), new AsEffect<int>(_ => { }))
            .Trigger(new AsTrigger(() => called = true))
            .Fire(0);
        Assert.True(called);
    }

    [Fact]
    public async Task Effect_ChainedEffectExecutes()
    {
        var called = false;
        await new EffectLink<int>(new AsEffect<int>(_ => { }), new AsEffect<int>(_ => { }))
            .Effect(new AsEffect<int>(_ => called = true))
            .Fire(0);
        Assert.True(called);
    }

    [Fact]
    public async Task InputPassesThroughToCraft()
        => Assert.Equal(42, await new EffectLink<int>(
                new AsEffect<int>(_ => { }),
                new AsEffect<int>(_ => { }))
            .Craft(new AsCraft<int, int>(x => x))
            .Yield(42));

    [Fact]
    public async Task Craft_FiresBothEffectsOnInput()
    {
        var first = 0;
        var second = 0;
        var crafted =
            await new EffectLink<int>(
                    new AsEffect<int>(ipt => first = ipt),
                    new AsEffect<int>(ipt => second = ipt))
                .Craft(new AsCraft<int, int>(x => x + 1))
                .Yield(42);
        Assert.Equal((42, 42, 43), (first, second, crafted));
    }
}
