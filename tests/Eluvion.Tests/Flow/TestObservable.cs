namespace Eluvion.Tests.Flow;

/// <summary>An observable that emits on demand, for tests.</summary>
public sealed class TestObservable<T> : IObservable<T>
{
    private readonly List<IObserver<T>> observers = [];
    private bool completed;

    public int SubscriberCount => observers.Count;

    public IDisposable Subscribe(IObserver<T> observer)
    {
        observers.Add(observer);
        return new Unsubscriber(observers, observer);
    }

    public void Emit(T value)
    {
        if (completed) return;
        foreach (var o in observers.ToList())
            o.OnNext(value);
    }

    public void Complete()
    {
        completed = true;
        foreach (var o in observers.ToList())
            o.OnCompleted();
    }

    private sealed class Unsubscriber(List<IObserver<T>> list, IObserver<T> item) : IDisposable
    {
        public void Dispose() => list.Remove(item);
    }
}
