using Eluvion.Effect;
using Eluvion.Trigger;

namespace Eluvion.Craft;

public static partial class CraftSmarts
{
    public static ICraft<TInput, TCrafted> Trigger<TInput, TCrafted>(
        this ICraft<TInput, TCrafted> origin, Action trigger
    ) => origin.Trigger(new AsTrigger(trigger));
    
    public static ICraft<TInput, TCrafted> Trigger<TInput, TCrafted>(
        this ICraft<TInput, TCrafted> origin, Func<Task> trigger
    ) => origin.Trigger(new AsTrigger(trigger));
    
    public static ICraft<TInput, TCrafted> Effect<TInput, TCrafted>(
        this ICraft<TInput, TCrafted> origin, Action<TCrafted> effect
    ) => origin.Effect(new AsEffect<TCrafted>(effect));
    
    public static ICraft<TInput, TCrafted> Effect<TInput, TCrafted>(
        this ICraft<TInput, TCrafted> origin, Func<TCrafted, Task> effect
    ) => origin.Effect(new AsEffect<TCrafted>(effect));
    
    public static ICraft<TInput, TCraftedFurther> Craft<TInput, TCrafted, TCraftedFurther>(
        this ICraft<TInput, TCrafted> origin, Func<TCrafted, TCraftedFurther> craft
    ) => origin.Craft(new AsCraft<TCrafted, TCraftedFurther>(craft));
    
    public static ICraft<TInput, TCraftedFurther> Craft<TInput, TCrafted, TCraftedFurther>(
        this ICraft<TInput, TCrafted> origin, Func<TCrafted, Task<TCraftedFurther>> craft
    ) => origin.Craft(new AsCraft<TCrafted, TCraftedFurther>(craft));
}