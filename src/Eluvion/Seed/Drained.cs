namespace Eluvion.Seed;

/// <summary>A seed holding everything a flow spawned.</summary>
public sealed class Drained<T>(IFlow<T> flow, CancellationToken ct = default) : SeedEnvelope<IEnumerable<T>>(async () =>
{
    var drained = new List<T>();
    await foreach (var spawned in flow.Yield(ct))
        drained.Add(spawned);
    return drained;
})
{ }

public static partial class SeedSmarts
{
    /// <summary>A seed holding everything this flow spawns.</summary>
    public static ISeed<IEnumerable<T>> Drained<T>(this IFlow<T> flow, CancellationToken ct = default) =>
        new Drained<T>(flow, ct);
}
