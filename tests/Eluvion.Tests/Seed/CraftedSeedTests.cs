using Eluvion.Craft;
using Eluvion.Effect;
using Eluvion.Seed;
using Eluvion.Trigger;
using Xunit;

namespace Eluvion.Tests.Seed;

public sealed class CraftedSeedTests
{
    [Fact]
    public async Task Yield_TransformsOriginValue()
        => Assert.Equal("42", await new CraftedSeed<int, string>(
            new AsSeed<int>(42),
            new AsCraft<int, string>(x => x.ToString())
        ).Yield());

    [Fact]
    public async Task Effect_ActsOnCraftedValue()
    {
        var seen = "";
        await new CraftedSeed<int, string>(
                new AsSeed<int>(42),
                new AsCraft<int, string>(x => x.ToString())
            )
            .Effect(new AsEffect<string>(ipt => seen = ipt))
            .Yield();
        Assert.Equal("42", seen);
    }

    [Fact]
    public async Task Trigger_FiresAfterYielding()
    {
        var called = false;
        var result =
            await new CraftedSeed<int, int>(new AsSeed<int>(42), new AsCraft<int, int>(x => x + 1))
                .Trigger(new AsTrigger(() => called = true))
                .Yield();
        Assert.Equal((true, 43), (called, result));
    }

    [Fact]
    public async Task Craft_ChainsFurther()
        => Assert.Equal(44, await new CraftedSeed<int, int>(
                new AsSeed<int>(42),
                new AsCraft<int, int>(x => x + 1)
            )
            .Craft(new AsCraft<int, int>(x => x + 1))
            .Yield());

    [Fact]
    public async Task Craft_KeepsChainStructure()
        => Assert.IsType<CraftedSeed<int, int>>(
            new AsSeed<int>(42).Craft(new AsCraft<int, int>(x => x))
        );
}
