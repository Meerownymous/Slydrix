using Eluvion.Craft;

namespace Eluvion.Effect;

/// <summary>Two effects that act on the same input in sequence.</summary>
public sealed class EffectLink<TIn>(
    IEffect<TIn> first,
    IEffect<TIn> second
) : IEffect<TIn>
{
    public async Task Fire(TIn ipt)
    {
        await first.Fire(ipt);
        await second.Fire(ipt);
    }

    public IEffect<TIn> Trigger(ITrigger trigger) =>
        new EffectLink<TIn>(
            this,
            new AsEffect<TIn>(_ => trigger.Act())
        );

    public IEffect<TIn> Effect(IEffect<TIn> effect) =>
        new EffectLink<TIn>(this, effect);

    public ICraft<TIn, TOut> Craft<TOut>(ICraft<TIn, TOut> craft) =>
        new Effected<TIn, TOut>(this, craft);
}