namespace Eluvion.Craft;

/// <summary>A craft built from the given async transformation function.</summary>
public sealed class AsCraft<TIn,TOut>(Func<TIn,Task<TOut>> act) : ICraft<TIn,TOut>
{
    /// <summary>A craft built from the given synchronous transformation function.</summary>
    public AsCraft(Func<TIn,TOut> act) : this(ipt => Task.FromResult(act(ipt)))
    { }

    public async Task<TOut> Yield(TIn ipt) => await act(ipt);

    public ICraft<TIn, TOut> Trigger(ITrigger trigger) =>
        new CraftLink<TIn, TOut, TOut>(this, new Unchanged<TOut>(trigger));

    public ICraft<TIn, TOut> Effect(IEffect<TOut> effect) =>
        new CraftLink<TIn, TOut, TOut>(this, new Unchanged<TOut>(effect));


    public ICraft<TIn, TOutNext> Craft<TOutNext>(ICraft<TOut, TOutNext> craft) =>
        new CraftLink<TIn, TOut, TOutNext>(this, craft);
}