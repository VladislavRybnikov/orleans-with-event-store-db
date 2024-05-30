namespace SimpleEventStoreDb.Demo.States;

public interface IHasEmptyValue<out TThis> where TThis : IState<TThis>
{
    static abstract TThis Empty { get; }
}

public interface IState<out TThis> : IHasEmptyValue<TThis> where TThis : IState<TThis>
{
    int Version { get; }
}