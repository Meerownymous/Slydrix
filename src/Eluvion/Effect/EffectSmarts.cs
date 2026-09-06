using Eluvion.Craft;
using Eluvion.Trigger;

namespace Eluvion.Effect;

public static partial class EffectSmarts
{
    public static IEffect<TInput> Trigger<TInput>(
        this IEffect<TInput> origin, 
        Action trigger
    ) => origin.Trigger(new AsTrigger(trigger));
    
    public static IEffect<TInput> Trigger<TInput>(
        this IEffect<TInput> origin, 
        Func<Task> trigger
    ) => origin.Trigger(new AsTrigger(trigger));
    
    public static IEffect<TInput> Effect<TInput>(
        this IEffect<TInput> origin, 
        Action<TInput> effect
    ) => origin.Effect(new AsEffect<TInput>(effect));
    
    public static IEffect<TInput> Effect<TInput>(
        this IEffect<TInput> origin, 
        Func<TInput, Task> effect
    ) => origin.Effect(new AsEffect<TInput>(effect));
    
    public static ICraft<TInput, TCrafted> Craft<TInput, TCrafted>(
        this IEffect<TInput> origin, 
        Func<TInput, TCrafted> craft
    ) => origin.Craft(new AsCraft<TInput, TCrafted>(craft));
    
    public static ICraft<TInput, TCrafted> Craft<TInput, TCrafted>(
        this IEffect<TInput> origin, 
        Func<TInput, Task<TCrafted>> craft
    ) => origin.Craft(new AsCraft<TInput, TCrafted>(craft));
}