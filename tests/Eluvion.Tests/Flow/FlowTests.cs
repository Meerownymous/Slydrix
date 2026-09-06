using Eluvion.Craft;
using Eluvion.Effect;
using Eluvion.Flow;
using Eluvion.Seed;
using Eluvion.Trigger;
using Xunit;

namespace Eluvion.Tests.Flow;

public sealed class FlowTests
{
    private static IFlow<int> Three() => new AsFlow<int>(new AsAsync<int>([1, 2, 3]));

    [Fact]
    public async Task AsFlow_SpawnsEveryValue()
        => Assert.Equal([1, 2, 3], await new Drained<int>(Three()).Yield());

    [Fact]
    public async Task CraftedFlow_TransformsEveryValue()
        => Assert.Equal(["1", "2", "3"], await new Drained<string>(
            Three().Craft(new AsCraft<int, string>(x => x.ToString()))
        ).Yield());

    [Fact]
    public async Task FlowLink_AppliesEffectToEveryValue()
    {
        var seen = new List<int>();
        await new Drained<int>(Three().Effect(new AsEffect<int>(seen.Add))).Yield();
        Assert.Equal([1, 2, 3], seen);
    }

    [Fact]
    public async Task FlowLink_PassesValuesOnUnchanged()
        => Assert.Equal([1, 2, 3], await new Drained<int>(
            Three().Effect(new AsEffect<int>(_ => { }))
        ).Yield());

    [Fact]
    public async Task FlowLink_FiresTriggerPerValue()
    {
        var count = 0;
        await new Drained<int>(Three().Trigger(new AsTrigger(() => count++))).Yield();
        Assert.Equal(3, count);
    }

    [Fact]
    public async Task Effect_ActsOnCraftedValue()
    {
        var seen = new List<string>();
        await new Drained<string>(
            Three()
                .Craft(new AsCraft<int, string>(x => $"item-{x}"))
                .Effect(new AsEffect<string>(seen.Add))
        ).Yield();
        Assert.Equal(["item-1", "item-2", "item-3"], seen);
    }

    [Fact]
    public void Craft_KeepsChainStructure()
        => Assert.IsType<CraftedFlow<int, int>>(Three().Craft(new AsCraft<int, int>(x => x)));

    [Fact]
    public async Task Spread_TurnsASeedOfManyIntoAFlow()
        => Assert.Equal([1, 2, 3], await new Drained<int>(
            new Spread<int>(new AsSeed<IEnumerable<int>>(new List<int> { 1, 2, 3 }))
        ).Yield());

    [Fact]
    public async Task Spread_CraftsEveryItem()
        => Assert.Equal([2, 3, 4], await new Drained<int>(
            new Spread<int>(new AsSeed<IEnumerable<int>>(new List<int> { 1, 2, 3 }))
                .Craft(new AsCraft<int, int>(x => x + 1))
        ).Yield());

    [Fact]
    public async Task LastOf_HoldsTheLastValue()
    {
        var last = await new LastOf<int>(Three()).Yield();
        Assert.Equal((true, 3), (last.Has(), last.Value()));
    }

    [Fact]
    public async Task LastOf_IsEmptyOnAnEmptyFlow()
        => Assert.False((await new LastOf<int>(new AsFlow<int>(new AsAsync<int>([]))).Yield()).Has());

    [Fact]
    public async Task FirstOf_HoldsTheFirstValue()
    {
        var first = await new FirstOf<int>(Three()).Yield();
        Assert.Equal((true, 1), (first.Has(), first.Value()));
    }

    [Fact]
    public async Task FirstOf_IsEmptyOnAnEmptyFlow()
        => Assert.False((await new FirstOf<int>(new AsFlow<int>(new AsAsync<int>([]))).Yield()).Has());

    [Fact]
    public async Task FirstOf_StopsAfterTheFirstValue()
    {
        var seen = 0;
        await new FirstOf<int>(Three().Effect(new AsEffect<int>(_ => seen++))).Yield();
        Assert.Equal(1, seen);
    }

    [Fact]
    public async Task Drained_IsEmptyOnAnEmptyFlow()
        => Assert.Empty(await new Drained<int>(new AsFlow<int>(new AsAsync<int>([]))).Yield());
}
