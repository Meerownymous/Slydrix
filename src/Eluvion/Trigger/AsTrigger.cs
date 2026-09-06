using Eluvion.Craft;
using Eluvion.Effect;

namespace Eluvion.Trigger;

/// <summary>A trigger built from the given async action.</summary>
public sealed class AsTrigger(Func<Task> act) : ITrigger
{
    /// <summary>A trigger built from the given synchronous action.</summary>
    public AsTrigger(Action act) : this(() =>
    {
        act();
        return Task.CompletedTask;
    })
    { }
    public async Task Act() => await act();

    public ITrigger Trigger(ITrigger trigger) => new TriggerLink(this, trigger);

    public IEffect<TIn> Effect<TIn>(IEffect<TIn> effect) =>
        new EffectLink<TIn>(
            new AsEffect<TIn>(_ => Act()),
            effect
        );

    public ICraft<TIn, TOut> Craft<TIn, TOut>(ICraft<TIn, TOut> craft) =>
        new Triggered<TIn, TOut>(this, craft);
}

public static partial class TriggerSmarts
{
    /// <summary>A trigger firing this action.</summary>
    public static ITrigger AsTrigger(this Action act) => new AsTrigger(act);

    /// <summary>A trigger firing this async action.</summary>
    public static ITrigger AsTrigger(this Func<Task> act) => new AsTrigger(act);
}
