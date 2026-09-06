using Tonga;
using Tonga.Optional;

namespace Eluvion.Seed;

/// <summary>A seed holding the first value a flow spawned, if it spawned one.</summary>
public sealed class FirstOf<T>(IFlow<T> flow, CancellationToken ct = default) : SeedEnvelope<IOptional<T>>(async () =>
{
    await foreach (var spawned in flow.Yield(ct))
        return new OptFull<T>(spawned);
    return new OptEmpty<T>();
})
{ }

public static partial class SeedSmarts
{
    /// <summary>A seed holding the first value this flow spawns, if it spawns one.</summary>
    public static ISeed<IOptional<T>> FirstOf<T>(this IFlow<T> flow, CancellationToken ct = default) =>
        new FirstOf<T>(flow, ct);
}
