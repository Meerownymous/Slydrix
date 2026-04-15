using System.Threading.Channels;

namespace Eluvion.Seed;

/// <summary>Bridges an IObservable source into a channel by forwarding each event to the channel writer.</summary>
public sealed class Observed<T>(ChannelWriter<T> writer) : IObserver<T>
{
    public void OnNext(T value)           => writer.TryWrite(value);
    public void OnCompleted()             => writer.Complete();
    public void OnError(Exception error)  => writer.Complete(error);
}
