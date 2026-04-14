using Eluvion.Misc;

namespace Eluvion.Seed;

/// <summary>A seed whose value is deserialized from the given JSON text.</summary>
public sealed class SeedFromJson<TSeed>(TextMorph txt) : SeedEnvelope<TSeed>(new AsSeed<TSeed>(() =>
        Task.FromResult(new DeserializedJson<TSeed>(txt).Value())
    )
);