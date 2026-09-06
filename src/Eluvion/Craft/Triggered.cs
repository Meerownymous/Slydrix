namespace Eluvion.Craft;

/// <summary>A craft that has fired a trigger before transforming its input.</summary>
public sealed class Triggered<TIn, TOut>(ITrigger trigger, ICraft<TIn, TOut> craft) : ICraft<TIn, TOut>
{
    public async Task<TOut> Yield(TIn ipt)
    {
        await trigger.Act();
        return await craft.Yield(ipt);
    }

    public ICraft<TIn, TOut> Trigger(ITrigger next) =>
        new CraftLink<TIn, TOut, TOut>(this, new Unchanged<TOut>(next));

    public ICraft<TIn, TOut> Effect(IEffect<TOut> effect) =>
        new CraftLink<TIn, TOut, TOut>(this, new Unchanged<TOut>(effect));

    public ICraft<TIn, TNext> Craft<TNext>(ICraft<TOut, TNext> next) =>
        new CraftLink<TIn, TOut, TNext>(this, next);
}
