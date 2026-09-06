using Eluvion.Craft;
using Eluvion.Trigger;
using Xunit;

namespace Eluvion.Tests.Craft;

public sealed class TriggeredTests
{
    [Fact]
    public async Task Yield_FiresTriggerAndCrafts()
    {
        var called = false;
        var result =
            await new Triggered<int, string>(
                new AsTrigger(() => called = true),
                new AsCraft<int, string>(x => x.ToString())
            ).Yield(42);
        Assert.Equal((true, "42"), (called, result));
    }

    [Fact]
    public async Task Yield_FiresTriggerBeforeCraft()
    {
        var order = new List<string>();
        await new Triggered<int, int>(
            new AsTrigger(() => order.Add("trigger")),
            new AsCraft<int, int>(x =>
            {
                order.Add("craft");
                return x;
            })
        ).Yield(42);
        Assert.Equal(new[] { "trigger", "craft" }, order);
    }

    [Fact]
    public async Task Craft_TransformsFurther()
        => Assert.Equal(44, await new Triggered<int, int>(
                new AsTrigger(() => { }),
                new AsCraft<int, int>(x => x + 1)
            )
            .Craft(new AsCraft<int, int>(x => x + 1))
            .Yield(42));
}
