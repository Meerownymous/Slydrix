namespace Eluvion.Craft;

/// <summary>A craft whose output is its input, unchanged by the effect or trigger it fires.</summary>
public sealed class Unchanged<T>(Func<T, Task> act) : ICraft<T, T>
{
    /// <summary>A craft passing its input on after applying the given effect to it.</summary>
    public Unchanged(IEffect<T> effect) : this(effect.Fire)
    { }

    /// <summary>A craft passing its input on after firing the given trigger.</summary>
    public Unchanged(ITrigger trigger) : this(async _ => await trigger.Act())
    { }

    public async Task<T> Yield(T ipt)
    {
        await act(ipt);
        return ipt;
    }

    public ICraft<T, T> Trigger(ITrigger trigger) =>
        new CraftLink<T, T, T>(this, new Unchanged<T>(trigger));

    public ICraft<T, T> Effect(IEffect<T> effect) =>
        new CraftLink<T, T, T>(this, new Unchanged<T>(effect));

    public ICraft<T, TNext> Craft<TNext>(ICraft<T, TNext> craft) =>
        new CraftLink<T, T, TNext>(this, craft);
}
