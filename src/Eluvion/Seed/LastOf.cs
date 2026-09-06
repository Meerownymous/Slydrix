using Tonga;
using Tonga.Optional;

namespace Eluvion.Seed;

/// <summary>A seed holding the last value a flow spawned, if it spawned one.</summary>
public sealed class LastOf<T>(IFlow<T> flow, CancellationToken ct = default) : SeedEnvelope<IOptional<T>>(async () =>
{
    IOptional<T> last = new OptEmpty<T>();
    await foreach (var spawned in flow.Yield(ct))
        last = new OptFull<T>(spawned);
    return last;
})
{ }

public static partial class SeedSmarts
{
    /// <summary>A seed holding the last value this flow spawns, if it spawns one.</summary>
    public static ISeed<IOptional<T>> LastOf<T>(this IFlow<T> flow, CancellationToken ct = default) =>
        new LastOf<T>(flow, ct);
}
