using Eluvion.Trigger;

namespace Eluvion.Effect;

/// <summary>An effect delegating to the given effect, implicitly constructible.</summary>
public sealed class EffectMorph<TInput>(IEffect<TInput> effect) : EffectEnvelope<TInput>(effect)
{
    public static implicit operator EffectMorph<TInput>(Func<TInput> effect) => 
        new(new AsEffect<TInput>(effect));
}