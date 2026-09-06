using Eluvion.Craft;
using Tonga;

namespace Eluvion.Seed;

/// <summary>A seed whose value is provided by an async factory.</summary>
public sealed class AsSeed<TSeed>(Func<Task<TSeed>> spawnAsync) : ISeed<TSeed>
{
    /// <summary>A seed whose value is the result of the given task.</summary>
    public AsSeed(Task<TSeed> seed) : this(() => seed)
    { }

    /// <summary>A seed holding the given fixed value.</summary>
    public AsSeed(TSeed seed) : this(() => Task.FromResult(seed))
    { }

    /// <summary>A seed whose value is provided by the given synchronous factory.</summary>
    public AsSeed(Func<TSeed> seed) : this(() => Task.FromResult(seed()))
    { }

    /// <summary>A seed whose value is provided by the given scalar.</summary>
    public AsSeed(IScalar<TSeed> seed) : this(() => Task.FromResult(seed.Value()))
    { }
    
    public async Task<TSeed> Yield() => await spawnAsync();

    public ISeed<TSeed> Trigger(ITrigger trigger) =>
        new SeedLink<TSeed>(this, trigger);

    public ISeed<TSeed> Effect(IEffect<TSeed> effect) =>
        new SeedLink<TSeed>(this, effect);

    public ISeed<TNewSpawn> Craft<TNewSpawn>(ICraft<TSeed, TNewSpawn> craft) =>
        new CraftedSeed<TSeed, TNewSpawn>(this, craft);
}

public static partial class SeedSmarts
{
    public static ISeed<TSeed> AsSeed<TSeed>(this TSeed seed) => new AsSeed<TSeed>(seed);
    
    public static ISeed<TMapped> Mapped<TSeed, TMapped>(this ISeed<TSeed> seed, CraftMorph<TSeed, TMapped> mapping) =>
        seed.Craft(mapping);
    
    public static ISeed<TMapped> Mapped<TSeed, TMapped>(this ISeed<TSeed> seed, Func<TSeed, TMapped> mapping) =>
        seed.Craft(new AsCraft<TSeed, TMapped>(mapping));
}