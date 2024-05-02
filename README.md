# Orleans with EventStore DB
An example of event sourcing implementation on .NET using Orleans and EventStoreDb.

## :scroll: Versions used:
- .NET 8.0 (C# 12)
- Orleans 
- EventStore DB `v24.2.0`

## :incoming_envelope: Event Sourcing
Event sourcing - is a technique of representing changes of the domain model in the form of sequence of incremental changes (events).

![image](https://github.com/VladislavRybnikov/orleans-with-event-store-db/assets/32033837/ce42f70c-91ca-47a1-a510-8562c8dc2c05)

To get the state of the domain model at specific point of time - need to get the events and apply them one-by-one on the model.

![image](https://github.com/VladislavRybnikov/orleans-with-event-store-db/assets/32033837/a41965d7-33e0-4012-b28a-aae4d3fccc2a)

### Terms:

- Event - immutable information about data changes
- State - aggregated data based on events which represents the state of the domain model in some point of time.
- Stream - a sequence of events grouped by some condition.
- Projection - some data model based on one or multiple streams of events

### Benefits:
- Provide the whole history of data changes which helps to investigate issues
- No need to implement a transactional outbox pattern to deal with distributed transaction of saving changes to the database and publishing an event about changes
- Gives you a better understanding of the domain

## EventStore DB
TBD

### Saving event
```csharp
await eventStoreClient.AppendToStreamAsync(
    $"{StateType.Name}-{id}",
    version == 0 ? StreamRevision.None : StreamRevision.FromInt64(version - 1), 
    eventsData);
```
- The sceond param of the method - is expected version. If it is the first event in the stream than need to pass `StreamRevision.None`

### Reading an event
```csharp
var stream = eventStoreClient.ReadStreamAsync(
    Direction.Forwards,
    $"{StateType.Name}-{id}",
    streamPosition);
```

### Subscribing to event stream
EventStore db has already pre-configured projectinons. One of the is projection by category (`$by-category`). It is enabled by default when passing `--run-projections=All` or it could be enabled programaticaly using `EventStore.Client.Grpc.ProjectionManagement` library.
```csharp
await eventStoreProjectionManagementClient.EnableAsync("$by_category", cancellationToken: stoppingToken);
```

### Persistent event subscriptions

## Actors

## Orleans
TBD

### Terms

- Grain
- Silo

### JournaledGrain
