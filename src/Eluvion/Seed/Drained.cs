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
