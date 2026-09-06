using Eluvion.Craft;
using Eluvion.Effect;
using Eluvion.Trigger;

namespace Eluvion.Flow;

public static class FlowExtensions
{
    public static IFlow<T> Trigger<T>(
        this IFlow<T> origin, Action trigger
    ) => origin.Trigger(new AsTrigger(trigger));

    public static IFlow<T> Trigger<T>(
        this IFlow<T> origin, Func<Task> trigger
    ) => origin.Trigger(new AsTrigger(trigger));

    public static IFlow<T> Effect<T>(
        this IFlow<T> origin, Action<T> effect
    ) => origin.Effect(new AsEffect<T>(effect));

    public static IFlow<T> Effect<T>(
        this IFlow<T> origin, Func<T, Task> effect
    ) => origin.Effect(new AsEffect<T>(effect));

    public static IFlow<TSpawned> Craft<T, TSpawned>(
        this IFlow<T> origin, Func<T, TSpawned> craft
    ) => origin.Craft(new AsCraft<T, TSpawned>(craft));

    public static IFlow<TSpawned> Craft<T, TSpawned>(
        this IFlow<T> origin, Func<T, Task<TSpawned>> craft
    ) => origin.Craft(new AsCraft<T, TSpawned>(craft));
}
