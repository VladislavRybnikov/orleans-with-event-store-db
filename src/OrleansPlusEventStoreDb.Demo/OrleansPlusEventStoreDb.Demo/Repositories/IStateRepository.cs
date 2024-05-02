using OrleansPlusEventStoreDb.Demo.Events;
using OrleansPlusEventStoreDb.Demo.States;

namespace OrleansPlusEventStoreDb.Demo.Repositories;

public interface IStateRepository<TState> where TState : IState<TState>
{
    Task<TState> LoadAsync(string id);

    Task SaveAsync(string id, int version, TState state, IEnumerable<IDomainEvent> domainEvents);
}