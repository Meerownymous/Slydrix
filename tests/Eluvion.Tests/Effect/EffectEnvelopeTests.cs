using Eluvion.Craft;
using Eluvion.Effect;
using Eluvion.Trigger;
using Xunit;

namespace Eluvion.Tests.Effect;

public sealed class EffectEnvelopeTests
{
    [Fact]
    public async Task Act_DelegatesToWrappedCraft()
    {
        var called = false;
        await new CraftAsEffect<int, int>(new AsCraft<int, int>(x => { called = true; return x; })).Fire(0);
        Assert.True(called);
    }

    [Fact]
    public async Task Act_PassesCorrectInputToCraft()
    {
        var received = 0;
        await new CraftAsEffect<int, int>(new AsCraft<int, int>(x => { received = x; return x; })).Fire(42);
        Assert.Equal(42, received);
    }

    [Fact]
    public async Task Trigger_ChainedTriggerExecutes()
    {
        var called = false;
        await new CraftAsEffect<int, int>(new AsCraft<int, int>(x => x))
            .Trigger(new AsTrigger(() => called = true))
            .Fire(0);
        Assert.True(called);
    }

    [Fact]
    public async Task Effect_ChainedEffectReceivesInput()
    {
        var received = 0;
        await new CraftAsEffect<int, int>(new AsCraft<int, int>(x => x))
            .Effect(new AsEffect<int>(ipt => received = ipt))
            .Fire(42);
        Assert.Equal(42, received);
    }

    [Fact]
    public async Task InputFlowsThroughToCraft()
        => Assert.Equal(42, await new CraftAsEffect<int, int>(new AsCraft<int, int>(x => x))
            .Craft(new AsCraft<int, int>(x => x))
            .Yield(42));

    [Fact]
    public async Task Craft_FiresEffectOnInput()
    {
        var seen = 0;
        var crafted =
            await new CraftAsEffect<int, int>(new AsCraft<int, int>(x =>
                {
                    seen = x;
                    return x;
                }))
                .Craft(new AsCraft<int, int>(x => x + 1))
                .Yield(42);
        Assert.Equal((42, 43), (seen, crafted));
    }
}
