namespace Eluvion.Craft;

/// <summary>A craft whose input has been given to an effect before being transformed.</summary>
public sealed class Effected<TIn, TOut>(IEffect<TIn> effect, ICraft<TIn, TOut> craft) : ICraft<TIn, TOut>
{
    public async Task<TOut> Yield(TIn ipt)
    {
        await effect.Fire(ipt);
        return await craft.Yield(ipt);
    }

    public ICraft<TIn, TOut> Trigger(ITrigger trigger) =>
        new CraftLink<TIn, TOut, TOut>(this, new Unchanged<TOut>(trigger));

    public ICraft<TIn, TOut> Effect(IEffect<TOut> next) =>
        new CraftLink<TIn, TOut, TOut>(this, new Unchanged<TOut>(next));

    public ICraft<TIn, TNext> Craft<TNext>(ICraft<TOut, TNext> next) =>
        new CraftLink<TIn, TOut, TNext>(this, next);
}
