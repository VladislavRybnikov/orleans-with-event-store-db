# EventStore DB Demo
An example of event sourcing implementation on .NET using EventStoreDb.

## :scroll: Versions used:
- .NET 8.0 (C# 12)
    - Microsoft.AspNetCore.OpenApi `8.0.0`
    - Swashbuckle.AspNetCore `6.4.0`
- EventStore DB `24.2.0`
    - EventStore.Client.Grpc.PersistentSubscriptions `23.2.1`
    - EventStore.Client.Grpc.ProjectionManagement `23.2.1`
    - EventStore.Client.Grpc.Streams `23.2.1`

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
To subscribe to the event stream - need to use `eventStoreClient.SubscribeToStream` method and if a stream is a projection - need to provide additional parameter `resolveLinkTos: true`.
```csharp
return eventStoreClient.SubscribeToStream(
    stream, 
    FromStream.Start,
    resolveLinkTos: true,
    cancellationToken: cancellationToken);
```

### Persistent event subscriptions
```csharp
await eventStorePersistentSubscriptionsClient.CreateToStreamAsync(
    stream,
    groupName, 
    new PersistentSubscriptionSettings(resolveLinkTos: true),
    cancellationToken: cancellationToken);
```