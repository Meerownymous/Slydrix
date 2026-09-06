using Eluvion.Craft;

namespace Eluvion.Effect;

/// <summary>An effect delegating to the given effect.</summary>
public abstract class EffectEnvelope<TIn>(IEffect<TIn> origin) : IEffect<TIn>
{
    /// <summary>An effect delegating to the given async function.</summary>
    public EffectEnvelope(Func<TIn, Task> act) : this(
        new AsEffect<TIn>(async ipt => await act(ipt))
    )
    { }

    /// <summary>An effect delegating to the given synchronous action.</summary>
    public EffectEnvelope(Action<TIn> act) : this(
        new AsEffect<TIn>(ipt =>
        {
            act(ipt);
            return Task.CompletedTask;
        })
    )
    { }
    
    public Task Fire(TIn ipt) => origin.Fire(ipt);
    
    public IEffect<TIn> Trigger(ITrigger trigger) =>
        new EffectLink<TIn>(
            this,
            new AsEffect<TIn>(async _ => await trigger.Act())
        );

    public IEffect<TIn> Effect(IEffect<TIn> effect) => new EffectLink<TIn>(this, effect);

    public ICraft<TIn, TOut> Craft<TOut>(ICraft<TIn, TOut> craft) =>
        new Effected<TIn, TOut>(this, craft);
}

