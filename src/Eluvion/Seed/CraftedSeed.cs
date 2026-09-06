namespace Eluvion.Seed;

/// <summary>A seed whose value has been transformed by a craft.</summary>
public sealed class CraftedSeed<TSeed, TCrafted>(ISeed<TSeed> origin, ICraft<TSeed, TCrafted> craft) : ISeed<TCrafted>
{
    public async Task<TCrafted> Yield() => await craft.Yield(await origin.Yield());

    public ISeed<TCrafted> Trigger(ITrigger trigger) =>
        new SeedLink<TCrafted>(this, trigger);

    public ISeed<TCrafted> Effect(IEffect<TCrafted> effect) =>
        new SeedLink<TCrafted>(this, effect);

    public ISeed<TNext> Craft<TNext>(ICraft<TCrafted, TNext> next) =>
        new CraftedSeed<TCrafted, TNext>(this, next);
}
