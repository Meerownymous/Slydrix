using Eluvion.Seed;
using Xunit;

namespace Eluvion.Tests.Seed;

public sealed class SeedExtensionsTests
{
    [Fact]
    public async Task TriggersWithSyncFunction()
    {
        var called = false;
        await 42.AsSeed().Trigger(() => called = true).Yield();
        Assert.True(called);
    }

    [Fact]
    public async Task TriggersWithAsyncFunction()
    {
        var called = false;
        await 42.AsSeed().Trigger(async () => await Task.FromResult(called = true)).Yield();
        Assert.True(called);
    }

    [Fact]
    public async Task HasEffectWithSyncFunction()
    {
        var called = false;
        await 42.AsSeed().Effect(number => called = true).Yield();
        Assert.True(called);
    }

    [Fact]
    public async Task HasEffectWithAsyncFunction()
    {
        var called = false;
        await 42.AsSeed().Effect(async number => await Task.FromResult(called = true)).Yield();
        Assert.True(called);
    }

    [Fact]
    public async Task CraftsWithSyncFunction()
        => Assert.Equal(43, await 42.AsSeed().Craft(number => number + 1).Yield());

    [Fact]
    public async Task CraftsWithAsyncFunction()
        => Assert.Equal(43, await 42.AsSeed().Craft(async number => await Task.FromResult(number + 1)).Yield());
}
