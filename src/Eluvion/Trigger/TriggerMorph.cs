namespace Eluvion.Trigger;

/// <summary>A trigger delegating to the given trigger, implicitly constructible from an Action.</summary>
public sealed class TriggerMorph(ITrigger trigger) : TriggerEnvelope(trigger)
{
    public static implicit operator TriggerMorph(Action trigger) => 
        new(new AsTrigger(trigger));
}