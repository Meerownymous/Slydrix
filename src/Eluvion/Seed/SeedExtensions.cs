using Eluvion.Trigger;

namespace Eluvion.Seed;

public static class SeedExtensions
{
    public static ISeed<TSeed> Trigger<TSeed>(this ISeed<TSeed> origin, TriggerMorph triggerMorph) => origin.Trigger(triggerMorph); 
}