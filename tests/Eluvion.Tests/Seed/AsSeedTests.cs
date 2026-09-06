using Eluvion.Craft;
using Eluvion.Effect;
using Eluvion.Seed;
using Eluvion.Trigger;
using Xunit;

namespace Eluvion.Tests.Seed;

public sealed class AsSeedTests
{
    [Fact]
    public async Task Yield_WithValue_ReturnsValue()
        => Assert.Equal(42, await new AsSeed<int>(42).Yield());

    [Fact]
    public async Task Yield_WithSyncFunc_ReturnsResult()
        => Assert.Equal(42, await new AsSeed<int>(() => 42).Yield());

    [Fact]
    public async Task Yield_WithAsyncFunc_ReturnsResult()
        => Assert.Equal(42, await new AsSeed<int>(async () => await Task.FromResult(42)).Yield());

    [Fact]
    public async Task Yield_WithTask_ReturnsResult()
        => Assert.Equal(42, await new AsSeed<int>(Task.FromResult(42)).Yield());

    [Fact]
    public async Task AsSeedExtension_WrapsValueCorrectly()
        => Assert.Equal(42, await 42.AsSeed().Yield());

    [Fact]
    public async Task TransformsYieldedValue()
        => Assert.Equal("42", await new AsSeed<int>(42)
            .Craft(new AsCraft<int, string>(x => x.ToString()))
            .Yield());

    [Fact]
    public async Task Effect_ReceivesYieldedValue()
    {
        var received = 0;
        await new AsSeed<int>(42).Effect(new AsEffect<int>(ipt => received = ipt)).Yield();
        Assert.Equal(42, received);
    }

    [Fact]
    public async Task Trigger_ExecutesTrigger()
    {
        var called = false;
        await new AsSeed<int>(42).Trigger(new AsTrigger(() => called = true)).Yield();
        Assert.True(called);
    }

    [Fact]
    public async Task Craft_TransformsYieldedValue()
        => Assert.Equal("42", await new AsSeed<int>(42)
            .Craft(x => x.ToString())
            .Yield());
}
