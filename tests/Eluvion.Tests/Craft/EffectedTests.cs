using Eluvion.Craft;
using Eluvion.Effect;
using Eluvion.Trigger;
using Xunit;

namespace Eluvion.Tests.Craft;

public sealed class EffectedTests
{
    [Fact]
    public async Task Yield_FiresEffectOnInputBeforeCrafting()
    {
        var seen = 0;
        var result =
            await new Effected<int, string>(
                new AsEffect<int>(ipt => seen = ipt),
                new AsCraft<int, string>(x => x.ToString())
            ).Yield(42);
        Assert.Equal((42, "42"), (seen, result));
    }

    [Fact]
    public async Task Yield_FiresEffectBeforeCraft()
    {
        var order = new List<string>();
        await new Effected<int, int>(
            new AsEffect<int>(_ => order.Add("effect")),
            new AsCraft<int, int>(x =>
            {
                order.Add("craft");
                return x;
            })
        ).Yield(42);
        Assert.Equal(new[] { "effect", "craft" }, order);
    }

    [Fact]
    public async Task Effect_ActsOnCraftedOutput()
    {
        var seen = 0;
        await new Effected<int, int>(
                new AsEffect<int>(_ => { }),
                new AsCraft<int, int>(x => x + 1)
            )
            .Effect(new AsEffect<int>(ipt => seen = ipt))
            .Yield(42);
        Assert.Equal(43, seen);
    }

    [Fact]
    public async Task Trigger_FiresAfterCrafting()
    {
        var called = false;
        var result =
            await new Effected<int, int>(
                    new AsEffect<int>(_ => { }),
                    new AsCraft<int, int>(x => x + 1)
                )
                .Trigger(new AsTrigger(() => called = true))
                .Yield(42);
        Assert.Equal((true, 43), (called, result));
    }
}
