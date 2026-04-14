using Tonga;
using Tonga.Enumerable;

namespace Eluvion.Seed;

/// <summary>A seed whose value is determined by the first matching condition.</summary>
public sealed class SeedSwitch<TSeed>(IEnumerable<(Func<bool> condition, ISeed<TSeed> result)> cases) : SeedEnvelope<TSeed>(() =>
{
    ISeed<TSeed> result = default;
    bool matched = false;
    foreach (var cse in cases)
    {
        if (cse.condition())
        {
            matched = true;
            result = cse.result;
            break;
        }
    }

    if (!matched)
        throw new InvalidOperationException("No matching case found");

    return result;
})
{
    /// <summary>A seed selecting the first case whose condition holds, from the given seeds.</summary>
    public SeedSwitch(params (Func<bool> condition, ISeed<TSeed> result)[] cases) : this(
        cases.AsMapped(c => (c.condition, c.result))
    )
    { }

    /// <summary>A seed selecting the first case whose condition holds, from the given factories.</summary>
    public SeedSwitch(params (Func<bool> condition, Func<TSeed> result)[] cases) : this(
        cases.AsMapped(c => (c.condition, new AsSeed<TSeed>(c.result)))
    )
    { }

    /// <summary>A seed selecting the first case whose condition holds, from the given AsSeed instances.</summary>
    public SeedSwitch(IEnumerable<(Func<bool> condition, AsSeed<TSeed> result)> cases) : this(
        cases.AsMapped(c => (c.condition, c.result as ISeed<TSeed>))
    )
    { }

    /// <summary>A seed selecting the first case whose condition holds, from the given scalars.</summary>
    public SeedSwitch(params (Func<bool> condition, IScalar<TSeed> result)[] cases) : this(
        cases.AsMapped(c => (c.condition, new AsSeed<TSeed>(c.result)))
    )
    { }
}
