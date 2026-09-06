using Eluvion.Trigger;
using Xunit;

namespace Eluvion.Tests.Trigger;

public sealed class TriggerMorphTests
{
    [Fact]
    public async Task Act_DelegatesToWrappedTrigger()
    {
        var called = false;
        await new TriggerMorph(new AsTrigger(() => called = true)).Act();
        Assert.True(called);
    }

    [Fact]
    public async Task ImplicitFromAction_ExecutesOnAct()
    {
        var called = false;
        TriggerMorph triggerMorph = (Action)(() => called = true);
        await triggerMorph.Act();
        Assert.True(called);
    }

    [Fact]
    public async Task Trigger_ChainedTriggerExecutes()
    {
        var called = false;
        await new TriggerMorph(new AsTrigger(() => { }))
            .Trigger(new AsTrigger(() => called = true))
            .Act();
        Assert.True(called);
    }
}
