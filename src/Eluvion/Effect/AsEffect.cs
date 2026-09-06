using Eluvion.Craft;

namespace Eluvion.Effect;

/// <summary>An effect built from the given async function.</summary>
public sealed class AsEffect<TIn>(Func<TIn,Task> act) : IEffect<TIn>
{
    /// <summary>An effect built from the given synchronous action.</summary>
    public AsEffect(Action<TIn> act) : this(ipt =>
    {
        act(ipt);
        return Task.CompletedTask;
    })
    { }

    /// <summary>An effect built from a parameterless async action, ignoring its input.</summary>
    public AsEffect(Func<Task> act) : this(async _ => await act())
    { }

    /// <summary>An effect that calls the given function with its input and discards the result.</summary>
    public AsEffect(Func<TIn> act) : this(_ =>
    {
        act();
        return Task.CompletedTask;
    })
    { }

    /// <summary>An effect that fires the given trigger, ignoring its input.</summary>
    public AsEffect(ITrigger trigger) : this(async _ => await trigger.Act())
    { }

    public async Task Fire(TIn ipt) => await act(ipt);

    public IEffect<TIn> Trigger(ITrigger trigger) =>
        new EffectLink<TIn>(this, new AsEffect<TIn>(trigger));

    public IEffect<TIn> Effect(IEffect<TIn> effect) =>
        new EffectLink<TIn>(
            this,
            new AsEffect<TIn>(async ipt => await effect.Fire(ipt))
        );

    public ICraft<TIn, TOut> Craft<TOut>(ICraft<TIn, TOut> craft) =>
        new Effected<TIn, TOut>(this, craft);

}

