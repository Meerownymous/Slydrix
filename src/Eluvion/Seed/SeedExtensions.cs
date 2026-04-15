using Eluvion.Craft;
using Eluvion.Effect;
using Eluvion.Trigger;

namespace Eluvion.Seed;

public static class SeedExtensions
{
    public static ISeed<TSeed> Trigger<TSeed>(
        this ISeed<TSeed> origin, Action trigger
    ) => origin.Trigger(new AsTrigger(trigger));

    public static ISeed<TSeed> Trigger<TSeed>(
        this ISeed<TSeed> origin, Func<Task> trigger
    ) => origin.Trigger(new AsTrigger(trigger));

    public static ISeed<TSeed> Effect<TSeed>(
        this ISeed<TSeed> origin, Action<TSeed> effect
    ) => origin.Effect(new AsEffect<TSeed>(effect));

    public static ISeed<TSeed> Effect<TSeed>(
        this ISeed<TSeed> origin, Func<TSeed, Task> effect
    ) => origin.Effect(new AsEffect<TSeed>(effect));

    public static ISeed<TCrafted> Craft<TSeed, TCrafted>(
        this ISeed<TSeed> origin, Func<TSeed, TCrafted> craft
    ) => origin.Craft(new AsCraft<TSeed, TCrafted>(craft));

    public static ISeed<TCrafted> Craft<TSeed, TCrafted>(
        this ISeed<TSeed> origin, Func<TSeed, Task<TCrafted>> craft
    ) => origin.Craft(new AsCraft<TSeed, TCrafted>(craft));
}