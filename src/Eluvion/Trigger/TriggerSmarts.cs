using Eluvion.Craft;
using Eluvion.Effect;

namespace Eluvion.Trigger;

public static partial class TriggerSmarts
{
    public static ITrigger Trigger(this ITrigger origin, Action trigger) => 
        origin.Trigger(new AsTrigger(trigger));
    
    public static ITrigger Trigger(this ITrigger origin, Func<Task> trigger) => 
        origin.Trigger(new AsTrigger(trigger));
    
    public static IEffect<TInput> Effect<TInput>(this ITrigger origin, Action<TInput> effect) => 
        origin.Effect(new AsEffect<TInput>(effect));

    public static IEffect<TInput> Effect<TInput>(this ITrigger origin, Func<TInput, Task> effect) => 
        origin.Effect(new AsEffect<TInput>(effect));
    
    public static ICraft<TInput, TCrafted> Craft<TInput, TCrafted>(
        this ITrigger origin, 
        Func<TInput, TCrafted> craft
    ) => origin.Craft(new AsCraft<TInput, TCrafted>(craft));
    
    public static ICraft<TInput, TCrafted> Craft<TInput, TCrafted>(
        this ITrigger origin, 
        Func<TInput, Task<TCrafted>> craft
    ) => origin.Craft(new AsCraft<TInput, TCrafted>(craft));
}