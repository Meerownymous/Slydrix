using Eluvion.Fact;
using Tonga;
using Tonga.Enumerable;

namespace Eluvion.Seed;

/// <summary>A seed whose value comes from the first case whose condition holds.</summary>
public sealed class SeedSwitch<TSeed>(IEnumerable<(IFact condition, ISeed<TSeed> seed)> cases) : SeedEnvelope<TSeed>(() =>
    cases.FirstOne(
        cse => cse.condition.IsTrue(),
        new InvalidOperationException("No matching case found")
    ).Value().seed
)
{
    /// <summary>A seed selecting the first case whose fact holds, from the given seeds.</summary>
    public SeedSwitch(params (IFact condition, ISeed<TSeed> seed)[] cases) : this(
        cases.AsMapped(c => (c.condition, c.seed))
    )
    { }

    /// <summary>A seed selecting the first case whose condition holds, from the given seeds.</summary>
    public SeedSwitch(IEnumerable<(Func<bool> condition, ISeed<TSeed> seed)> cases) : this(
        cases.AsMapped(c => ((IFact)new Rechecked(c.condition), c.seed))
    )
    { }

    /// <summary>A seed selecting the first case whose condition holds, from the given seeds.</summary>
    public SeedSwitch(params (Func<bool> condition, ISeed<TSeed> seed)[] cases) : this(
        cases.AsMapped(c => ((IFact)new Rechecked(c.condition), c.seed))
    )
    { }

    /// <summary>A seed selecting the first case whose condition holds, from the given factories.</summary>
    public SeedSwitch(params (Func<bool> condition, Func<TSeed> value)[] cases) : this(
        cases.AsMapped(c => ((IFact)new Rechecked(c.condition), new AsSeed<TSeed>(c.value) as ISeed<TSeed>))
    )
    { }

    /// <summary>A seed selecting the first case whose condition holds, from the given AsSeed instances.</summary>
    public SeedSwitch(IEnumerable<(Func<bool> condition, AsSeed<TSeed> seed)> cases) : this(
        cases.AsMapped(c => ((IFact)new Rechecked(c.condition), c.seed as ISeed<TSeed>))
    )
    { }

    /// <summary>A seed selecting the first case whose condition holds, from the given scalars.</summary>
    public SeedSwitch(params (Func<bool> condition, IScalar<TSeed> value)[] cases) : this(
        cases.AsMapped(c => ((IFact)new Rechecked(c.condition), new AsSeed<TSeed>(c.value) as ISeed<TSeed>))
    )
    { }
}
