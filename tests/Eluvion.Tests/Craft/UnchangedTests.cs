using Eluvion.Craft;
using Eluvion.Effect;
using Eluvion.Trigger;
using Xunit;

namespace Eluvion.Tests.Craft;

public sealed class UnchangedTests
{
    [Fact]
    public async Task Yield_WithEffect_ReturnsInput()
        => Assert.Equal(42, await new Unchanged<int>(new AsEffect<int>(_ => { })).Yield(42));

    [Fact]
    public async Task Yield_WithEffect_FiresOnInput()
    {
        var seen = 0;
        await new Unchanged<int>(new AsEffect<int>(ipt => seen = ipt)).Yield(42);
        Assert.Equal(42, seen);
    }

    [Fact]
    public async Task Yield_WithTrigger_FiresAndReturnsInput()
    {
        var called = false;
        var result = await new Unchanged<int>(new AsTrigger(() => called = true)).Yield(42);
        Assert.Equal((true, 42), (called, result));
    }

    [Fact]
    public async Task Craft_TransformsAfterActing()
    {
        var seen = 0;
        var result =
            await new Unchanged<int>(new AsEffect<int>(ipt => seen = ipt))
                .Craft(new AsCraft<int, int>(x => x + 1))
                .Yield(42);
        Assert.Equal((42, 43), (seen, result));
    }
}
