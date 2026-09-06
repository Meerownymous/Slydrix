using Tonga;

namespace Eluvion.Fact;

/// <summary>A fact that is checked anew every time it is asked.</summary>
public sealed class Rechecked(Func<bool> condition) : IFact
{
    public bool IsTrue() => condition();
    public bool IsFalse() => !condition();
}
