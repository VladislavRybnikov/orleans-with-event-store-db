namespace OrleansPlusEventStoreDb.Demo.States;

public interface IState<out TThis> where TThis : IState<TThis>
{
    int Version { get; }

    static abstract TThis Empty { get; }
}