using Eluvion.Craft;
using Eluvion.Effect;

namespace Eluvion.Trigger;

/// <summary>Two triggers that fire in sequence.</summary>
public sealed class TriggerLink(ITrigger first, ITrigger second) : ITrigger
{
    public async Task Act()
    {
        await first.Act();
        await second.Act();
    }

    public ITrigger Trigger(ITrigger trigger) => new TriggerLink(this, trigger);

    public IEffect<TIn> Effect<TIn>(IEffect<TIn> effect) =>
        new EffectLink<TIn>(
            new AsEffect<TIn>(async _ => await Act()),
            effect
        );

    public ICraft<TIn, TOut> Craft<TIn, TOut>(ICraft<TIn, TOut> craft) =>
        new Triggered<TIn, TOut>(this, craft);
}