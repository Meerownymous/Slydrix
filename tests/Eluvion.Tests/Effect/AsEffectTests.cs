using Eluvion.Craft;
using Eluvion.Effect;
using Eluvion.Trigger;
using Xunit;

namespace Eluvion.Tests.Effect;

public sealed class AsEffectTests
{
    [Fact]
    public async Task Act_WithAsyncFunc_ReceivesCorrectInput()
    {
        var received = 0;
        await new AsEffect<int>(ipt => received = ipt).Fire(42);
        Assert.Equal(42, received);
    }

    [Fact]
    public async Task Act_WithSyncAction_ReceivesCorrectInput()
    {
        var received = 0;
        await new AsEffect<int>(ipt => received = ipt).Fire(42);
        Assert.Equal(42, received);
    }

    [Fact]
    public async Task Act_WithParamlessFuncTask_Executes()
    {
        var called = false;
        await new AsEffect<int>(() => { called = true; return Task.CompletedTask; }).Fire(0);
        Assert.True(called);
    }

    [Fact]
    public async Task Act_WithTrigger_ExecutesTrigger()
    {
        var called = false;
        await new AsEffect<int>(new AsTrigger(() => called = true)).Fire(0);
        Assert.True(called);
    }

    [Fact]
    public async Task Effect_BothEffectsReceiveSameInput()
    {
        var sum = 0;
        await new AsEffect<int>(ipt => sum += ipt)
            .Effect(new AsEffect<int>(ipt => sum += ipt))
            .Fire(21);
        Assert.Equal(42, sum);
    }

    [Fact]
    public async Task Trigger_ChainedTriggerExecutes()
    {
        var called = false;
        await new AsEffect<int>(_ => { })
            .Trigger(new AsTrigger(() => called = true))
            .Fire(0);
        Assert.True(called);
    }

    [Fact]
    public async Task InputFlowsThroughCraft()
        => Assert.Equal(42, await new AsEffect<int>(_ => { })
            .Craft(new AsCraft<int, int>(x => x))
            .Yield(42));

    [Fact]
    public async Task Craft_FiresEffectOnInput()
    {
        var seen = 0;
        var crafted =
            await new AsEffect<int>(ipt => seen = ipt)
                .Craft(new AsCraft<int, int>(x => x + 1))
                .Yield(42);
        Assert.Equal((42, 43), (seen, crafted));
    }
}
