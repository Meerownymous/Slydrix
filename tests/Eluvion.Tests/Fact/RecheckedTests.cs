using Eluvion.Fact;
using Xunit;

namespace Eluvion.Tests.Fact;

public sealed class RecheckedTests
{
    [Fact]
    public void IsTrue_ReflectsCondition()
        => Assert.True(new Rechecked(() => true).IsTrue());

    [Fact]
    public void IsFalse_IsTheOpposite()
        => Assert.True(new Rechecked(() => false).IsFalse());

    [Fact]
    public void AsksConditionEveryTime()
    {
        var asked = 0;
        var fact = new Rechecked(() =>
        {
            asked++;
            return true;
        });
        fact.IsTrue();
        fact.IsTrue();
        Assert.Equal(2, asked);
    }

    [Fact]
    public void FollowsAChangingCondition()
    {
        var open = true;
        var fact = new Rechecked(() => open);
        var before = fact.IsTrue();
        open = false;
        Assert.Equal((true, false), (before, fact.IsTrue()));
    }
}
