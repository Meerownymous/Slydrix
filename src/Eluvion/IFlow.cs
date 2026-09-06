namespace Eluvion;

public interface IFlow<TSpawned>
{
    /// <summary>The values this flow spawns.</summary>
    IAsyncEnumerable<TSpawned> Yield(CancellationToken ct = default);

    /// <summary>This flow with the given trigger fired after every spawned value.</summary>
    IFlow<TSpawned> Trigger(ITrigger trigger);

    /// <summary>This flow with the given effect applied to every spawned value.</summary>
    IFlow<TSpawned> Effect(IEffect<TSpawned> effect);

    /// <summary>This flow with every spawned value transformed through the given craft.</summary>
    IFlow<TNewSpawn> Craft<TNewSpawn>(ICraft<TSpawned, TNewSpawn> craft);
}
