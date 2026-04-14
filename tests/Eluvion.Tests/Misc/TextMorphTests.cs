using Xunit;
using Eluvion.Misc;

namespace Slydrix.Tests.Misc;

public sealed class TextMorphTests
{
    [Fact]
    public void ImplicitFromString_CanBePassedDirectly()
        => Assert.Equal(42, new DeserializedJson<int>("42").Value());

    [Fact]
    public void ImplicitFromStream_CanBePassedDirectly()
    {
        using var stream = new MemoryStream("42"u8.ToArray());
        Assert.Equal(42, new DeserializedJson<int>(stream).Value());
    }
}
