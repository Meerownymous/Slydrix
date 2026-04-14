using Xunit;
using Eluvion.Seed;

namespace Eluvion.Tests.Seed
{
    public sealed class SeedSwitchTests
    {
        [Fact]
        public async Task Yield_FirstConditionTrue_ReturnsFirst()
            => Assert.Equal(1, await new SeedSwitch<int>(
                (() => true, () => 1),
                (() => true, () => 2)
            ).Yield());

        [Fact]
        public async Task Yield_FirstConditionFalse_ReturnsSecond()
            => Assert.Equal(2, await new SeedSwitch<int>(
                (() => false, () => 1),
                (() => true,  () => 2)
            ).Yield());

        [Fact]
        public async Task Yield_WithISeed_ReturnsValue()
            => Assert.Equal(42, await new SeedSwitch<int>(
                (() => true, new AsSeed<int>(42))
            ).Yield());

        [Fact]
        public async Task Yield_NoMatchingCondition_ThrowsInvalidOperationException()
            => await Assert.ThrowsAsync<InvalidOperationException>(() =>
                new SeedSwitch<int>((() => false, () => 0)).Yield()
            );
    }
}
