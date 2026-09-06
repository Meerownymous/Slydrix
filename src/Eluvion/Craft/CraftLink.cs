namespace Eluvion.Craft;

/// <summary>Two crafts whose transformations are composed in sequence.</summary>
public sealed class CraftLink<TIn,TInAndOut,TOut>(
    ICraft<TIn,TInAndOut> first,
    ICraft<TInAndOut,TOut> second
) : ICraft<TIn, TOut>
{
    public async Task<TOut> Yield(TIn ipt) => await second.Yield(await first.Yield(ipt));
    public ICraft<TIn, TOut> Trigger(ITrigger trigger) =>
        new CraftLink<TIn, TOut, TOut>(this, new Unchanged<TOut>(trigger));

    public ICraft<TIn, TOut> Effect(IEffect<TOut> effect) =>
        new CraftLink<TIn, TOut, TOut>(this, new Unchanged<TOut>(effect));

    public ICraft<TIn, TOutNext> Craft<TOutNext>(ICraft<TOut, TOutNext> next) =>
        new CraftLink<TIn, TOut, TOutNext>(this, next);
}