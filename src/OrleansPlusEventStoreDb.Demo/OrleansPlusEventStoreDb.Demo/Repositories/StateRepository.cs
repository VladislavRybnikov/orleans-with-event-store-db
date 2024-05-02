using EventStore.Client;
using OrleansPlusEventStoreDb.Demo.Events;
using OrleansPlusEventStoreDb.Demo.States;

namespace OrleansPlusEventStoreDb.Demo.Repositories;

public class StateRepository<TState>(EventStoreClient eventStoreClient) : IStateRepository<TState> where TState : IState<TState>
{
    private static readonly Type StateType = typeof(TState);
    // ReSharper disable once StaticMemberInGenericType
    private static readonly int? SnapshotPerEventsNumber = (StateType.GetCustomAttributes(typeof(StateSnapshotAttribute), false)
        .FirstOrDefault() as StateSnapshotAttribute)?.SnapshotPerEventsNumber;
    
    public async Task<TState> LoadAsync(string id)
    {
        var state = TState.Empty;
        var streamPosition = StreamPosition.Start;

        if (SnapshotPerEventsNumber != null)
        {
            var snapShotsStream = eventStoreClient.ReadStreamAsync(
                Direction.Backwards,
                $"{StateType.Name}Snapshot-{id}",
                StreamPosition.End,
                maxCount: 1);
            
            if (await snapShotsStream.ReadState == ReadState.Ok)
            {
                var snapshotState = (await snapShotsStream.FirstOrDefaultAsync()).Event.GetFromJson<TState>();
                if (snapshotState != null)
                {
                    state = snapshotState;
                    streamPosition = StreamPosition.FromInt64(snapshotState.Version);
                }
            }
        }

        var stream = eventStoreClient.ReadStreamAsync(
            Direction.Forwards,
            $"{StateType.Name}-{id}",
            streamPosition);

        if (await stream.ReadState == ReadState.Ok)
        {
            var domainEvents = await stream.Select(e => e.Event.MapToDomainEvent()).ToListAsync();
            state.ApplyDomainEvents(domainEvents);
        }

        return state;
    }

    public async Task SaveAsync(string id, int version, TState state, IEnumerable<IDomainEvent> domainEvents)
    {
        var eventsData = domainEvents.Select(x => x.GetEventData()).ToArray();

        if (SnapshotPerEventsNumber is {} snapshotNumber && version != 0 && version % snapshotNumber == 0)
        {
            var expectedSnapshotVersion = (version / snapshotNumber) - 1;
            var snapshotData = state.GetEventData(id);
            
            await eventStoreClient.AppendToStreamAsync(
                $"{StateType.Name}Snapshot-{id}",
                expectedSnapshotVersion == 0 ? StreamRevision.None : StreamRevision.FromInt64(expectedSnapshotVersion - 1), 
                [snapshotData]);
        }

        await eventStoreClient.AppendToStreamAsync(
            $"{StateType.Name}-{id}",
            version == 0 ? StreamRevision.None : StreamRevision.FromInt64(version - 1), 
            eventsData);
    }
}
