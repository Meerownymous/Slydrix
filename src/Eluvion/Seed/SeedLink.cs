namespace Eluvion.Seed;

/// <summary>A seed chained with a side effect on its yielded value.</summary>
public class SeedLink<TSeed>(ISeed<TSeed> first, Func<TSeed, Task> second) : ISeed<TSeed>
{
    /// <summary>A seed that fires the given trigger after yielding.</summary>
    public SeedLink(ISeed<TSeed> first, ITrigger trigger) : this(
        first, async _ => await trigger.Act()
    )
    { }

    /// <summary>A seed that applies the given effect to its value after yielding.</summary>
    public SeedLink(ISeed<TSeed> first, IEffect<TSeed> effect) : this(
        first, async seed => await effect.Fire(seed)
    )
    { }
    
    public async Task<TSeed> Yield()
    {
        var result = await first.Yield();
        await second(result);
        return result;
    }

    public ISeed<TSeed> Trigger(ITrigger trigger) =>
        new SeedLink<TSeed>(this, trigger);

    public ISeed<TSeed> Effect(IEffect<TSeed> effect) =>
        new SeedLink<TSeed>(this, effect);

    public ISeed<TNewSeed> Craft<TNewSeed>(ICraft<TSeed, TNewSeed> craft) =>
        new CraftedSeed<TSeed, TNewSeed>(this, craft);
}